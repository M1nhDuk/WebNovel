using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Chapter;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NovelService.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly NovelDbContext _context;
        private readonly ILogger<ChapterController> _logger;

        public ChapterController(IChapterService chapterService, NovelDbContext context, ILogger<ChapterController> logger)
        {
            _chapterService = chapterService;
            _context = context;
            _logger = logger;
        }

        private Guid GetUserIdFromToken()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }


        // === FLOW 1: SERIES -> NOVEL -> CHAPTER ===

        //   [HttpPost("series/{seriesId:int}/novels/{novelId:int}/chapters")]
        [HttpPost("novels/{novelId:int}/chapters")]
        [Authorize]
        public async Task<IActionResult> CreateChapterForNovel([FromRoute] int novelId, [FromBody] ChapterCreateDto dto)
        {
            try
            {
                dto.novelID = novelId;
                dto.series_id = null;
                var result = await _chapterService.CreateChapterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chapter for novel {NovelId}", novelId);
                return BadRequest(new { message = ex.Message });
            }
        }


        //Update
        //  [HttpPut("series/{seriesId:int}/novels/{novelId:int}/chapters/{chapterId:int}")]
        [HttpPut("novels/{novelId:int}/chapters/{chapterId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateChapterForNovel(int novelId, [FromRoute] int chapterId, [FromBody] ChapterUpdateDto dto)
        {
            try
            {
                var uploaderId = GetUserIdFromToken();
                var result = await _chapterService.UpdateChapterAsync(chapterId, dto, uploaderId, novelId: novelId);
                if (result == null) return NotFound(new { message = "Chapter not found within this novel." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update chapter failed for novelId={novelId}, chapterId={chapterId}", novelId, chapterId);
                return BadRequest(new { message = ex.Message });
            }


        }


        //Get(View)
        //[HttpGet("series/{seriesId:int}/novels/{novelId:int}/chapters/{chapterId:int}")]
        [HttpGet("novels/{novelId:int}/chapters/{chapterId:int}")]
        public async Task<ActionResult<ChapterDetailDto>> GetChapterByIdForNovel(int novelId, [FromRoute] int chapterId)
        {
            var result = await _chapterService.GetChapterById(chapterId, novelId: novelId);
            if (result == null) return NotFound(new { message = "Chapter not found within this novel." });
            return Ok(result);
        }


        //Delete
        // [HttpDelete("series/{seriesId:int}/novels/{novelId:int}/chapters/{chapterId:int}")]
        [HttpDelete("novels/{novelId:int}/chapters/{chapterId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteChapterForNovel(int novelId, [FromRoute] int chapterId)
        {
            try
            {
                var uploaderId = GetUserIdFromToken();
                var deleted = await _chapterService.DeleteChapterById(chapterId, uploaderId, novelId: novelId);
                if (!deleted) return NotFound(new { message = "Chapter not found within this novel." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete chapter failed for novelId={novelId}, chapterId={chapterId}", novelId, chapterId);
                return BadRequest(new { message = ex.Message });
            }
        }


        //Reorder
        // [HttpPost("series/{seriesId:int}/novels/{novelId:int}/chapters/reorder")]
        [HttpPost("novels/{novelId:int}/chapters/reorder")]
        [Authorize]
        public async Task<IActionResult> ReorderChaptersForNovel([FromRoute] int novelId, [FromBody] ReorderChaptersRequest request)
        {
            request.novel_Id = novelId;
            request.series_Id = null;
            var result = await _chapterService.ReorderChapterAsync(request);
            if (!result) return BadRequest("Cannot reorder chapters for this novel.");
            return Ok("Chapters reordered successfully.");
        }

        // === FLOW 2: CLASSICSERIES -> CHAPTER ===


        //Create
        [HttpPost("series/{seriesId:int}/chapters")]
        [Authorize]
        public async Task<IActionResult> CreateChapterForClassicSeries([FromRoute] int seriesId, [FromBody] ChapterCreateDto dto)
        {
            
            try
            {
                dto.series_id = seriesId;
                dto.novelID = null;
                var result = await _chapterService.CreateChapterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chapter for series {SeriesId}", seriesId);
                return BadRequest(new { message = ex.Message });
            }
        }


        //Get(View)
        [HttpGet("series/{seriesId:int}/chapters/{chapterId:int}")]
        public async Task<ActionResult<ChapterDetailDto>> GetChapterByIdForSeries(int seriesId, [FromRoute] int chapterId)
        {

            var result = await _chapterService.GetChapterById(chapterId, seriesId: seriesId);
            if (result == null) return NotFound(new { message = "Chapter not found within this series." });
            return Ok(result);

        }


        //Update
        [HttpPut("series/{seriesId:int}/chapters/{chapterId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateChapterForSeries(int seriesId, [FromRoute] int chapterId, [FromBody] ChapterUpdateDto dto)
        {
            try
            {
                var uploaderId = GetUserIdFromToken();
                var result = await _chapterService.UpdateChapterAsync(chapterId, dto, uploaderId, seriesId: seriesId);
                if (result == null) return NotFound(new { message = "Chapter not found within this series." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update chapter failed for seriesId={seriesId}, chapterId={chapterId}", seriesId, chapterId);
                return BadRequest(new { message = ex.Message });
            }
        }


        //Delete
        [HttpDelete("series/{seriesId:int}/chapters/{chapterId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteChapterForSeries([FromRoute] int chapterId, int seriesId)
        {
            try
            {
                var uploaderId = GetUserIdFromToken();
                var deleted = await _chapterService.DeleteChapterById(chapterId, uploaderId, seriesId: seriesId);
                if (!deleted) return NotFound(new { message = "Chapter not found within this series." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete chapter failed for seriesId={seriesId}, chapterId={chapterId}", seriesId, chapterId);
                return BadRequest(new { message = ex.Message });
            }
        }


        //Reorder
        [HttpPost("series/{seriesId:int}/chapters/reorder")]
        [Authorize]
        public async Task<IActionResult> ReorderChaptersForSeries([FromRoute] int seriesId, [FromBody] ReorderChaptersRequest request)
        {
            request.series_Id = seriesId;
            request.novel_Id = null;
            var result = await _chapterService.ReorderChapterAsync(request);
            if (!result) return BadRequest("Cannot reorder chapters for this series.");
            return Ok("Chapters reordered successfully.");
        }
    }
}