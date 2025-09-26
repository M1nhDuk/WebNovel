using Shareds.DTOs.Novel;
using Shareds.DTOs;
using Shareds.DTOs.Chapter;

namespace NovelService.Service
{
    public class INoveLService
    {
        public interface INovelService
        {
            Task<NovelDetailDto> CreateNovelAsync(CreateNovelDto dto);
            Task<List<NovelListDto>> GetAllNovelsAsync();
            Task<NovelDetailDto?> GetNovelByIdAsync(int id);
            Task<bool> UpdateNovelAsync(int id, NovelUpdateDto dto);
            Task<bool> DeleteNovelAsync(int id);


            //Chapter
            Task<ChapterDetailDto> CreateChapterAsync(ChapterCreateDto dto);
            Task<bool> UpdateChapterAsync(int chapterId, ChapterUpdateDto dto);
            Task<bool> DeleteChapterAsync(int chapterId);
            Task<bool> ReorderChaptersAsync(ReorderChaptersRequest request);
        }

    }
}
