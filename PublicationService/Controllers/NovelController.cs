using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelService.Controllers.NovelService.Controllers;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Novel;
using System;

namespace PublicationService.Controllers
{
    [ApiController]
    [Route("api/series/{series_Id:int}/novels")]
    public class NovelController : ControllerBase
    {
        private readonly NovelDbContext _context;
        private readonly INovelService _novelService;
        private readonly ILogger<NovelController> _logger;
        public NovelController(NovelDbContext context ,INovelService novelService, ILogger<NovelController> logger)
        {
            _context = context;
            _novelService = novelService;
            _logger = logger;
        }

        // POST: api/novels
        [HttpPost]
        public async Task<ActionResult<NovelDetailDto>> CreateNovel(int series_Id, [FromBody] CreateNovelDto dto)
        {

            if (dto == null)
                return BadRequest("Invalid request data");

            // var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            dto.series_Id = series_Id;
            
            try
            {
                var result = await _novelService.CreateNovelAsync(dto, series_Id);
                return StatusCode(201, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{novel_Id:int}")]
        public async Task<IActionResult>UpdateNovel(int id, [FromBody] NovelUpdateDto dto, [FromRoute] int series_Id)
        {
            
            if (dto == null)
                return BadRequest("Invalid request data");
            try
            {
                int uploaderId = 1;
                var result = await _novelService.UpdateNovelAsync(id, dto, uploaderId, series_Id );
                if (result == null)
                    return NotFound(new { message = "Novel not found" });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Novel failed for id={Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/novels/{id}
        [HttpGet("{novel_Id:int}")]
        public async Task<ActionResult<NovelDetailDto>> GetNovelById([FromRoute] int series_Id, [FromRoute] int novel_Id)
        {
            var novel = await _novelService.GetNovelByID(novel_Id, series_Id);

            if (novel == null) return BadRequest(new { message = "Novel not found" });

            if (novel.series_Id != series_Id) return BadRequest(new { message = "Series id not found" }); 


            return Ok(novel);
        }


        [HttpDelete("{novel_Id:int}")]
        public async Task<IActionResult> DeleteNovelById([FromRoute] int series_Id, [FromRoute] int novel_Id)
        {
            try
            {
                int uploader_Id = 1;
                var delete = await _novelService.DeleteNovelAsync(novel_Id, uploader_Id, series_Id);

                
                if (!delete)
                    return NotFound(new { message = "Can not delete" });

                return NoContent();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Chapter failed for id={Id}", novel_Id);
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderNovels([FromBody] NovelReoderRequest request)
        {
            var result = await _novelService.ReorderNovelsAsync(request);
            if (!result) return BadRequest("Cannot reorder novels.");
            return Ok("Reorder success");
        }

    }
}
