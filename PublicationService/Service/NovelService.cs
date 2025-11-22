using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;

using Shareds.DTOs.Novel;

using AutoMapper;
using Shareds.DTOs.Chapter;
using System.Net.Http;

namespace NovelService.Service
{
    public class NovelService : INovelService
    {
        private readonly NovelDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<INovelService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;
        public NovelService(NovelDbContext context, ILogger<INovelService> logger, IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _context = context;
            // _userService = userService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }


        //Create
        public async Task<NovelDetailDto> CreateNovelAsync(CreateNovelDto dto, int series_Id)
        {
            // kiểm tra series tồn tại
            var series = await _context.Novel_Series.FirstOrDefaultAsync(s => s.series_Id == dto.series_Id);
            if (series == null) throw new InvalidOperationException("Series not found");

            if (string.IsNullOrWhiteSpace(dto.title)) throw new InvalidOperationException("Enter title");


            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {

                var maxNumber = await _context.Novels
                    .Where(n => n.series_Id == dto.series_Id)
                    .MaxAsync(n => (int?)n.novel_number) ?? 0;
                var nextNumber = maxNumber + 1;

                // check duplicate title in series
                var exists = await _context.Novels.AnyAsync(n => n.series_Id == dto.series_Id && n.title == dto.title);
                if (exists) throw new InvalidOperationException("Novel with same title exists in series");

                var novel = new Novel
                { 
                    series_Id = series_Id,
                    title = dto.title,
                    cover_images = string.IsNullOrEmpty(dto.cover_images) ? "/images/covers/default_cover.jpg" : dto.cover_images,
                    novel_number = nextNumber,
                    updated_at = DateTime.UtcNow
                };
                _context.Novels.Add(novel);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                return new NovelDetailDto
                {
                    novel_Id = novel.novel_Id,
                    series_Id = novel.series_Id,
                    title = novel.title,
                    cover_images = novel.cover_images,
                    novel_number = novel.novel_number,
                    author = series.author,
                    artist = series.artist,
                    updated_at = novel.updated_at,
                    uploader_id = series.uploader_id,
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        //Update 
        public async Task<NovelDetailDto?> UpdateNovelAsync(int novel_Id, NovelUpdateDto dto, Guid uploader_id, string userRole, int series_Id)
        {
            // kiểm tra series tồn tại
            var novel = await _context.Novels
                .Include(n => n.NovelSeries)
                .FirstOrDefaultAsync(n => n.novel_Id == novel_Id && n.series_Id == series_Id);


            if (novel == null)
                throw new InvalidOperationException("Novel not found");

            if (novel.NovelSeries == null)
            {
                throw new InvalidOperationException("Cannot update novel: Parent series not found.");
            }

            if (novel.NovelSeries.uploader_id != uploader_id && userRole != "Admin") 
            {
                throw new UnauthorizedAccessException("You are not authorized to update this novel.");
            }

            if (!string.IsNullOrWhiteSpace(dto.title))
                novel.title = dto.title;

            if (!string.IsNullOrWhiteSpace(dto.cover_images))
                novel.cover_images = dto.cover_images;


            await _context.SaveChangesAsync();

            return await GetNovelByID(novel.novel_Id, series_Id);
        }


        //Read by id
        public async Task<NovelDetailDto?> GetNovelByID(int id, int series_Id )
        {

            var n = await _context.Novels.Include(x => x.Chapters)
                .Include(x => x.NovelSeries)
                .FirstOrDefaultAsync(x => x.novel_Id == id && x.series_Id == series_Id);

            if (n == null) return null;

            return new NovelDetailDto
            {
                series_Id = n.series_Id,
                novel_Id = n.novel_Id,
                title = n.title,
                novel_number = n.novel_number,
                cover_images = n.cover_images,
                updated_at = n.updated_at,



                // map từ NovelSeries
                author = n.NovelSeries?.author ?? string.Empty,
                artist = n.NovelSeries?.artist,
                uploader_id = n.NovelSeries?.uploader_id ?? Guid.Empty,


                //lấy từ UserService (chưa làm)
                uploader_name = string.Empty,
                uploader_avatar = null,

                // map chapter
                Chapters = n.Chapters.OrderBy(c => c.chapter_number).Select(c => new ChapterDetailDto
                {
                    novelID = c.novelID,
                    chapter_id = c.chapter_id,
                    title = c.title,
                    chapter_number = c.chapter_number,
                    // word_count = c.word_count,
                    created_at = c.created_at,
                    //  content = c.content,

                }).ToList()
            };
        }



        //Delete
        public async Task<bool> DeleteNovelAsync(int id, Guid uploader_Id, string userRole, int series_Id)
        {
            var novel = await _context.Novels
                .Include(n => n.Chapters)
                .Include(n => n.NovelSeries)
                .FirstOrDefaultAsync(n => n.novel_Id == id && n.series_Id == series_Id);

            if (novel == null)
                throw new InvalidOperationException("Novel not found");

            if (novel.NovelSeries == null)
                throw new InvalidOperationException("Cannot delete novel: Parent series not found.");

            if (novel.NovelSeries.uploader_id != uploader_Id && userRole != "Admin")
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this novel.");
            }

            //LƯU ĐƯỜNG DẪN ẢNH CẦN XÓA 
            string? coverPathToDelete = null;
            var defaultCoverPath = "/images/covers/default_cover.jpg";

            // Chỉ đánh dấu xóa nếu có ảnh và không phải ảnh mặc định
            if (!string.IsNullOrEmpty(novel.cover_images) && novel.cover_images != defaultCoverPath)
            {
                coverPathToDelete = novel.cover_images;
            }

            // XÓA COMMENT (Logic cũ giữ nguyên)
            try
            {
                var chapterIds = novel.Chapters.Select(c => c.chapter_id).ToList();

                if (chapterIds.Any())
                {
                    var httpClient = _httpClientFactory.CreateClient("InteractionServiceClient");

                    foreach (var chapId in chapterIds)
                    {
                        var response = await httpClient.DeleteAsync($"api/internal/comments/by-chapter/{chapId}");
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("Failed to delete comments for ChapterId {ChapterId}", chapId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling InteractionService while deleting NovelId {NovelId}", id);
            }

            int deletedWordCount = novel.Chapters?.Sum(c => c.word_count) ?? 0;

            // Xóa Chapters trong DB
            if (novel.Chapters != null && novel.Chapters.Any())
            {
                _context.Chapters.RemoveRange(novel.Chapters);
            }

            // Xóa Novel trong DB
            _context.Novels.Remove(novel);

            // Cập nhật Word Count cho Series cha
            if (novel.NovelSeries != null)
            {
                novel.NovelSeries.word_count = Math.Max(0, novel.NovelSeries.word_count - deletedWordCount);
                novel.NovelSeries.updated_at = DateTime.UtcNow;
                _context.Novel_Series.Update(novel.NovelSeries);
            }


            await _context.SaveChangesAsync();

            // XÓA FILE ẢNH VẬT LÝ (Chỉ chạy khi DB đã xóa xong)
            if (!string.IsNullOrEmpty(coverPathToDelete))
            {
                try
                {
                    // Lấy tên file từ đường dẫn URL 
                    var fileName = Path.GetFileName(coverPathToDelete);

                    // Tạo đường dẫn vật lý
                    var filePath = Path.Combine(_environment.WebRootPath, "images", "covers", fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation("Deleted cover image for NovelId {NovelId}: {FilePath}", id, filePath);
                    }
                }
                catch (Exception ex)
                {
                 
                    _logger.LogWarning(ex, "Novel deleted successfully but failed to delete cover image file: {Path}", coverPathToDelete);
                }
            }

            return true;
        }


        //Reorder
        public async Task<bool> ReorderNovelsAsync(NovelReoderRequest request, Guid uploader_Id, string userRole)
        {
            // Lấy tất cả novels thuộc series
            var novels = await _context.Novels
                .Where(n => n.series_Id == request.series_Id)
                .Include(n => n.NovelSeries)
                .OrderBy(n => n.novel_number)
                .ToListAsync();

            if(!novels.Any()) return false;

            var parentSeries = novels.FirstOrDefault()?.NovelSeries;
            if (parentSeries == null) return false;
            if (parentSeries.uploader_id != uploader_Id && userRole != "Admin") 
            {
                throw new UnauthorizedAccessException("You are not authorized to reorder these novels.");
            }

            int total = novels.Count;

            if(request.Novels == null || request.Novels.Count == 0) return false;

            
            if (request.Novels.Select(d => d.novel_id).Distinct().Count() != request.Novels.Count) return false;

            //Check chapter id có tồn tại 
            var dbIDs = novels.Select(n => n.novel_Id).ToHashSet();
            if (!request.Novels.All(n => dbIDs.Contains(n.novel_id))) return false;

            //check new_position trong khoảng và không trùng lặp vị trí
            if (request.Novels.Any(d => d.new_position < 1 || d.new_position > total)) return false;
            if (request.Novels.Select(d => d.new_position).Distinct().Count() != request.Novels.Count) return false;

            var assigned = new bool[total + 1];
            var finalPos = new Dictionary<int, int>(total);

            foreach (var n in request.Novels)
            {
                assigned[n.new_position] = true;
                finalPos[n.novel_id] = n.new_position;
            }

            int cursor = 1;
            foreach (var novel in novels)
            {
                if (finalPos.ContainsKey(novel.novel_Id))
                    continue;

                while (cursor <= total && assigned[cursor]) cursor++;
                if (cursor > total)
                {
                    return false;
                }

                finalPos[novel.novel_Id] = cursor;
                assigned[cursor] = true;
            }

            var novelById = novels.ToDictionary(n => n.novel_Id);


            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // B1: gán tạm âm
                int temp = -1;
                foreach (var item in finalPos)
                {
                    if (novelById.TryGetValue(item.Key, out var nn))
                    {
                        nn.novel_number = temp--;
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
                    if (novelById.TryGetValue(item.Key, out var nn))
                    {
                        nn.novel_number = item.Value;
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


