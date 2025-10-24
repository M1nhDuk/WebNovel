using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.UserService;
using System.Security.Claims;
using UserService.Data;
using UserService.Models;
using UserService.UserSettingService.Interface;

namespace UserService.UserSettingService
{
    public class UserFavoriteService: IUserFavoriteService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<UserFavoriteService> _logger;
        private readonly IHttpClientFactory _httpClientFactory; 
        private readonly IConfiguration _configuration;

        public UserFavoriteService(UserDbContext context, ILogger<UserFavoriteService> logger, IHttpClientFactory httpClientFactory, 
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }


        //Check series có tồn tại hay không 
        public async Task<FavoriteToggleResult> ToggleFavoriteAsync(Guid userId, AddFavoriteDto dto)
        {

            var existing = await _context.UserFavorite
                .FirstOrDefaultAsync(f => f.UserId == userId && f.seriesId == dto.SeriesId);

            if (existing != null)
            {
                // TÌNH HUỐNG 1: Đã thích -> Bỏ thích (Xóa)
                _context.UserFavorite.Remove(existing);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} UN-favorited Series {SeriesId}", userId, dto.SeriesId);

                return new FavoriteToggleResult { IsFavorited = false, Data = null };
            }
            else
            {
                // TÌNH HUỐNG 2: Chưa thích -> Yêu thích (Thêm)
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
                            throw new KeyNotFoundException($"Series với ID {dto.SeriesId} không tồn tại.");
                        }
                        else
                        {
                            _logger.LogError("Erro when calling to check {SeriesId}. Status: {StatusCode}", dto.SeriesId, response.StatusCode);
                            throw new HttpRequestException("Không thể xác minh thông tin truyện.");
                        }
                    }
                    _logger.LogInformation("Valid Series {SeriesId} inside NovelService.", dto.SeriesId);
                } 
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Network error when calling NovelService to check Series {SeriesId}", dto.SeriesId);
                    throw new InvalidOperationException("Không thể kết nối đến dịch vụ truyện.");
                }
            }

                var favorite = new UserFavorite
                {
                    UserId = userId,
                    seriesId = dto.SeriesId,
                    TimeAdded = DateTime.UtcNow,
                    LastKnownChapterCount = dto.CurrentChapterCount // Lưu số chương lúc bấm thích
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
                        LastKnowChapter = favorite.LastKnownChapterCount
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


        //Phân trang, FE gọi series Id để lấy dữ liệu hiển thị
        public async Task<List<UserFavoriteDto>> GetAllFavoriteAsync(Guid UserId)
        {
            var favorites = await _context.UserFavorite
                .Where(f => f.UserId == UserId)
                .OrderByDescending(f => f.TimeAdded)
                .Select(f => new UserFavoriteDto
                {
                    SeriesId = f.seriesId,
                    AddedAt = f.TimeAdded,
                    LastKnowChapter = f.LastKnownChapterCount
                })
                .ToListAsync();

            return favorites;
        }
        


        //Reset
        public async Task<bool> SyncFavoriteCountsAsync(Guid UserId, List<FavoriteReadUpdateDto> updates)
        {
            var seriesId = updates.Select(f => f.SeriesId).ToList();

            var userFavorite = await _context.UserFavorite.Where(f => f.UserId == UserId && seriesId.Contains(f.seriesId))
                .ToListAsync();

            if (!userFavorite.Any()) return false;

            var updatesDict = updates.ToDictionary(u => u.SeriesId);

            foreach (var favorite in userFavorite)
            {
                if (updatesDict.TryGetValue(favorite.seriesId, out var update))
                {
                    favorite.LastKnownChapterCount = update.LatestChapterCount;
                }
            }

            _context.UserFavorite.UpdateRange(userFavorite);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} synced chapter counts for {Count} favorites", UserId, userFavorite.Count);
            return true;
        }
    }
}
