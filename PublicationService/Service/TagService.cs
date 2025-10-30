using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Tag;

namespace NovelService.Service
{
    public class TagService: ITagService
    {
        private readonly NovelDbContext _context;
        private readonly ILogger<TagService> _logger;

        public TagService(NovelDbContext context, ILogger<TagService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private TagDto MapTagToDto(Tag tag)
        {
            return new TagDto
            {
                tagId = tag.tagId,
                tagName = tag.tagName,
                Description = tag.Description
            };
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            var tags = await _context.Tags.AsNoTracking().ToListAsync();

            return tags.Select(t => MapTagToDto(t));
        }


        public async Task<TagDto?> GetTagByIdAsync(int id)
        {
            var tag = await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.tagId == id);

            if (tag == null)
            {
                return null;
            }
            return MapTagToDto(tag);
        }

        public async Task<TagDto> CreateTagAsync(TagCreateDto dto)
        {
            bool nameExists = await _context.Tags.AnyAsync(t => t.tagName == dto.tagName);

            if (nameExists)
            {
                throw new InvalidOperationException("Tag name already exists.");
            }

            var newTag = new Tag
            {
                tagName = dto.tagName,
                Description = dto.Description,
            };

            _context.Tags.Add(newTag);
            await _context.SaveChangesAsync();

            return MapTagToDto(newTag);
        }

        public async Task<bool> UpdateTagAsync(int id, TagUpdateDto dto)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return false;
            }

            if (tag.tagName != dto.tagName)
            {
                bool nameExists = await _context.Tags.AnyAsync(t => t.tagName == dto.tagName && t.tagId != id);
                if (nameExists)
                {
                    throw new InvalidOperationException("Tag name already exists.");
                }
            }

                tag.tagName = dto.tagName;
                tag.Description = dto.Description;

                _context.Entry(tag).State = EntityState.Modified;


                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Concurrency exception updating tag {TagId}", id);
                    return false;
                }
            }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return false; 
            }

            bool isInUse = await _context.Novel_Tags.AnyAsync(nt => nt.tagID == id);
            if (isInUse)
            {
                throw new InvalidOperationException("Cannot delete this tag because it is in use by one or more series.");
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
