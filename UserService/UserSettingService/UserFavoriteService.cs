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

        public UserFavoriteService(UserDbContext context, ILogger<UserFavoriteService> logger)
        {
            _context = context;
            _logger = logger;
        }

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

            _logger.LogInformation("User {UserId} đã xóa thành công {Count} mục yêu thích.", userId, deletedCount);

            return deletedCount;
        }


        //Phân trang, FE gọi series Id để lấy dữ liệu hiển thị
        public async Task<List<UserFavoriteDto>> GetAllFavoriteAsync(Guid UserId)
        {
            var favorites = await _context.UserFavorite
                .Where(f => f.UserId == UserId)
                .OrderByDescending(f => f.TimeAdded)
                .Select(f => MapToDto(f))
                .ToListAsync();

            return favorites;
        }
        

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

        private UserFavoriteDto MapToDto(UserFavorite favorite)
        {
            return new UserFavoriteDto
            {
                SeriesId = favorite.seriesId,
                AddedAt = favorite.TimeAdded,
                LastKnowChapter = favorite.LastKnownChapterCount
            };
        }
    }
}
