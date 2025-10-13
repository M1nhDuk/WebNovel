using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.Novel;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Service
{
    public class ClassicSeriesService: IClassicSeries
    {
        private readonly NovelDbContext _context;
        private readonly ILogger<ClassicSeriesService> _logger;

        public ClassicSeriesService(NovelDbContext context, ILogger<ClassicSeriesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // TẠO TRADITIONAL SERIES (TS)
        public async Task<ClassicSeriesDetailDto> CreateTraditionalSeriesAsync(CreateTraditionalSeriesDto dto, int uploaderId)
        {
            if (string.IsNullOrEmpty(dto.series_title)) throw new InvalidOperationException("Series title is required");
            if (string.IsNullOrEmpty(dto.description)) throw new InvalidOperationException("Description is required");
            if (string.IsNullOrEmpty(dto.ISBN_13)) throw new InvalidOperationException("ISBN_13 is required for Traditional Series");
            if (string.IsNullOrEmpty(dto.author)) throw new InvalidOperationException("Author is required");


            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var ts = new ClassicSeries
                {
                    series_title = dto.series_title,
                    author = dto.author,
                    artist = dto.artist,
                    description = dto.description,
                    cover_images = dto.cover_images,
                    note = dto.note,
                    uploader_id = uploaderId,
                    category_id = dto.category_id ?? throw new ArgumentNullException(nameof(dto.category_id)),
                    status_id = dto.status_id,
                    views = 0,
                    word_count = 0,
                    type = type.TRADITIONAL,

                    // ClassicSeries specific
                    ISBN_10 = dto.ISBN_10,
                    ISBN_13 = dto.ISBN_13,
                    publisher = dto.publisher,
                    publish_date = dto.publish_date,
                    edition = dto.edition
                };

               
                _context.ClassicSeries.Add(ts);
                await _context.SaveChangesAsync();

                // update derived word_count, etc. (nếu cần)
                ts.word_count = 0;
                _context.Novel_Series.Update(ts);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                return await GetByIdAsync(ts.series_Id) ?? throw new InvalidOperationException("Failed to return created traditional series");
    
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Create Traditional Series failed");
                throw;
            }
        }


       
       

        public async Task<ClassicSeriesDetailDto?> GetByIdAsync(int id)
        {
            var ts = await _context.ClassicSeries
                .Include(x => x.Chapters)
                .Include(x => x.status)
                .Include(x => x.category)
                .Include(x => x.NovelTags)
                    .ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(x => x.series_Id == id);

            if (ts == null) return null;

            return new ClassicSeriesDetailDto
            {
                series_Id = ts.series_Id,
                series_title = ts.series_title,
                author = ts.author,
                artist = ts.artist,
                description = ts.description,
                cover_images = ts.cover_images,
                word_count = ts.word_count,
                views = ts.views,
                note = ts.note,
                created_at = ts.created_at,
                updated_at = ts.updated_at,
                uploader_id = ts.uploader_id,
                ISBN_10 = ts.ISBN_10,
                ISBN_13 = ts.ISBN_13,
                publisher = ts.publisher,
                publish_date = ts.publish_date,
                edition = ts.edition,
                

                // category + status
                category_id = ts.category_id,
                categoryName = ts.category?.category_name,
                status_id = ts.status_id,
                statusName = ts.status?.statusName,

                // tags: chỉ lấy tên
                Tags = ts.NovelTags.Select(t => t.Tag.tagName).ToList(),


            };
        }
    }
}
