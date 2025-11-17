using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;

namespace NovelService.Service.Interfaces
{
    public interface IChapterService
    {
        Task<ChapterDetailDto> CreateChapterAsync(ChapterCreateDto dto, Guid uploader_id, string userRole);
        Task<ChapterDetailDto?> UpdateChapterAsync(int chapter_id, ChapterUpdateDto dto, Guid uploader_id, string userRole, int? novelId = null, int? seriesId = null);
        Task<ChapterDetailDto?> GetChapterById(int chapter_id, int? novelId = null, int? seriesId = null);
        Task<bool> DeleteChapterById(int chapter_id, Guid uploaderId, string userRole, int? novelId = null, int? seriesId = null);

        Task<bool> ReorderChapterAsync(ReorderChaptersRequest request, Guid uploaderId, string userRole);
    }
}
