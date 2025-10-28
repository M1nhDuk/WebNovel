using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService;

namespace UserService.UserSettingService.Interface
{
    public interface IBookmarkService
    {
        Task<BookmarkToggleResultDto> ToggleBookmarkAsync(Guid userId, ToggleBookmarkDto dto);
        Task<bool> RemoveBookmarkForChapterAsync(Guid userId, int chapterId);
        Task<PagedResult<BookmarkDto>> GetGroupedBookmarksByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<BookmarkDto?> GetBookmarkForChapterAsync(Guid userId, int chapterId);
        Task<bool> RemoveBookmarkByIdAsync(Guid userId, Guid bookmarkId);

    }
}
