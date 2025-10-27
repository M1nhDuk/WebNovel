using InteractionService.Data;
using InteractionService.Models;
using InteractionService.Service.Inteface;
using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.Comment;
using Shareds.DTOs.NovelSeries;

namespace InteractionService.Service
{
    public class CommentService : ICommentService
    {
        private readonly InteracDbContext _context;
        private readonly ILogger<CommentService> _logger;
        // private readonly IHttpClientFactory _httpClientFactory;

        public CommentService(InteracDbContext context, ILogger<CommentService> logger)
        {
            _context = context;
            _logger = logger;
            // _httpClientFactory = httpClientFactory;
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

            if (dto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments.AnyAsync(c => c.CommentId == dto.ParentCommentId.Value);
                if (!parentExists)
                {
                    throw new KeyNotFoundException("Parent comment not found.");
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

            // TODO: Enrich comment with UserName and Avatar by calling AuthService/UserService if needed here or in Get methods

            return MapToDto(newComment); 

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

            // TODO: Enrich replies with UserName and Avatar

            var replyDtos = replies.Select(r => MapToDto(r, r.Replies.Count)).ToList();

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

            _logger.LogInformation("User {UserId} updated comment {CommentId}", userId, commentId);

            // TODO: Enrich with user info if needed
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
                // Gán UserName, UserAvatarThumbnail sau khi enrich
                UserName = c.UserName,
                UserAvatarThumbnail = c.UserAvatarThumbnail
            };
        }
    }
}
