using Shareds.DTOs.UserService;

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
        Task<List<UserFavoriteDto>> GetAllFavoriteAsync(Guid UserId);
        Task<bool> SyncFavoriteCountsAsync(Guid UserId, List<FavoriteReadUpdateDto> updates);
    }
}
