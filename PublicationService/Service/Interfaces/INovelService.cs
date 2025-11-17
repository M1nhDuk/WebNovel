using Shareds.DTOs.Novel;

namespace NovelService.Service.Interfaces
{
    public interface INovelService
    {
        Task<NovelDetailDto> CreateNovelAsync(CreateNovelDto dto, int series_Id);
        Task<NovelDetailDto?> UpdateNovelAsync(int novel_Id, NovelUpdateDto dto, Guid uploader_id, string userRole, int series_Id); 
        Task<NovelDetailDto?> GetNovelByID(int novel_Id, int series_Id);
        Task<bool> DeleteNovelAsync(int novel_id, Guid uploader_Id, string userRole, int series_Id);

        Task<bool> ReorderNovelsAsync(NovelReoderRequest request, Guid uploader_Id, string userRole);

    }
}
