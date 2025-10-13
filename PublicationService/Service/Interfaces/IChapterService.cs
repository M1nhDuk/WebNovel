using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;

namespace NovelService.Service.Interfaces
{
    public interface IChapterService
    {
        Task<ChapterDetailDto> CreateChapterAsync(int novel_Id, ChapterCreateDto dto );
        Task<ChapterDetailDto?> UpdateChapterAsync(int chapter_id, ChapterUpdateDto dto, int uploader_id);
        Task<ChapterDetailDto?> GetChapterById(int chapter_id);
        Task<bool> DeleteChapterById (int chapter_id, int uploaderId);

        Task<bool> ReorderChapterAsync(ReorderChaptersRequest request);
    }
}
