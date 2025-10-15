using NovelService.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using Shareds.DTOs.Novel;

using AutoMapper;
using Shareds.DTOs.Chapter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NovelService.Service
{
    public class ChapterService : IChapterService
    {

        private readonly NovelDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<IChapterService> _logger;

        public ChapterService(NovelDbContext context, ILogger<IChapterService> logger)
        {
            _context = context;
            // _userService = userService;
            _logger = logger;
        }

        //TẠO CHAPTER (liên kết đến novel tồn tại) + update counts
        public async Task<ChapterDetailDto> CreateChapterAsync(ChapterCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.title)) throw new InvalidOperationException("Title required");

            var hasNovelParent = dto.novelID.HasValue;
            var hasSeriesParent = dto.series_id.HasValue;
            if (hasNovelParent == hasSeriesParent) 
            {
                throw new InvalidOperationException("Chapter must belong to exactly one Novel OR one Series.");
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                NovelSeries parentSeries = null;
                int chapterNumber;

                // --- Logic tính word count  ---
                int wordCount = 0;
                if (!string.IsNullOrWhiteSpace(dto.content))
                    wordCount = dto.content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;

               
                if (hasNovelParent)
                {
                    var novel = await _context.Novels
                        .Include(n => n.NovelSeries)
                        .FirstOrDefaultAsync(n => n.novel_Id == dto.novelID.Value);

                    if (novel == null) throw new InvalidOperationException($"Novel {dto.novelID} not found");

                    if (novel.NovelSeries?.type == type.TRADITIONAL)
                        throw new InvalidOperationException("Cannot add chapter to a Novel that belongs to a TRADITIONAL series.");

                    if (novel.series_Id.HasValue)
                    {
                        parentSeries = novel.NovelSeries;
                    }

                    //Index cho novel
                    var max = await _context.Chapters.Where(c => c.novelID == dto.novelID.Value).MaxAsync(c => (int?)c.chapter_number) ?? 0;
                    chapterNumber = max + 1;
                }
                else // hasSeriesParent is true
                {
                    var series = await _context.Novel_Series.FindAsync(dto.series_id.Value);

                    if (series == null) throw new InvalidOperationException($"Series {dto.series_id} not found");

                    if (series.type != type.TRADITIONAL)
                        throw new InvalidOperationException("A chapter can only be directly added to a TRADITIONAL series.");

                    parentSeries = series;

                    //Index cho series chapter
                    var max = await _context.Chapters.Where(c => c.series_Id == dto.series_id.Value).MaxAsync(c => (int?)c.chapter_number) ?? 0;
                    chapterNumber = max + 1;
                }


                
                var chapter = new Chapter
                {
                    novelID = dto.novelID,
                    series_Id = dto.series_id, 
                    title = dto.title,
                    content = dto.content,
                    chapter_number = chapterNumber,
                    word_count = wordCount,
                    created_at = DateTime.UtcNow
                };
                _context.Chapters.Add(chapter);

                //Cập nhật word_count cho series  
                if (parentSeries != null)
                {
                    parentSeries.word_count += wordCount;
                    parentSeries.updated_at = DateTime.UtcNow;
                    _context.Novel_Series.Update(parentSeries);
                }

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
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "CreateChapterAsync failed");
                throw;
            }
        }
        

        //Update
        public async Task<ChapterDetailDto?> UpdateChapterAsync(int chapter_id, ChapterUpdateDto dto, int uploader_id, int? novelId = null, int? seriesId = null) // chưa check quyền quản tri (uploaderID)
        {
            var query = _context.Chapters
                .Include(c => c.Novel).ThenInclude(n => n.NovelSeries)
                .Include(c => c.TS)
                .AsQueryable();


            if (novelId.HasValue)
            {
                query = query.Where(c => c.novelID == novelId.Value);
            }
            else if (seriesId.HasValue)
            {
                query = query.Where(c => c.series_Id == seriesId.Value);
            }

            var chapter = await query.FirstOrDefaultAsync(c => c.chapter_id == chapter_id);

            if (chapter == null)
                throw new InvalidOperationException("Chapter not found");

            bool contentChanged = false;
            if (!string.IsNullOrWhiteSpace(dto.title))
                chapter.title = dto.title;

            if (!string.IsNullOrWhiteSpace(dto.content))
            {
                chapter.content = dto.content;
                chapter.word_count = dto.content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
                contentChanged = true;
            }



            // Nếu nội dung thay đổi, cập nhật lại word_count của series cha
            if (contentChanged)
            {
                // Xác định parentSeries
                var parentSeries = chapter.Novel?.NovelSeries ?? chapter.TS;

                if (parentSeries != null)
                {
                    // Tính lại tổng word count của tất cả chapter thuộc series
                    var totalWordCount = await _context.Chapters
                        .Where(c => (c.Novel != null && c.Novel.series_Id == parentSeries.series_Id) || c.series_Id == parentSeries.series_Id)
                        .SumAsync(c => c.word_count);

                    parentSeries.word_count = totalWordCount;
                    parentSeries.updated_at = DateTime.UtcNow;
                    _context.Novel_Series.Update(parentSeries);
                }
            }

            await _context.SaveChangesAsync();
            return await GetChapterById(chapter.chapter_id);
        }

        //View
        public async Task<ChapterDetailDto?> GetChapterById(int chapter_id, int? novelId = null, int? seriesId = null)
        {
            var query = _context.Chapters.AsQueryable();

            if (novelId.HasValue)
            {
                query = query.Where(c => c.novelID == novelId.Value);
            } 
            else if (seriesId.HasValue)
            {
                query = query.Where(c => c.series_Id == seriesId.Value);
            }

            var c = await query.FirstOrDefaultAsync(c => c.chapter_id == chapter_id);

            if (c == null) return null;

            return new ChapterDetailDto
            {
                novelID = c.novelID,
                chapter_id = c.chapter_id,
                title = c.title,
                chapter_number = c.chapter_number,
                word_count = c.word_count,
                created_at = c.created_at,
                content = c.content
            };
        }

        //Delete
        public async Task<bool> DeleteChapterById(int id, int uploaderId, int? novelId = null, int? seriesId = null) // chưa check quyền quản tri (uploaderID)
        {
            var query = _context.Chapters
                .Include(c => c.Novel).ThenInclude(n => n.NovelSeries)
                .Include(c => c.TS)
                .AsQueryable();

            if (novelId.HasValue)
            {
                query = query.Where(c => c.novelID == novelId.Value);
            }
            else if (seriesId.HasValue)
            {
                query = query.Where(c => c.series_Id == seriesId.Value);
            }

            var chapter = await query.FirstOrDefaultAsync(c => c.chapter_id == id);

            if (chapter == null) return false;

            var parentSeries = chapter.Novel?.NovelSeries ?? chapter.TS;

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Trừ word_count khỏi series cha
                if (parentSeries != null)
                {
                    parentSeries.word_count = Math.Max(0, parentSeries.word_count - chapter.word_count);
                    parentSeries.updated_at = DateTime.UtcNow;
                    _context.Novel_Series.Update(parentSeries);
                }

                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "DeleteChapterAsync failed for chapterId={chapterId}", id);
                throw;
            }
        }

        //Reorder
        public async Task<bool> ReorderChapterAsync(ReorderChaptersRequest request)
        {
            var hasNovelParent = request.novel_Id.HasValue;
            var hasSeriesParent = request.series_Id.HasValue;

            if (hasNovelParent == hasSeriesParent)
            {
                throw new InvalidOperationException("Reorder request must be for exactly one Novel OR one Series.");
            }

            List<Chapter> chapters;

            if(hasNovelParent)
            {
                chapters = await _context.Chapters
                   .Where(c => c.novelID == request.novel_Id)
                   .OrderBy(c => c.chapter_number)
                   .ToListAsync();
            }
            else
            {
                // Kiểm tra xem series có phải là TRADITIONAL không 
                var series = await _context.Novel_Series.FindAsync(request.series_Id.Value);
                if (series == null || series.type != type.TRADITIONAL)
                {
                    return false; // Hoặc ném lỗi
                }

                chapters = await _context.Chapters
                    .Where(c => c.series_Id == request.series_Id)
                    .OrderBy(c => c.chapter_number)
                    .ToListAsync();
            }
                       

            if (!chapters.Any()) return false;

            int total = chapters.Count;

           if(request.Chapters == null || request.Chapters.Count == 0) return false;

           //check duplicate trong chapter list
           if(request.Chapters.Select(d => d.chapter_id).Distinct().Count() != request.Chapters.Count) return false;

           //Check chapter id có tồn tại 
           var dbIDs = chapters.Select(c => c.chapter_id).ToHashSet();
            if (!request.Chapters.All(c => dbIDs.Contains(c.chapter_id))) return false;

            //check new_position trong khoảng và không trùng lặp vị trí
            if(request.Chapters.Any(d => d.new_position < 1 || d.new_position > total)) return false;
            if (request.Chapters.Select(d => d.new_position).Distinct().Count() != request.Chapters.Count) return false;

            var assigned = new bool[total + 1];
            var finalPos = new Dictionary<int, int>(total);

            foreach (var c in request.Chapters )
            {
                assigned[c.new_position] = true;
                finalPos[c.chapter_id] = c.new_position;
            }



            //Điền các vị trí còn lại bằng các chương không có delta, giữ nguyên thứ tự của các chương 
            int cursor = 1;
            foreach (var chapter in chapters)
            {
                if (finalPos.ContainsKey(chapter.chapter_id))
                    continue;

                while(cursor <= total && assigned[cursor]) cursor++;
                if(cursor > total)
                {
                    return false;
                }

                finalPos[chapter.chapter_id] = cursor;
                assigned[cursor] = true;
            }

            var chaptersById = chapters.ToDictionary(c => c.chapter_id);

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // B1: gán tạm âm
                int temp = -1;
                foreach (var item in finalPos)
                {
                    if (chaptersById.TryGetValue(item.Key, out var ch))
                    {
                        ch.chapter_number = temp--;
                    }
                    else
                    {
                        await tx.RollbackAsync();
                        return false;
                    }
                }
                 await _context.SaveChangesAsync();

                // B2: gán số thứ tự theo chính thức           
                foreach (var item in finalPos)
                {
                    if (chaptersById.TryGetValue(item.Key, out var ch))
                    {
                        ch.chapter_number = item.Value;
                    }
                    else
                    {
                        await tx.RollbackAsync();
                        return false;
                    }
                }
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return false;
            }
        }
    }
}
