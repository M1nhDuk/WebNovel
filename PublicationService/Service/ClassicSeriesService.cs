using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.Novel;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Service
{
    public class ClassicSeriesService
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


        public async Task<ChapterDetailDto> CreateChapterForCSAsync(int series_Id, ChapterCreateDto dto, int authorId)
        {
            if (series_Id <= 0) throw new ArgumentException("seriesId required");
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.title)) throw new InvalidOperationException("Title required");

            // ensure dto.series_id either null or equal seriesId
            if (dto.series_id.HasValue && dto.series_id.Value != series_Id)
                throw new InvalidOperationException("series_Id mismatch");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var series = await _context.Novel_Series.FindAsync(series_Id);
                if (series == null) throw new KeyNotFoundException($"Series {series_Id} not found");
                if (series.type != type.TRADITIONAL) throw new InvalidOperationException("Series is not TRADITIONAL");

                // compute word count
                int wordCount = 0;
                if (!string.IsNullOrWhiteSpace(dto.content))
                    wordCount = dto.content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;

                // chapter number
                int chapterNumber;
                if (dto.chapter_number <= 0)
                {
                    var max = await _context.Chapters.Where(c => c.series_Id == series_Id).MaxAsync(c => (int?)c.chapter_number) ?? 0;
                    chapterNumber = max + 1;
                }
                else
                {
                    var exists = await _context.Chapters.AnyAsync(c => c.series_Id == series_Id && c.chapter_number == dto.chapter_number);
                    if (exists) throw new InvalidOperationException($"Chapter number {dto.chapter_number} already exists for series {series_Id}");
                    chapterNumber = dto.chapter_number;
                }

                var chapter = new Chapter
                {
                    novelID = null,
                    series_Id = series_Id,
                    title = dto.title,
                    content = dto.content,
                    chapter_number = chapterNumber,
                    word_count = wordCount,
                    created_at = DateTime.UtcNow
                };

                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();

                // update series.word_count
                series.word_count += wordCount;
                series.updated_at = DateTime.UtcNow;
                _context.Novel_Series.Update(series);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                return new ChapterDetailDto
                {
                    chapter_id = chapter.chapter_id,
                    novelID = chapter.novelID,
                    series_Id = chapter.series_Id,
                    title = chapter.title,
                    content = chapter.content,
                    chapter_number = chapter.chapter_number,
                    word_count = chapter.word_count,
                    created_at = chapter.created_at
                };
            }
            catch (DbUpdateException dbEx)
            {
                await tx.RollbackAsync();
                _logger.LogError(dbEx, "CreateChapterForSeriesAsync DB error");
                throw new InvalidOperationException("Failed to create chapter (DB conflict).");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "CreateChapterForSeriesAsync failed");
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
