using Shareds.DTOs.NovelStatus;

namespace NovelService.Service.Interfaces
{
    public interface IStatusService
    {
        Task<IEnumerable<NovelStatusDto>> GetAllStatusesAsync();
        Task<NovelStatusDto?> GetStatusByIdAsync(int id);
        Task<NovelStatusDto> CreateStatusAsync(StatusCreateDto dto);
        Task<bool> UpdateStatusAsync(int id, StatusUpdateDto dto);
        Task<bool> DeleteStatusAsync(int id);
    }
}
