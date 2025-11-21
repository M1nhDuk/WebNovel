using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.NovelSeries;
using System.Linq;
using System.Net.Http;

namespace NovelService.Controllers
{
    [ApiController] 
    [Route("api/internal/publication")]
    public class InternalNovelController: ControllerBase
    {
        public readonly NovelDbContext _context;
        private readonly ILogger<InternalNovelController> _logger;
        private readonly INovelSeriesService _seriesService;
        private readonly INovelService _novelService;
        private readonly IChapterService _chapterService;
        private readonly IHttpClientFactory _httpClientFactory;

        public InternalNovelController(NovelDbContext context,
            ILogger<InternalNovelController> logger,
            INovelSeriesService seriesService,
            INovelService novelService,
            IChapterService chapterService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _seriesService = seriesService;
            _novelService = novelService;
            _chapterService = chapterService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("series/{seriesId:int}/uploader")]


        public async Task<ActionResult<Guid>> GetSeriesUploader(int seriesId)
        {
            try
            {
                var series = await _context.Novel_Series
                            .Where(s => s.series_Id == seriesId)
                            .Select(s => s.uploader_id)
                            .FirstOrDefaultAsync();

                if(series == Guid.Empty)
                {
                    return NotFound($"Series with ID {seriesId} not found or has no uploader.");
                }
                return Ok(series);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("chapters/{chapterId:int}/uploader")]
        public async Task<ActionResult<Guid>> GetChapterUploader(int chapterId)
        {
            try
            {
                var chapter = await _context.Chapters
                              .Include(c => c.Novel)
                                    .ThenInclude(n => n.NovelSeries)
                              .Include(c => c.TS)
                              .FirstOrDefaultAsync(c => c.chapter_id == chapterId);

                if (chapter == null)
                {
                    return NotFound($"Chapter with ID {chapterId} not found.");
                }

                // Xác định uploaderId từ parent (NovelSeries hoặc TS)
                Guid uploaderId = chapter.Novel?.NovelSeries?.uploader_id ?? chapter.TS?.uploader_id ?? Guid.Empty;



                if (uploaderId == Guid.Empty)
                {
                    return NotFound($"Could not find uploader for the content related to Chapter ID {chapterId}.");
                }
                return Ok(uploaderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting uploader for Chapter ID {ChapterId}", chapterId);
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet("validate/series/{seriesId:int}/chapter/{chapterId:int}")]
        public async Task<IActionResult> ValidateChapterExistsInSeries(int seriesId, int chapterId)
        {
            try
            {
                var chapter = await _context.Chapters
                     .Include(c => c.Novel) 
                         .ThenInclude(n => n.NovelSeries) 
                     .FirstOrDefaultAsync(c => c.chapter_id == chapterId);

                bool isValid = false;
                
                if (chapter != null)
                {
                    if (chapter.series_Id == seriesId)
                    {
                        isValid = true;
                    }
                    else if (chapter.Novel != null && chapter.Novel.series_Id == seriesId)
                    {
                        isValid = true;
                    }
                }


                if(isValid)
                {
                    return Ok();
                }
                else
                {               
                    return NotFound(); 
                }
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error during validation.");
            }
        }

        //Reading History + Favorite
        [HttpPost("batch-series-summary")]
        public async Task<ActionResult<List<SeriesSummaryDto>>> GetBatchSeriesSummaries([FromBody] List<int> seriesIds)
        {
            if (seriesIds == null || !seriesIds.Any()) return Ok(new List<SeriesSummaryDto>());

            try
            {
                var summaries = await _context.Novel_Series
                    .AsNoTracking()
                    .Where(s => seriesIds.Contains(s.series_Id))
                    .Select(s => new SeriesSummaryDto
                    {
                        SeriesId = s.series_Id,
                        Title = s.series_title,
                        CoverImage = s.cover_images,
                        TotalChapterCount = s.type == Models.type.TRADITIONAL
                            ? s.Chapters.Count()
                            : s.Novel.SelectMany(n => n.Chapters).Count()
                    })
                    .ToListAsync();

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch series summaries.");
                return StatusCode(500, "Internal server error.");
            }
        }


        //Book mark
        [HttpPost("batch-details")]
        public async Task<ActionResult<List<PublicationDetailResponseItem>>> GetBatchPublicationDetails([FromBody] List<PublicationDetailRequestItem> requestItems)
        {
            if (requestItems == null || !requestItems.Any())
            {
                return Ok(new List<PublicationDetailResponseItem>());
            }

            var chapterIds = requestItems.Select(r => r.ChapterId).Distinct().ToList();

            try
            {
                // Truy vấn các chapter và thông tin liên quan
                var chapters = await _context.Chapters
                    .Where(c => chapterIds.Contains(c.chapter_id))
                    .Include(c => c.Novel)
                        .ThenInclude(n => n.NovelSeries) // Flow: Series -> Novel -> Chapter
                    .Include(c => c.TS) // Flow: ClassicSeries  -> Chapter
                    .AsNoTracking()
                    .ToListAsync();

                var results = new List<PublicationDetailResponseItem>();

                // Map kết quả
                foreach (var req in requestItems)
                {
                    var chapter = chapters.FirstOrDefault(c => c.chapter_id == req.ChapterId);
                    if (chapter != null)
                    {
 
                        var series = chapter.TS ?? chapter.Novel?.NovelSeries;


                        if (series != null && series.series_Id == req.SeriesId)
                        {
                            results.Add(new PublicationDetailResponseItem
                            {
                                SeriesId = req.SeriesId,
                                ChapterId = req.ChapterId,
                                SeriesTitle = series.series_title,
                                SeriesCoverImage = series.cover_images,
                                ChapterTitle = chapter.title,
                                ChapterNumber = chapter.chapter_number
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Chapter {ChapterId} found but does not belong to requested Series {SeriesId}", req.ChapterId, req.SeriesId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Chapter {ChapterId} requested in batch details not found.", req.ChapterId);
                    }
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch publication details.");
                return StatusCode(500, "Internal server error while fetching batch details.");
            }
        }






        //Helper gọi InteractionService để xóa comment
        private async Task<bool> DeleteCommentsForSeries(int seriesId)
        {
            try
            {           
                var client = _httpClientFactory.CreateClient("InteractionServiceClient");
     
                var response = await client.DeleteAsync($"api/internal/comments/by-series/{seriesId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to delete comments for SeriesId {SeriesId} from InteractionService. Status: {StatusCode}", seriesId, response.StatusCode);
                    return false;
                }
                _logger.LogInformation("Successfully triggered comment deletion for SeriesId {SeriesId}", seriesId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling InteractionService to delete comments for SeriesId {SeriesId}", seriesId);
                return false; 
            }
        }

 
        private async Task<bool> DeleteCommentsForChapter(int chapterId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("InteractionServiceClient");
                var response = await client.DeleteAsync($"api/internal/comments/by-chapter/{chapterId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to delete comments for ChapterId {ChapterId} from InteractionService. Status: {StatusCode}", chapterId, response.StatusCode);
                    return false;
                }
                _logger.LogInformation("Successfully triggered comment deletion for ChapterId {ChapterId}", chapterId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling InteractionService to delete comments for ChapterId {ChapterId}", chapterId);
                return false;
            }
        }


        [HttpGet("chapter-routing-info/{chapterId:int}")]
        public async Task<ActionResult<ChapterRoutingInfoDto>> GetChapterRoutingInfo(int chapterId)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Novel)
                    .ThenInclude(n => n.NovelSeries)
                .Include(c => c.TS) // Cho ClassicSeries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.chapter_id == chapterId);

            if (chapter == null)
            {
                return NotFound();
            }

            var series = chapter.Novel?.NovelSeries ?? chapter.TS;

            if (series == null)
            {
                return NotFound("Parent series not found.");
            }

            return Ok(new ChapterRoutingInfoDto
            {
                SeriesId = series.series_Id,
                ChapterId = chapter.chapter_id
            });
        }



        // SERIES -- NOVEL -- CHAPTER 

        [HttpDelete("admin/series/{id:int}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AdminDeleteSeries(int id)
        {
            _logger.LogWarning("Admin executing delete for SeriesId {SeriesId}", id);
            var series = await _context.Novel_Series.FindAsync(id);
            if (series == null) return NotFound();

            await DeleteCommentsForSeries(id);

            _context.Novel_Series.Remove(series);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        [HttpDelete("admin/novels/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteNovel(int id)
        {
            var novel = await _context.Novels.FindAsync(id);
            if (novel == null) return NotFound();

            foreach (var chapter in novel.Chapters)
            {
                await DeleteCommentsForChapter(chapter.chapter_id);
            }

            _context.Novels.Remove(novel);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("admin/chapters/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteChapter(int id)
        {

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return NotFound();

            await DeleteCommentsForChapter(id);

            _context.Chapters.Remove(chapter);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
