using InteractionService.Data;
using InteractionService.Models;
using InteractionService.Service.Inteface;
using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.AuthService;
using Shareds.DTOs.Comment;
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService;
using System.Net.Http;
using System.Net.Http.Json;

namespace InteractionService.Service
{
    public class CommentService : ICommentService
    {
        private readonly InteracDbContext _context;
        private readonly ILogger<CommentService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public CommentService(InteracDbContext context, ILogger<CommentService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private async Task EnrichCommentsWithUserInfo(List<CommentDto> comments)
        {
            if (comments == null || !comments.Any())
            {
                return;
            }

            // 1. Lấy danh sách UserId không trùng lặp
            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            if (!userIds.Any())
            {
                return;
            }

            // 2. Tạo HttpClient
            // Sử dụng named client đã cấu hình
            var httpClient = _httpClientFactory.CreateClient("AuthServiceClient");


            // 3. Build URL request
            var idsQueryParam = string.Join(",", userIds);
            var requestUrl = $"api/internal/users/batch?ids={idsQueryParam}"; // Path tương đối nếu dùng BaseAddress

            // 4. Gọi API nội bộ
            List<UserInfoDto>? usersInfo = null;
            try
            {
                usersInfo = await httpClient.GetFromJsonAsync<List<UserInfoDto>>(requestUrl);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when calling AuthService batch endpoint. URL: {Url}", httpClient.BaseAddress + requestUrl);
          
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing response from AuthService batch endpoint. URL: {Url}", httpClient.BaseAddress + requestUrl);
                return;
            }


            // 5. Map thông tin User vào CommentDto
            if (usersInfo != null && usersInfo.Any())
            {
                var userInfoDict = usersInfo.ToDictionary(u => u.UserId);
                foreach (var comment in comments)
                {
                    if (userInfoDict.TryGetValue(comment.UserId, out var userInfo))
                    {
                        comment.UserName = userInfo.UserName;
                        comment.UserAvatarThumbnail = userInfo.AvatarThumbnail;
                    }
                    else
                    {
                 
                        comment.UserName = "[Deleted User]"; 
                        comment.UserAvatarThumbnail = null; 
                        _logger.LogWarning("User info not found for UserId {UserId} during comment enrichment", comment.UserId);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Received empty or null user info list from AuthService for user IDs: {UserIds}", idsQueryParam);
                foreach (var comment in comments)
                {
                    comment.UserName = "[Unknown User]";
                    comment.UserAvatarThumbnail = null;
                }
            }
        }


        public async Task<CommentDto> CreateCommentAsync(Guid userId, int? seriesId, int? chapterId, CreateCommentDto dto)
        {
            if (seriesId.HasValue == chapterId.HasValue)
            {
                throw new ArgumentException("Comment must belong to exactly one Series OR one Chapter.");
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new ArgumentException("Comment content cannot be empty.");
            }

            Guid? parentCommentUserId = null; // ID của người bị reply
            Guid? contentAuthorId = null;

            if (dto.ParentCommentId.HasValue) //Reply
            {
                var parentComment = await _context.Comments.FindAsync(dto.ParentCommentId.Value);
                if (parentComment == null)
                {
                    throw new KeyNotFoundException("Parent comment not found.");
                }
                if (parentComment.SeriesId != seriesId || parentComment.ChapterId != chapterId)
                {
                    throw new ArgumentException("Reply target does not match parent comment target.");
                }

                // Lấy UserId của người viết comment gốc để gửi thông báo
                parentCommentUserId = parentComment.UserId;
            }
            else //comment gốc mới
            {
                //lấy uploader_id từ NovelService
                contentAuthorId = await GetContentAuthorId(seriesId, chapterId);
                if (!contentAuthorId.HasValue)
                {
                    _logger.LogWarning("Could not find author for SeriesId={SeriesId} or ChapterId={ChapterId}. Cannot send notification.", seriesId, chapterId);
                    throw new InvalidOperationException("Could not determine content author."); 
                }
            }

            var newComment = new Comment
            {
                UserId = userId,
                CommentText = dto.Content,
                CreatedAt = DateTime.UtcNow,
                SeriesId = seriesId,
                ChapterId = chapterId,
                ParentCommentId = dto.ParentCommentId
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} created comment {CommentId} for {TargetType} {TargetId}",
                userId, newComment.CommentId, seriesId.HasValue ? "Series" : "Chapter", seriesId ?? chapterId);

            string? linkUrl = seriesId.HasValue ? $"/series/{seriesId}" : $"/chapters/{chapterId}";

            if (parentCommentUserId.HasValue && parentCommentUserId.Value != userId) 
            {
                // lấy UserName của người vừa reply bằng userId để đưa vào message
                var commenterInfo = await GetUserInfo(userId); // Lấy thông tin người comment 
                var commenterName = commenterInfo?.UserName ?? "Someone";

                var notificationDto = new CreateNotificationDto
                {
                    UserId = parentCommentUserId.Value,
                    Type = "NewComment",
                    Message = $"{commenterName} replied to your comment.",
                    LinkUrl = linkUrl + $"#comment-{newComment.CommentId}" // Link tới comment mới
                };
                await SendNotificationAsync(notificationDto);
            }


            //User B comment vào bài của User A
            else if (contentAuthorId.HasValue && contentAuthorId.Value != userId) //comment gốc
            {

                // Cần lấy UserName của người vừa comment 
                var commenterInfo = await GetUserInfo(userId); // Lấy thông tin người comment
                var commenterName = commenterInfo?.UserName ?? "Someone";
                var targetName = seriesId.HasValue ? "your series" : "your chapter";

                var notificationDto = new CreateNotificationDto
                {
                    UserId = contentAuthorId.Value, // Người nhận là User A (tác giả)
                    Type = "NewComment",
                    Message = $"{commenterName} commented on {targetName}.",
                    LinkUrl = linkUrl + $"#comment-{newComment.CommentId}" // Link tới comment mới
                };
                await SendNotificationAsync(notificationDto);
            }

            var commentDto = MapToDto(newComment);
            await EnrichCommentsWithUserInfo(new List<CommentDto> { commentDto });
            return commentDto;

        }

        public async Task<PagedResult<CommentDto>> GetCommentsAsync(int? seriesId, int? chapterId, int pageNumber = 1, int pageSize = 20)
        {
            if (seriesId.HasValue == chapterId.HasValue)
            {
                throw new ArgumentException("Must specify either SeriesId OR ChapterId.");
            }

            var query = _context.Comments.Where(c => c.ParentComment == null);

            if (seriesId.HasValue)
            {
                query = query.Where(c => c.SeriesId == seriesId.Value);
            }
            else
            {
                query = query.Where(c => c.ChapterId == chapterId.Value);
            }

            var totalCount = await query.CountAsync();

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Replies)
                .ToListAsync();

            var commentDtos = comments.Select(c => MapToDto(c, c.Replies.Count)).ToList();

            await EnrichCommentsWithUserInfo(commentDtos);

            return new PagedResult<CommentDto>
            {
                Items = commentDtos,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<CommentDto>> GetRepliesAsync(Guid parentCommentId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Comments
                              .Where(c => c.ParentCommentId == parentCommentId);

            var totalCount = await query.CountAsync();

            var replies = await query
                .OrderBy(c => c.CreatedAt) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Replies) 
                .ToListAsync();


            var replyDtos = replies.Select(r => MapToDto(r, r.Replies.Count)).ToList();

            await EnrichCommentsWithUserInfo(replyDtos);

            return new PagedResult<CommentDto>
            {
                Items = replyDtos,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<CommentDto?> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentDto dto)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return null; 
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to edit this comment.");
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new ArgumentException("Comment content cannot be empty.");
            }

            comment.CommentText = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            var updatedDto = MapToDto(comment);

            // Enrich thông tin user
            await EnrichCommentsWithUserInfo(new List<CommentDto> { updatedDto });

            _logger.LogInformation("User {UserId} updated comment {CommentId}", userId, commentId);

      
            return MapToDto(comment);
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return false;
            }

            if (comment.UserId != userId /* && !User.IsInRole("Admin") */)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this comment.");
            }

  
            _context.Comments.Remove(comment);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("User {UserId} deleted comment {CommentId}", userId, commentId);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCommentsBySeriesAsync(int seriesId)
        {
            var commentsToDelete = await _context.Comments.Where(c => c.SeriesId == seriesId).ToListAsync();

            if(!commentsToDelete.Any())
            {
                return true;
            }

            _context.Comments.RemoveRange(commentsToDelete);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} comments for SeriesId {SeriesId}", result, seriesId);

            return result > 0;
        }

        public async Task<bool> DeleteCommentsByChapterAsync(int chapterId)
        {
            var commentsToDelete = await _context.Comments.Where(c => c.ChapterId == chapterId).ToListAsync();

            if (!commentsToDelete.Any())
            {
                return true;
            }

            _context.Comments.RemoveRange(commentsToDelete);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} comments for ChapterId {ChapterId}", result, chapterId);

            return result > 0;

        }



        //lấy Uploader ID từ NovelService
        private async Task<Guid?> GetContentAuthorId(int? seriesId, int? chapterId)
        {
            var httpClient = _httpClientFactory.CreateClient("NovelServiceClient");
            string requestUrl;

            if (seriesId.HasValue)
            {
                requestUrl = $"api/internal/publication/series/{seriesId.Value}/uploader";
            }
            else if (chapterId.HasValue)
            {
                requestUrl = $"api/internal/publication/chapters/{chapterId.Value}/uploader";
            }
            else
            {
                return null;
            }

            try
            {
                var response = await httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Guid>();
                }
                else
                {
                    _logger.LogWarning("Failed to get author ID from PublicationService. URL: {Url}, Status: {StatusCode}", httpClient.BaseAddress + requestUrl, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PublicationService to get author ID. URL: {Url}", httpClient.BaseAddress + requestUrl);
                return null;
            }
        }



        //Helper để gửi thông báo đến UserService
        private async Task SendNotificationAsync(CreateNotificationDto dto)
        {
            var userServiceUrl = _configuration["ServiceUrls:UserService"];

            if (string.IsNullOrEmpty(userServiceUrl))
            {
                _logger.LogError("ServiceUrls:UserService is not configured. Cannot send notification.");
                return;
            }

            var httpClient = _httpClientFactory.CreateClient();
            var notificationUrl = $"{userServiceUrl}/api/internal/notifications";

            try
            {
                var response = await httpClient.PostAsJsonAsync(notificationUrl, dto);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send notification via UserService for UserId {UserId}. Status: {StatusCode}, Reason: {Reason}", dto.UserId, response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                else
                {
                    _logger.LogInformation("Successfully sent notification type {Type} to UserId {UserId}", dto.Type, dto.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification via UserService for UserId {UserId}", dto.UserId);
            }
        }


        //Helper để lấy thông tin user
        private async Task<UserInfoDto?> GetUserInfo(Guid userId)
        {
            var httpClient = _httpClientFactory.CreateClient("AuthServiceClient");
            var requestUrl = $"api/internal/users/batch?ids={userId}";
            try
            {
                var usersInfo = await httpClient.GetFromJsonAsync<List<UserInfoDto>>(requestUrl);
                return usersInfo?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user info for UserId {UserId} from AuthService", userId);
                return null;
            }
        }


        private CommentDto MapToDto(Comment c, int replyCount = 0)
        {
            return new CommentDto
            {
                CommentId = c.CommentId,
                UserId = c.UserId,
                Content = c.CommentText,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                SeriesId = c.SeriesId,
                ChapterId = c.ChapterId,
                ParentCommentId = c.ParentCommentId,
                ReplyCount = replyCount,
                UserName = c.UserName,
                UserAvatarThumbnail = c.UserAvatarThumbnail
            };
        }
    }
}
