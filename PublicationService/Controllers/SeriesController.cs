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
using SixLabors.ImageSharp.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 

namespace NovelService.Controllers
{
        [ApiController]
        [Route("api")]
        public class SeriesController : ControllerBase
        {
            private readonly IClassicSeries _classicSeries;
            private readonly INovelSeriesService _seriesService;
            private readonly ILogger<SeriesController> _logger;
            private readonly NovelDbContext _context; 
            private readonly IWebHostEnvironment _environment;

            private const long MaxFileSize = 15 * 1024 * 1024; // 15MB
            public SeriesController(IClassicSeries classicSeries, 
                INovelSeriesService novelSeriesService, 
                ILogger<SeriesController> logger, 
                NovelDbContext context,
                IWebHostEnvironment environment)
            {
                _classicSeries = classicSeries;
                _seriesService = novelSeriesService;
                _logger = logger;
                _context = context; 
                _environment = environment; 
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


            [HttpPost("series")]
            [Authorize]
            public async Task<ActionResult<NovelSeriesDetailDto>> CreateSeries([FromBody] CreateSeriesDto dto)
            {
                try
                {
                    var uploaderId = GetUserIdFromToken();

                    var result = await _seriesService.CreateSeriesAsync(dto, uploaderId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // PUT: api/series/{id}
            [HttpPut("series/{id:int}")]
            [Authorize]
            public async Task<IActionResult> UpdateSeries(int id, [FromBody] UpdateNovelService dto)
            {
                try
                {
                    var uploaderId = GetUserIdFromToken();

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

            //Get
            [HttpGet("series/{id:int}")]
            public async Task<ActionResult<NovelSeriesDetailDto>> GetByIdAsync(int id)
            {

                //Tăng view khi xem chi tiết
                var increaseView = await _context.Novel_Series
                    .Where(s => s.series_Id == id)
                    .ExecuteUpdateAsync(updates => updates.SetProperty(
                        s => s.views,
                        s => s.views + 1
                    ));


                var series = await _seriesService.GetByIdAsync(id);
                if (series == null) return NotFound();

                return Ok(series);                
            }


            //Delete
            [HttpDelete("series/{id:int}")]
            [Authorize]
            public async Task<IActionResult> DeleteSeries(int id)
            {
                try
                {
                    var uploaderId = GetUserIdFromToken();
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

            [HttpGet("user/series")] 
            [Authorize] 
            public async Task<ActionResult<PagedResult<SeriesListDto>>> GetMySeries(int pageNumber = 1, int pageSize = 10)
            {
                try
                {
                    var uploaderId = GetUserIdFromToken(); 
                    var result = await _seriesService.GetSeriesByUploaderAsync(uploaderId, pageNumber, pageSize);
                    return Ok(result);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Could not get UserId from token even though authorized.");
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving series for uploader {UploaderId}", GetUserIdFromToken());
                    return StatusCode(500, "An error occurred while retrieving your series.");
                }
            }

            [HttpPost("series/{id:int}/cover")]
            [Authorize] 
            [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
            public async Task<IActionResult> UploadSeriesCover(int id, IFormFile file)
            {
                //Validate File
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No upload file foud." });
                }

                if (file.Length > MaxFileSize)
                {
                    return BadRequest(new { message = $"Only allow 15MB file {MaxFileSize / 1024 / 1024}MB." });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Only allow .jpg, .jpeg, .png." });
                }

                try
                {
                    var uploaderId = GetUserIdFromToken();

                   
                    var series = await _context.Novel_Series.FindAsync(id);
                    if (series == null)
                    {
                        return NotFound(new { message = "Series not found." });
                    }
                    if (series.uploader_id != uploaderId)
                    {
                       
                        return Forbid("You do not have permit to change.");
                    }

                    //Lưu
                    var uploadsFolderPath = Path.Combine(_environment.WebRootPath, "images", "covers");

                    if (!Directory.Exists(uploadsFolderPath))
                    {
                        Directory.CreateDirectory(uploadsFolderPath);
                    }

                    // Tạo tên file duy nhất
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

                    // Lưu file vào đường dẫn vật lý
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    //Xóa File Ảnh Bìa Cũ 
                    var oldRelativePath = series.cover_images;
                    var defaultPath = "/images/covers/default_cover.jpg"; 

                    if (!string.IsNullOrEmpty(oldRelativePath) && oldRelativePath.Trim() != defaultPath)
                    {
                        var oldFileName = Path.GetFileName(oldRelativePath); 
                        var oldFilePath = Path.Combine(uploadsFolderPath, oldFileName);

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                                _logger.LogInformation("Remove old cover images: {OldFilePath}", oldFilePath);
                            }
                            catch (IOException ex)
                            {
                                
                                _logger.LogWarning(ex, "Cannot remove old cover images: {OldFilePath}", oldFilePath);
                            }
                        }
                    }

                    // Lưu đường dẫn tương đối vào DB
                    var relativePath = $"/images/covers/{uniqueFileName}";

                    series.cover_images = relativePath;

                    series.updated_at = DateTime.UtcNow; 

                    _context.Novel_Series.Update(series);

                    await _context.SaveChangesAsync();

                    //Trả về URL/Path mới cho FE
                    var fullUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";
                    return Ok(new { coverUrl = fullUrl }); 


                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message }); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro when upload coeve images of series {SeriesId}", id);
                    return StatusCode(500, new { message = "Erro when uploading file." }); 
                }
            }



            [HttpGet("series/search")]
            public async Task<IActionResult> SearchSeries(
           [FromQuery] string keyword,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10)
            {
                var result = await _seriesService.SearchSeries(keyword, pageNumber, pageSize);
                return Ok(result);
            }

//--------------------------------------------------------------------------------------------------------------------------------
            //Classic Series

            [HttpPost("series/classic")]
            [Authorize]
            public async Task<ActionResult<ClassicSeriesDetailDto>> CreateClassicSeries([FromBody] CreateTraditionalSeriesDto dto)
            {
                try
                {
                    var uploaderId = GetUserIdFromToken();

                    var result = await _classicSeries.CreateTraditionalSeriesAsync(dto, uploaderId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpPut("series/{id:int}/classic")]
            [Authorize]
            public async Task<IActionResult> UpdateClassicSeries(int id, [FromBody] UpdateClassicSeriesDto dto)
            {
                try
                {
                    var uploaderId = GetUserIdFromToken();
                    var result = await _classicSeries.UpdateClassicSeriesAsync(id, dto, uploaderId);
                    if (result == null)
                        return NotFound(new { message = "Classic Series not found or you are not authorized." });

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UpdateClassicSeries failed for id={Id}", id);
                    return BadRequest(new { message = ex.Message });
                }
            }


        }
    }