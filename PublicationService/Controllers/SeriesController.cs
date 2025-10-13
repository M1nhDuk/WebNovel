using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NovelService.Data;
using NovelService.Service;
using NovelService.Service.Interfaces;
using Shareds.DTOs;
using Shareds.DTOs.ClassicSeries;
using Shareds.DTOs.NovelSeries;
using System;
using System.Security.Claims;

namespace NovelService.Controllers
{

    namespace NovelService.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class SeriesController : ControllerBase
        {
            private readonly IClassicSeries _classicSeries;
            private readonly INovelSeriesService _seriesService;
            private readonly ILogger<SeriesController> _logger;

            public SeriesController(IClassicSeries classicSeries, INovelSeriesService novelSeriesService, ILogger<SeriesController> logger)
            {
                _classicSeries = classicSeries;
                _seriesService = novelSeriesService;
                _logger = logger;
            }

            [HttpPost]
            public async Task<ActionResult<NovelSeriesDetailDto>> CreateSeries([FromBody] CreateSeriesDto dto)
            {
                try
                {
                    //sau này lấy uploaderId từ token
                    int uploaderId = 1;

                    var result = await _seriesService.CreateSeriesAsync(dto, uploaderId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // PUT: api/series/{id}
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateSeries(int id, [FromBody] UpdateNovelService dto)
            {
                try
                {
                    int uploaderId = 1;

                    var result = await _seriesService.UpdateSeriesAsync(id, dto, uploaderId);
                    if (result == null)
                        return NotFound(new { message = "Series not found" });

                    return Ok(result);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Forbid(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UpdateSeries failed for id={Id}", id);
                    return BadRequest(new { message = ex.Message });
                }
            }


            [HttpGet("{id}")]
            public async Task<ActionResult<NovelSeriesDetailDto>> GetByIdAsync(int id)
            {
                var series = await _seriesService.GetByIdAsync(id);
                if (series == null) return NotFound();

                return Ok(series);                
            }


            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteSeries(int id)
            {
                try
                {
                    int uploaderId = 1; // sau này lấy từ token
                    var result = await _seriesService.DeleteSeriesById(id, uploaderId);
                    if (!result)
                        return NotFound(new { message = "Series not found" });

                    return Ok(new { message = "Series deleted successfully" });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Forbid(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Delete Series failed for id={Id}", id);
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpGet("series")]
            public async Task<IActionResult> GetAllSeries(
                 int pageNumber = 1,
                 int pageSize = 10,
                 [FromQuery] SeriesFilterDto filter = null,
                 SeriesSortBy sortBy = SeriesSortBy.Title,
                 
                 bool isAscending = true)
            {
                var result = await _seriesService.GetAllSeriesAsync(pageNumber, pageSize, filter, sortBy, isAscending);
                return Ok(result);
            }



            [HttpGet("search")]
            public async Task<IActionResult> SearchSeries(
           [FromQuery] string keyword,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10)
            {
                var result = await _seriesService.SearchSeries(keyword, pageNumber, pageSize);
                return Ok(result);
            }

//------------------------------------------------------------------------------------------------------
            //Classic Series

            [HttpPost]
            public async Task<ActionResult<ClassicSeriesDetailDto>> CreateClassicSeries([FromBody] CreateTraditionalSeriesDto dto)
            {
                try
                {
                    //sau này lấy uploaderId từ token
                    int uploaderId = 1;

                    var result = await _classicSeries.CreateTraditionalSeriesAsync(dto, uploaderId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

        }
    }
}