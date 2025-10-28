using Shareds.DTOs.UserService;

namespace UserService.UserSettingService.Interface
{
    public interface IBookmarkService
    {
        Task<BookmarkToggleResultDto> ToggleBookmarkAsync(Guid userId, ToggleBookmarkDto dto);
        Task<bool> RemoveBookmarkForChapterAsync(Guid userId, int chapterId);
        Task<List<BookmarkDto>> GetGroupedBookmarksByUserAsync(Guid userId);
    }
}
