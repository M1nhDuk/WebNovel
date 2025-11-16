using Shareds.DTOs.UserService;
using Shareds.DTOs.NovelSeries;

namespace UserService.UserSettingService.Interface
{
    public class FavoriteToggleResult
    {
        public bool IsFavorited { get; set; }
        public UserFavoriteDto? Data { get; set; } 
    }

    public interface IUserFavoriteService
    {
        Task<FavoriteToggleResult> ToggleFavoriteAsync(Guid userId, AddFavoriteDto dto);
        Task<int> RemoveFavoriteAsync(Guid UserId, List<int> seriesIds);
        Task<PagedResult<UserFavoriteDto>> GetAllFavoriteAsync(Guid UserId, int pageNumber, int pageSize);
        Task<bool> SyncFavoriteCountsAsync(Guid UserId, List<FavoriteReadUpdateDto> updates);
    }
}
