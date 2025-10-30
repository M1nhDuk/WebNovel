using Shareds.DTOs.Tag;

namespace NovelService.Service.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<TagDto?> GetTagByIdAsync(int id);
        Task<TagDto> CreateTagAsync(TagCreateDto dto);
        Task<bool> UpdateTagAsync(int id, TagUpdateDto dto);
        Task<bool> DeleteTagAsync(int id);
    }
}
