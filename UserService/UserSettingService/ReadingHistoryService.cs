using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService.ReadingHistory;
using UserService.Data;
using UserService.UserSettingService.Interface;
using UserService.Models;

namespace UserService.UserSettingService
{
    public class SeriesSummaryDto
    {
        public int SeriesId { get; set; }
        public string? Title { get; set; }
        public string? CoverImage { get; set; }
    }

    public class ReadingHistoryService : IReadingHistoryService
    {
     
            private readonly UserDbContext _context;
            private readonly ILogger<ReadingHistoryService> _logger;
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IConfiguration _configuration;
            private readonly string _publicationServiceUrl;


            public ReadingHistoryService(UserDbContext context, ILogger<ReadingHistoryService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _context = context;
                _logger = logger;
                _httpClientFactory = httpClientFactory;
                _configuration = configuration;
                _publicationServiceUrl = configuration["ServiceUrls:NovelService"] ??
                                         throw new InvalidOperationException("ServiceUrls:NovelService is not configured.");
            }


        public async Task<List<int>> GetReadChapterIdsAsync(Guid userId, int seriesId)
        {
            return await _context.UserReadChapter
                .Where(rh => rh.UserId == userId && rh.SeriesId == seriesId)
                .Select(rh => rh.ChapterId)
                .ToListAsync();
        }


        public async Task AddOrUpdateHistoryAsync(Guid userId, AddReadingHistoryDto dto)
        {
            
            var hasRead = await _context.UserReadChapter
                .AnyAsync(x => x.UserId == userId
                            && x.SeriesId == dto.SeriesId
                            && x.ChapterId == dto.ChapterId);

            if (!hasRead)
            {
                _context.UserReadChapter.Add(new UserReadChapter
                {
                    UserId = userId,
                    SeriesId = dto.SeriesId,
                    ChapterId = dto.ChapterId,
                    ReadAt = DateTime.UtcNow
                });

                
                var favorite = await _context.UserFavorite
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.seriesId == dto.SeriesId);

                if (favorite != null && favorite.UnreadCount > 0)
                {
                    favorite.UnreadCount -= 1;

                    if (favorite.UnreadCount < 0) favorite.UnreadCount = 0;
                }
            }

            
            var existingProgress = await _context.ReadingHistories
                .FirstOrDefaultAsync(rh => rh.UserId == userId && rh.SeriesId == dto.SeriesId);

            if (existingProgress != null)
            {
                existingProgress.ChapterId = dto.ChapterId;
                existingProgress.LastAccessedAt = DateTime.UtcNow;
                _context.ReadingHistories.Update(existingProgress);
            }
            else
            {
                _context.ReadingHistories.Add(new ReadingHistory
                {
                    UserId = userId,
                    SeriesId = dto.SeriesId,
                    ChapterId = dto.ChapterId,
                    LastAccessedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }



        public async Task<PagedResult<ReadingHistoryDto>> GetHistoryAsync(Guid userId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 20) pageSize = 20;

            var query = _context.ReadingHistories
                .AsNoTracking()
                .Where(rh => rh.UserId == userId)
                .OrderByDescending(rh => rh.LastAccessedAt);

            var totalCount = await query.CountAsync();

            var historyEntries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(rh => new ReadingHistoryDto
                {
                    HistoryId = rh.HistoryId,
                    SeriesId = rh.SeriesId,
                    LastAccessedAt = rh.LastAccessedAt,
                    ChapterId = rh.ChapterId 
                })
                .ToListAsync();

            await EnrichHistoryItemsAsync(historyEntries);

            return new PagedResult<ReadingHistoryDto>
            {
                Items = historyEntries,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<int> RemoveHistoryAsync(Guid userId, List<Guid> historyIds)
            {
                if (historyIds == null || !historyIds.Any())
                {
                    return 0;
                }

                var entriesToDelete = await _context.ReadingHistories
                    .Where(rh => rh.UserId == userId && historyIds.Contains(rh.HistoryId))
                    .ToListAsync();

                if (!entriesToDelete.Any())
                {
                    _logger.LogWarning("No reading history entries found to delete for User {UserId} with provided IDs.", userId);
                    return 0;
                }

                _context.ReadingHistories.RemoveRange(entriesToDelete);
                var deletedCount = await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} deleted {Count} reading history entries.", userId, deletedCount);
                return deletedCount;
            }


        //Helper
            private async Task EnrichHistoryItemsAsync(List<ReadingHistoryDto> historyItems)
            {
                if (!historyItems.Any()) return;

                var seriesIds = historyItems.Select(h => h.SeriesId).Distinct().ToList();
                if (!seriesIds.Any()) return;

                var httpClient = _httpClientFactory.CreateClient();
                var enrichmentUrl = $"{_publicationServiceUrl}/api/internal/publication/batch-series-summary";

                try
                {
                    var response = await httpClient.PostAsJsonAsync(enrichmentUrl, seriesIds);
                    response.EnsureSuccessStatusCode(); 

                    var seriesSummaries = await response.Content.ReadFromJsonAsync<List<SeriesSummaryDto>>();

                    if (seriesSummaries != null && seriesSummaries.Any())
                    {
                        var summaryLookup = seriesSummaries.ToDictionary(s => s.SeriesId);
                        foreach (var item in historyItems)
                        {
                            if (summaryLookup.TryGetValue(item.SeriesId, out var summary))
                            {
                                item.SeriesTitle = summary.Title;
                                item.SeriesCoverImage = summary.CoverImage;
                            }
                            else
                            {
                                item.SeriesTitle = "[Series Not Found]";
                                _logger.LogWarning("Could not find summary details for Series {SeriesId}", item.SeriesId);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Received no summary details from PublicationService for {Count} series IDs.", seriesIds.Count);
                        foreach (var item in historyItems) item.SeriesTitle = "[Details Unavailable]";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enriching reading history from PublicationService. URL: {Url}", enrichmentUrl);       
                    foreach (var item in historyItems)
                    {
                        item.SeriesTitle = "[Error Fetching Details]";
                    }
                }
            }
        }
    }
