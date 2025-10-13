using Shareds.DTOs.Novel;

namespace NovelService.Service.Interfaces
{
    public interface INovelService
    {
        Task<NovelDetailDto> CreateNovelAsync(CreateNovelDto dto, int series_Id);
        Task<NovelDetailDto?> UpdateNovelAsync(int novel_Id, NovelUpdateDto dto, int uploader_id);
        Task<NovelDetailDto?> GetNovelByID(int novel_Id);
        Task<bool> DeleteNovelAsync(int novel_id, int uploader_Id);

        Task<bool> ReorderNovelsAsync(NovelReoderRequest request);

    }
}
