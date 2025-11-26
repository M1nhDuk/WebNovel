using InteractionService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService;
using Shareds.DTOs;
using UserService.Data;
using UserService.Migrations;
using UserService.UserSettingService.Interface;

namespace UserService.UserSettingService
{
    public class BookmarkService: IBookmarkService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<BookmarkService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _publicationServiceUrl;

        public BookmarkService(UserDbContext context, ILogger<BookmarkService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _publicationServiceUrl = configuration["ServiceUrls:NovelService"] ??
                                     throw new InvalidOperationException("ServiceUrls:NovelService is not configured.");
        }

        public async Task<BookmarkToggleResultDto> ToggleBookmarkAsync(Guid userId, ToggleBookmarkDto dto)
        {
            var existingBookmark = await _context.UserBookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ChapterId == dto.ChapterId);

            UserBookmark bookmarkToReturn;

            if (existingBookmark != null)
            {
                //Đã bookmark chương này -> Cập nhật lại vị trí/ghi chú
                existingBookmark.LocationIdentifier = dto.LocationIdentifier;
                existingBookmark.ContextSnippet = dto.ContextSnippet;
                existingBookmark.CreatedAt = DateTime.UtcNow;
                existingBookmark.SeriesId = dto.SeriesId; 

                _context.UserBookmarks.Update(existingBookmark);
                bookmarkToReturn = existingBookmark;

            }
            else
            {
                // Chưa bookmark chương này -> Tạo mới
                bool isValidReference = await ValidatePublicationReference(dto.SeriesId, dto.ChapterId);
                if (!isValidReference)
                {
                    throw new KeyNotFoundException("Referenced Series or Chapter does not exist.");
                }

                var newBookmark = new UserBookmark
                {
                    UserId = userId,
                    SeriesId = dto.SeriesId,
                    ChapterId = dto.ChapterId,
                    LocationIdentifier = dto.LocationIdentifier,
                    ContextSnippet = dto.ContextSnippet,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserBookmarks.Add(newBookmark);
                bookmarkToReturn = newBookmark;
            }

            await _context.SaveChangesAsync();

            var resultDto = MapToDto(bookmarkToReturn);

            return new BookmarkToggleResultDto
            {
                IsBookmarked = true,
                Data = resultDto
            };
        }


        public async Task<bool> RemoveBookmarkForChapterAsync(Guid userId, int chapterId)
        {
            // Tìm bookmark của đúng chương này
            var bookmark = await _context.UserBookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ChapterId == chapterId);

            if (bookmark == null)
            {
                _logger.LogWarning("No bookmark found for User {UserId} in Chapter {ChapterId} to remove.", userId, chapterId);
                return false;
            }

            //Xóa Bookmark trg db
            _context.UserBookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} removed bookmark for Chapter {ChapterId}", userId, chapterId);

            return true;
        }


        public async Task<bool> RemoveBookmarkByIdAsync(Guid userId, Guid bookmarkId)
        {
            var bookmark = await _context.UserBookmarks
                .FirstOrDefaultAsync(b => b.BookmarkId == bookmarkId && b.UserId == userId);

            if (bookmark == null)
            {
                return false;
            }

            _context.UserBookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();


            _logger.LogInformation("User {UserId} removed bookmark {BookmarkId} by its ID", userId, bookmarkId);
            return true;
        }


        public async Task<BookmarkDto?> GetBookmarkForChapterAsync(Guid userId, int chapterId)
        {
            var bookmark = await _context.UserBookmarks
                .AsNoTracking() 
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ChapterId == chapterId);

            if (bookmark == null)
            {
                return null;
            }

            var dto = MapToDto(bookmark);
            await EnrichBookmarksAsync(new List<BookmarkDto> { dto });
            return dto;
        }


        public async Task<PagedResult<BookmarkDto>> GetGroupedBookmarksByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
           
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 20) pageSize = 20; 

        
            var query = _context.UserBookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt); 

      
            var totalCount = await query.CountAsync();

     
            var bookmarks = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

       
            var dtos = bookmarks.Select(MapToDto).ToList();

          
            await EnrichBookmarksAsync(dtos);

            return new PagedResult<BookmarkDto>
            {
                Items = dtos,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }




        //Helper

        private async Task<bool> ValidatePublicationReference(int seriesId, int chapterId)
        {
            var httpClient = _httpClientFactory.CreateClient(); 
            var validationUrl = $"{_publicationServiceUrl}/api/internal/publication/validate/series/{seriesId}/chapter/{chapterId}";

            try
            {
                var response = await httpClient.GetAsync(validationUrl);
                if (response.IsSuccessStatusCode) return true;
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return false;

                _logger.LogError("Validation request to PublicationService failed. URL: {Url}, Status: {StatusCode}", validationUrl, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PublicationService for validation. URL: {Url}", validationUrl);
                return false;
            }
        }




        private async Task EnrichBookmarksAsync(List<BookmarkDto> bookmarks)
        {
            if (!bookmarks.Any()) return;

            var detailsNeeded = bookmarks
                .Select(b => new PublicationDetailRequestItem { SeriesId = b.SeriesId, ChapterId = b.ChapterId })
                .DistinctBy(p => new { p.SeriesId, p.ChapterId })
                .ToList();

            if (!detailsNeeded.Any()) return;

            var httpClient = _httpClientFactory.CreateClient();
            var enrichmentUrl = $"{_publicationServiceUrl}/api/internal/publication/batch-details";

            try
            {
                var response = await httpClient.PostAsJsonAsync(enrichmentUrl, detailsNeeded);
                response.EnsureSuccessStatusCode();

                var details = await response.Content.ReadFromJsonAsync<List<PublicationDetailResponseItem>>();

                if (details == null || !details.Any())
                {
                    _logger.LogWarning("Received no details from PublicationService batch endpoint for {Count} requests.", detailsNeeded.Count);
                    return;
                }

                var detailsLookup = details.ToDictionary(d => (d.SeriesId, d.ChapterId));

                foreach (var bookmark in bookmarks)
                {
                    if (detailsLookup.TryGetValue((bookmark.SeriesId, bookmark.ChapterId), out var detail))
                    {
                        bookmark.SeriesTitle = detail.SeriesTitle;
                        bookmark.SeriesCoverImage = detail.SeriesCoverImage;
                        bookmark.ChapterTitle = detail.ChapterTitle;
                        bookmark.ChapterNumber = detail.ChapterNumber;
                    }
                    else
                    {
                        _logger.LogWarning("Could not find enrichment details for Series {SeriesId}, Chapter {ChapterId}", bookmark.SeriesId, bookmark.ChapterId);
                        bookmark.SeriesTitle = "[Series Not Found]";
                        bookmark.ChapterTitle = "[Chapter Not Found]";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching bookmarks from PublicationService. URL: {Url}", enrichmentUrl);
                foreach (var bookmark in bookmarks)
                {
                    bookmark.SeriesTitle = "[Error Fetching Details]";
                    bookmark.ChapterTitle = "[Error Fetching Details]";
                }
            }
        }

        private BookmarkDto MapToDto(UserBookmark b)
        {
            return new BookmarkDto
            {
                BookmarkId = b.BookmarkId,
                SeriesId = b.SeriesId,
                ChapterId = b.ChapterId,
                LocationIdentifier = b.LocationIdentifier,
                ContextSnippet = b.ContextSnippet,
                CreatedAt = b.CreatedAt
            };
        }
    }

}
