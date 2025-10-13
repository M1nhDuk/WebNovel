using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NovelService.Data;
using NovelService.Service;
using NovelService.Service.Interfaces;
using PublicationService.Controllers;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.Novel;
using System;


namespace NovelService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChapterController : ControllerBase
    {
        private readonly IClassicSeries _classicService;
        private readonly IChapterService _chapterService;
        private readonly ILogger<ChapterController> _logger;

        public ChapterController(IClassicSeries classicService, IChapterService chapterService, ILogger<ChapterController> logger)
        {
            _chapterService = chapterService;
            _logger = logger;
            _classicService = classicService;
        }


        //POST (Chapter thuộc novel)
        [HttpPost("novels/{novel_Id:int}/chapters")]
        public async Task<IActionResult> CreateChapter(int novel_Id, [FromBody] ChapterCreateDto dto)
        {
            var result = await _chapterService.CreateChapterAsync(novel_Id, dto);
            return Ok(result);
        }


        //Put
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChapter(int id, [FromBody] ChapterUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data");
            try
            {
                int uploaderId = 1;
                var result = await _chapterService.UpdateChapterAsync(id, dto, uploaderId);
                if (result == null)
                    return NotFound(new { message = "Chapter not found" });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Chapter failed for id={Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterDetailDto>> GetChapterById(int id)
        {
            var chapter = await _chapterService.GetChapterById(id);
            if (chapter == null) return NotFound();

            return Ok(chapter);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapterById(int id)
        {
            try
            {
                int uploaderId = 1;
                var delete = await _chapterService.DeleteChapterById(id, uploaderId);

                if (!delete)
                    return NotFound(new { message = "Chapter not found" });

                return NoContent();
            }


            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Chapter failed for id={Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderChapter([FromBody] ReorderChaptersRequest request)
        {
            var result = await _chapterService.ReorderChapterAsync(request);
            if (!result) return BadRequest("Cannot reorder chapter.");
            return Ok("Reorder success");
        }

        //-------------------------------------------------------------------------------------------------------

        //POST(Chapter trực thuộc series)

        // tạo chapter cho traditional series
        [HttpPost("series/{seriesId:int}/chapters")]
        public async Task<IActionResult> CreateForSeries(int seriesId, [FromBody] ChapterCreateDto dto)
        {
            var result = await _classicService.CreateChapterForCSAsync(seriesId, dto);
            return Ok(result);
        }
    }
}
