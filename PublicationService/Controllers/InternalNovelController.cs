using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Controllers
{
    [ApiController] 
    [Route("api/internal/publication")]
    public class InternalNovelController: ControllerBase
    {
        public readonly NovelDbContext _context;
        private readonly ILogger<InternalNovelController> _logger;

        public InternalNovelController(NovelDbContext context, ILogger<InternalNovelController> logger)
        {
            _context = context;
            _logger = logger;
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


        [HttpPost("batch-series-summary")] 
        public async Task<ActionResult<List<SeriesSummaryDto>>> GetBatchSeriesSummaries([FromBody] List<int> seriesIds)
        {
            if (seriesIds == null || !seriesIds.Any())
            {
                return Ok(new List<SeriesSummaryDto>()); 
            }

            try
            {
                var summaries = await _context.Novel_Series
                    .Where(s => seriesIds.Contains(s.series_Id))
                    .Select(s => new SeriesSummaryDto
                    {
                        SeriesId = s.series_Id,
                        Title = s.series_title, 
                        CoverImage = s.cover_images 
                    })
                    .ToListAsync();

     
                if (summaries.Count != seriesIds.Distinct().Count())
                {
                    var foundIds = summaries.Select(s => s.SeriesId).ToHashSet();
                    var notFoundIds = seriesIds.Distinct().Where(id => !foundIds.Contains(id));
                    _logger.LogWarning("Could not find series summaries for IDs: {NotFoundIds}", string.Join(", ", notFoundIds));
                }


                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch series summaries for IDs: {SeriesIds}", string.Join(",", seriesIds));
                return StatusCode(500, "Internal server error while fetching series summaries.");
            }
        }


    }
}
