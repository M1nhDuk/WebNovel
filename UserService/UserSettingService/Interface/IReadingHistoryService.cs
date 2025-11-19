using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService.ReadingHistory;

namespace UserService.UserSettingService.Interface
{
    public interface IReadingHistoryService
    {
        Task AddOrUpdateHistoryAsync(Guid userId, AddReadingHistoryDto dto);
        Task<PagedResult<ReadingHistoryDto>> GetHistoryAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> RemoveHistoryAsync(Guid userId, List<Guid> historyIds);

        Task<List<int>> GetReadChapterIdsAsync(Guid userId, int seriesId);
    }
}
