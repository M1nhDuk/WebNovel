using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.UserService;
using System.Security.Claims;
using UserService.Data;
using UserService.Models;
using UserService.UserSettingService.Interface;
using Shareds.DTOs.NovelSeries;


namespace UserService.UserSettingService
{
    public class UserFavoriteService: IUserFavoriteService
    {

        internal class SeriesSummaryDto
        {
            public int SeriesId { get; set; }
            public string? Title { get; set; }
            public string? CoverImage { get; set; }

            public int TotalChapterCount { get; set; }
        }


        private readonly UserDbContext _context;
        private readonly ILogger<UserFavoriteService> _logger;
        private readonly IHttpClientFactory _httpClientFactory; 
        private readonly IConfiguration _configuration;
        private readonly string _publicationServiceUrl;

        public UserFavoriteService(UserDbContext context, ILogger<UserFavoriteService> logger, IHttpClientFactory httpClientFactory, 
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _publicationServiceUrl = configuration["ServiceUrls:NovelService"] ??
                                     throw new InvalidOperationException("ServiceUrls:NovelService is not configured.");
        }


        //Check series có tồn tại hay không 
        public async Task<FavoriteToggleResult> ToggleFavoriteAsync(Guid userId, AddFavoriteDto dto)
        {

            var existing = await _context.UserFavorite
                .FirstOrDefaultAsync(f => f.UserId == userId && f.seriesId == dto.SeriesId);

            if (existing != null)
            {
                //Đã thích -> Bỏ thích
                _context.UserFavorite.Remove(existing);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} UN-favorited Series {SeriesId}", userId, dto.SeriesId);

                return new FavoriteToggleResult { IsFavorited = false, Data = null };
            }
            else
            {
                //Chưa thích -> Yêu thích (Thêm)
                var novelServiceBaseUrl = _configuration["ServiceUrls:NovelService"];
                if(string.IsNullOrEmpty(novelServiceBaseUrl))
                {
                    _logger.LogError("ServiceUrls:NovelService does not config in appsettings.json");
                    throw new InvalidOperationException("Erro system configuration");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var seriesCheckUrl = $"{novelServiceBaseUrl}/api/series/{dto.SeriesId}";

                try
                {
                    var response = await httpClient.GetAsync(seriesCheckUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _logger.LogWarning("User {UserId} toggle {SeriesId} which is not exsits.", userId, dto.SeriesId);
                            throw new KeyNotFoundException($"Series with ID {dto.SeriesId} not exsists.");
                        }
                        else
                        {
                            _logger.LogError("Erro when calling to check {SeriesId}. Status: {StatusCode}", dto.SeriesId, response.StatusCode);
                            throw new HttpRequestException("Cannot verify series.");
                        }
                    }
                    _logger.LogInformation("Valid Series {SeriesId} inside NovelService.", dto.SeriesId);
                } 
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Network error when calling NovelService to check Series {SeriesId}", dto.SeriesId);
                    throw new InvalidOperationException("Erro connection");
                }
            }

                var favorite = new UserFavorite
                {
                    UserId = userId,
                    seriesId = dto.SeriesId,
                    TimeAdded = DateTime.UtcNow,
                    UnreadCount = 0
                };

                _context.UserFavorite.Add(favorite);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} favorited Series {SeriesId}", userId, dto.SeriesId);

                return new FavoriteToggleResult
                {
                    IsFavorited = true,
                    Data = new UserFavoriteDto 
                    {
                        SeriesId = favorite.seriesId,
                        AddedAt = favorite.TimeAdded,
                        UnreadCount = 0
                    }
                };
        }


        public async Task<int> RemoveFavoriteAsync(Guid userId, List<int> seriesIds)
        {
        
            if (seriesIds == null || !seriesIds.Any())
            {
                return 0; 
            }

            var favoriteDelete = await _context.UserFavorite
                .Where(f => f.UserId == userId && seriesIds.Contains(f.seriesId))
                .ToListAsync();

            if (!favoriteDelete.Any())
            {
                _logger.LogWarning("No series to delete.");
                return 0;
            }

            _context.UserFavorite.RemoveRange(favoriteDelete);

            var deletedCount = await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} delete success {Count} favorite series.", userId, deletedCount);

            return deletedCount;
        }


        public async Task<PagedResult<UserFavoriteDto>> GetAllFavoriteAsync(Guid UserId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 20) pageSize = 20; 

            var query = _context.UserFavorite
                .Where(f => f.UserId == UserId)
                .OrderByDescending(f => f.TimeAdded);

            var totalCount = await query.CountAsync();

            var favorites = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserFavoriteDto
                {
                    SeriesId = f.seriesId,
                    AddedAt = f.TimeAdded,
                    LastKnowChapter = f.LastKnownChapterCount
                })
                .ToListAsync();

            await EnrichFavoriteItemsAsync(favorites);

            return new PagedResult<UserFavoriteDto>
            {
                Items = favorites,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }



        //Reset
        public async Task<bool> SyncFavoriteCountsAsync(Guid UserId, List<FavoriteReadUpdateDto> updates)
        {
            var seriesId = updates.Select(f => f.SeriesId).ToList();

            var userFavorite = await _context.UserFavorite
                .Where(f => f.UserId == UserId && seriesId.Contains(f.seriesId))
                .ToListAsync();

            if (!userFavorite.Any()) return false;

            var updatesDict = updates.ToDictionary(u => u.SeriesId);

            foreach (var favorite in userFavorite)
            {
                if (updatesDict.TryGetValue(favorite.seriesId, out var update))
                {
                    // Chỉ cập nhật nếu tiến độ mới cao hơn tiến độ cũ (tránh lùi tiến độ khi đọc lại)
                    if (update.LatestChapterCount > favorite.LastKnownChapterCount)
                    {
                        favorite.LastKnownChapterCount = update.LatestChapterCount;
                        favorite.TimeAdded = DateTime.UtcNow; 
                    }
                }
            }
            _context.UserFavorite.UpdateRange(userFavorite);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} synced chapter counts for {Count} favorites", UserId, userFavorite.Count);
            return true;
        }


        private async Task EnrichFavoriteItemsAsync(List<UserFavoriteDto> favoriteItems)
        {
            if (!favoriteItems.Any()) return;

            var seriesIds = favoriteItems.Select(h => h.SeriesId).Distinct().ToList();
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
                    foreach (var item in favoriteItems)
                    {
                        if (summaryLookup.TryGetValue(item.SeriesId, out var summary))
                        {
                            item.SeriesTitle = summary.Title;
                            item.SeriesCoverImage = summary.CoverImage;
                            item.CurrentChapterCount = summary.TotalChapterCount;
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
                    foreach (var item in favoriteItems) item.SeriesTitle = "[Details Unavailable]";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching reading history from PublicationService. URL: {Url}", enrichmentUrl);
                foreach (var item in favoriteItems)
                {
                    item.SeriesTitle = "[Error Fetching Details]";
                }
            }
        }



    }
}
