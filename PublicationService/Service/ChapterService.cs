using NovelService.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using Shareds.DTOs.Novel;

using AutoMapper;
using Shareds.DTOs.Chapter;

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
        public async Task<ChapterDetailDto> CreateChapterAsync(int novel_Id, ChapterCreateDto dto)
        {
            if (novel_Id <= 0) throw new ArgumentException("Novel Id required");
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.title)) throw new InvalidOperationException("Title required");

            
            if (dto.novelID.HasValue && dto.novelID.Value != novel_Id)
                throw new InvalidOperationException("novelId mismatch");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var novel = await _context.Novels
                      .Include(n => n.NovelSeries) // để dễ cập nhật series sau đó
                      .FirstOrDefaultAsync(n => n.novel_Id == novel_Id);

                if (novel == null)
                    throw new InvalidOperationException($"Novel {dto.novelID} not found");

                //Check series type
                if (novel.series_Id.HasValue)
                {
                    var parentSeries = await _context.Novel_Series.FindAsync(novel.series_Id.Value);
                    if (parentSeries != null && parentSeries.type == type.TRADITIONAL)
                        throw new InvalidOperationException("Cannot add chapter to novel belonging to a TRADITIONAL series");
                }

                // tính word count
                int wordCount = 0;
                if (!string.IsNullOrWhiteSpace(dto.content))
                    wordCount = dto.content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;

                // xác định chapter number
                int chapterNumber;
                if (dto.chapter_number <= 0)
                {
                    var max = await _context.Chapters.Where(c => c.novelID == novel_Id)
                        .MaxAsync(c => (int?)c.chapter_number) ?? 0;

                    chapterNumber = max + 1;
                }
                else
                {
                    var exists = await _context.Chapters.AnyAsync(c => c.novelID == novel_Id && c.chapter_number == dto.chapter_number);

                    if (exists) throw new InvalidOperationException($"Chapter number {dto.chapter_number} already exists for novel {novel_Id}");
                    chapterNumber = dto.chapter_number;
                }

                var chapter = new Chapter
                {
                    novelID = novel_Id,
                    series_Id = novel.series_Id,
                    title = dto.title,
                    content = dto.content,
                    chapter_number = chapterNumber,
                    word_count = wordCount,
                    created_at = DateTime.UtcNow
                };

                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();

                if (novel.series_Id.HasValue)
                {
                    var series = novel.NovelSeries ?? await _context.Novel_Series.FindAsync(novel.series_Id.Value);
                    if (series != null)
                    {
                        series.word_count += wordCount;
                        series.updated_at = DateTime.UtcNow;
                      
                        await _context.SaveChangesAsync();
                    }
                }

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
                _logger.LogError(dbEx, "CreateChapterForNovelAsync DB error");
                throw new InvalidOperationException("Failed to create chapter (DB conflict).");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "CreateChapterForNovelAsync failed");
                throw;
            }
            
        }
        

        //Update

        public async Task<ChapterDetailDto?> UpdateChapterAsync(int chapter_id, ChapterUpdateDto dto, int uploader_id) // chưa check quyền quản tri (uploaderID)
        {

            var chapter = await _context.Chapters.Include(c => c.Novel)
                .ThenInclude(n => n.NovelSeries)
                .FirstOrDefaultAsync(c => c.chapter_id == chapter_id);

            if (chapter == null)
                throw new InvalidOperationException("Chapter not found");

            if (!string.IsNullOrWhiteSpace(dto.title))
                chapter.title = dto.title;

            if (!string.IsNullOrWhiteSpace(dto.content))
            {
                chapter.content = dto.content;
                chapter.word_count = dto.content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;

                await _context.SaveChangesAsync();
            }

            //cập nhật word_count của cả series
            if (chapter.Novel?.NovelSeries != null)
            {
                var total_wordCount = await _context.Chapters
                        .Where(c => c.Novel!.series_Id == chapter.Novel.series_Id)
                        .SumAsync(c => c.word_count);

                chapter.Novel.NovelSeries.word_count = total_wordCount;
                chapter.Novel.NovelSeries.updated_at = DateTime.UtcNow;

                _context.Novel_Series.Update(chapter.Novel.NovelSeries);

                // _context.Chapters.Update(chapter);
                await _context.SaveChangesAsync();
            }
            return await GetChapterById(chapter.chapter_id);
        }



        //View
        public async Task<ChapterDetailDto?> GetChapterById(int chapter_id)
        {
            var c = await _context.Chapters.FirstOrDefaultAsync(c => c.chapter_id == chapter_id);
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
        public async Task<bool> DeleteChapterById(int id, int uploaderId) // chưa check quyền quản tri (uploaderID)
        {
            var chapter = await _context.Chapters.Include(c => c.Novel).FirstOrDefaultAsync(c => c.chapter_id == id);
            if (chapter == null) return false;

            var series = await _context.Novel_Series.FirstOrDefaultAsync(s => s.series_Id == chapter.Novel!.series_Id);

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                if (series != null)
                {
                    series.word_count = Math.Max(0, series.word_count - chapter.word_count);
                    series.updated_at = DateTime.UtcNow;

                    _context.Novel_Series.Update(series);
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
     
            var chapters = await _context.Chapters
                .Where(c => c.novelID == request.novel_Id)
                .OrderBy(c => c.chapter_number)
                .ToListAsync();

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

            

            //Fill remaining positions with chapters not in deltas, preserving their relative order
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
