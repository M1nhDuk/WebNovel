using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;

namespace NovelService.Service.Interfaces
{
    public interface IChapterService
    {
        Task<ChapterDetailDto> CreateChapterAsync(ChapterCreateDto dto );
        Task<ChapterDetailDto?> UpdateChapterAsync(int chapter_id, ChapterUpdateDto dto, int uploader_id, int? novelId = null, int? seriesId = null);
        Task<ChapterDetailDto?> GetChapterById(int chapter_id, int? novelId = null, int? seriesId = null);
        Task<bool> DeleteChapterById (int chapter_id, int uploaderId, int? novelId = null, int? seriesId = null);

        Task<bool> ReorderChapterAsync(ReorderChaptersRequest request);
    }
}
