using Microsoft.AspNetCore.Mvc;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Chapter;
using System;
using System.Threading.Tasks;

namespace NovelService.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly ILogger<ChapterController> _logger;

        public ChapterController(IChapterService chapterService, ILogger<ChapterController> logger)
        {
            _chapterService = chapterService;
            _logger = logger;
        }

        // === FLOW 1: SERIES -> NOVEL -> CHAPTER ===

        [HttpPost("series/{seriesId:int}/novels/{novelId:int}/chapters")]
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
        [HttpPut("series/{seriesId:int}/novels/{novelId:int}/chapters/{chapterId:int}")]
        public async Task<IActionResult> UpdateChapterForNovel([FromRoute] int chapterId, [FromBody] ChapterUpdateDto dto)
        {
            return await UpdateChapterInternal(chapterId, dto);
        }


        //Get(View)
        [HttpGet("series/{seriesId:int}/novels/{novelId:int}/chapters/{chapterId:int}")]
        public async Task<ActionResult<ChapterDetailDto>> GetChapterByIdForNovel([FromRoute] int chapterId)
        {
            return await GetChapterByIdInternal(chapterId);
        }


        //Delete
        [HttpDelete("series/{seriesId:int}/novels/{novelId:int}/chapters/{chapterId:int}")]
        public async Task<IActionResult> DeleteChapterForNovel([FromRoute] int chapterId)
        {
            return await DeleteChapterByIdInternal(chapterId);
        }


        //Reorder
        [HttpPost("series/{seriesId:int}/novels/{novelId:int}/chapters/reorder")]
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
        public async Task<ActionResult<ChapterDetailDto>> GetChapterByIdForSeries([FromRoute] int chapterId)
        {
            return await GetChapterByIdInternal(chapterId);
        }


        //Update
        [HttpPut("series/{seriesId:int}/chapters/{chapterId:int}")]
        public async Task<IActionResult> UpdateChapterForSeries([FromRoute] int chapterId, [FromBody] ChapterUpdateDto dto)
        {
            return await UpdateChapterInternal(chapterId, dto);
        }


        //Delete
        [HttpDelete("series/{seriesId:int}/chapters/{chapterId:int}")]
        public async Task<IActionResult> DeleteChapterForSeries([FromRoute] int chapterId)
        {
            return await DeleteChapterByIdInternal(chapterId);
        }


        //Reorder
        [HttpPost("series/{seriesId:int}/chapters/reorder")]
        public async Task<IActionResult> ReorderChaptersForSeries([FromRoute] int seriesId, [FromBody] ReorderChaptersRequest request)
        {
            request.series_Id = seriesId;
            request.novel_Id = null;
            var result = await _chapterService.ReorderChapterAsync(request);
            if (!result) return BadRequest("Cannot reorder chapters for this series.");
            return Ok("Chapters reordered successfully.");
        }

        // === PRIVATE HELPER METHODS ===

        private async Task<IActionResult> UpdateChapterInternal(int chapterId, ChapterUpdateDto dto)
        {
            if (dto == null) return BadRequest("Invalid request data");
            try
            {
                int uploaderId = 1;
                var result = await _chapterService.UpdateChapterAsync(chapterId, dto, uploaderId);
                if (result == null) return NotFound(new { message = "Chapter not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Chapter failed for id={Id}", chapterId);
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<ActionResult<ChapterDetailDto>> GetChapterByIdInternal(int chapterId)
        {
            var chapter = await _chapterService.GetChapterById(chapterId);
            if (chapter == null) return NotFound();
            return Ok(chapter);
        }

        private async Task<IActionResult> DeleteChapterByIdInternal(int chapterId)
        {
            try
            {
                int uploaderId = 1;
                var deleted = await _chapterService.DeleteChapterById(chapterId, uploaderId);
                if (!deleted) return NotFound(new { message = "Chapter not found" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Chapter failed for id={Id}", chapterId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}