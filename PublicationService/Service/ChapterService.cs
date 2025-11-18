using NovelService.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using Shareds.DTOs.Novel;

using AutoMapper;
using Shareds.DTOs.Chapter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;

namespace NovelService.Service
{
    public class ChapterService : IChapterService
    {

        private readonly NovelDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<IChapterService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChapterService(NovelDbContext context, ILogger<IChapterService> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            // _userService = userService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        //TẠO CHAPTER (liên kết đến novel tồn tại) + update counts
        public async Task<ChapterDetailDto> CreateChapterAsync(ChapterCreateDto dto, Guid uploader_id, string userRole)
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

                if (parentSeries == null)
                {
                    throw new InvalidOperationException("Could not find parent series for authorization.");
                }
                if (parentSeries.uploader_id != uploader_id && userRole != "Admin")
                {
                    throw new UnauthorizedAccessException("You are not authorized to add chapters to this content.");
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
        public async Task<ChapterDetailDto?> UpdateChapterAsync(int chapter_id, ChapterUpdateDto dto, Guid uploader_id, string userRole, int? novelId = null, int? seriesId = null)
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

            // Xác định parentSeries
            var parentSeries = chapter.Novel?.NovelSeries ?? chapter.TS;

            if (parentSeries == null)
            {
                throw new InvalidOperationException("Could not find parent series for authorization.");
            }

            if (parentSeries.uploader_id != uploader_id && userRole != "Admin")
            {
                throw new UnauthorizedAccessException("You are not authorized to add chapters to this content.");
            }

            bool contentChanged = false;

            int oldWordCount = chapter.word_count;
            int newWordCount = oldWordCount;

            if (!string.IsNullOrWhiteSpace(dto.title))
                chapter.title = dto.title;

            if (!string.IsNullOrWhiteSpace(dto.content))
            {
                chapter.content = dto.content;
                newWordCount = dto.content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
                chapter.word_count = newWordCount; 
                contentChanged = true;
            }

            // Nếu nội dung thay đổi, cập nhật lại word_count của series cha
            if (contentChanged && parentSeries != null)
            {
                // Tính toán chênh lệch
                int wordCountDelta = newWordCount - oldWordCount;

                parentSeries.word_count += wordCountDelta;
                parentSeries.updated_at = DateTime.UtcNow;
                _context.Novel_Series.Update(parentSeries);
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
        public async Task<bool> DeleteChapterById(int id, Guid uploaderId, string userRole, int? novelId = null, int? seriesId = null)
        {
            // 1. Truy vấn Chapter và kiểm tra quyền 
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

            if (parentSeries == null || (parentSeries.uploader_id != uploaderId && userRole != "Admin"))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this chapter.");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("InteractionServiceClient");
                var response = await httpClient.DeleteAsync($"api/internal/comments/by-chapter/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to delete comments for ChapterId {ChapterId}. Status: {Status}. Details: {Error}", id, response.StatusCode, errorContent);

                    throw new Exception("Failed to clear comments. Aborting chapter deletion.");
                }
                _logger.LogInformation("Successfully triggered comment deletion for ChapterId {ChapterId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling InteractionService during chapter deletion.");
                throw; 
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                int wordCountToRemove = chapter.word_count;
                int seriesIdToUpdate = parentSeries.series_Id;

                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();

                await _context.Novel_Series
                    .Where(s => s.series_Id == seriesIdToUpdate)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.word_count, x => x.word_count - wordCountToRemove < 0 ? 0 : x.word_count - wordCountToRemove)
                        .SetProperty(x => x.updated_at, DateTime.UtcNow)
                    );

                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Database error while deleting chapter {ChapterId}", id);
                throw;
            }
        }

        //Reorder
        public async Task<bool> ReorderChapterAsync(ReorderChaptersRequest request, Guid uploaderId, string userRole)
        {
            var hasNovelParent = request.novel_Id.HasValue;
            var hasSeriesParent = request.series_Id.HasValue;

            if (hasNovelParent == hasSeriesParent)
            {
                throw new InvalidOperationException("Reorder request must be for exactly one Novel OR one Series.");
            }

            List<Chapter> chapters;
            NovelSeries parentSeries = null;

            if (hasNovelParent)
            {
                chapters = await _context.Chapters
                   .Where(c => c.novelID == request.novel_Id)
                   .Include(c => c.Novel).ThenInclude(n => n.NovelSeries)
                   .OrderBy(c => c.chapter_number)
                   .ToListAsync();
                parentSeries = chapters.FirstOrDefault()?.Novel?.NovelSeries;
            }
            else
            {
                // Kiểm tra xem series có phải là TRADITIONAL không 
                var series = await _context.Novel_Series.FindAsync(request.series_Id.Value);
                if (series == null || series.type != type.TRADITIONAL)
                {
                    return false; // Hoặc ném lỗi
                }

                parentSeries = series;
                chapters = await _context.Chapters
                    .Where(c => c.series_Id == request.series_Id)
                    .OrderBy(c => c.chapter_number)
                    .ToListAsync();
            }
                       

            if (!chapters.Any()) return false;

            if (parentSeries == null) return false;
            if (parentSeries.uploader_id != uploaderId && userRole != "Admin")
            {
                throw new UnauthorizedAccessException("You are not authorized to reorder these chapters.");
            }

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



            //Điền các vị trí còn lại bằng các chương và giữ nguyên thứ tự của các chương 
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
