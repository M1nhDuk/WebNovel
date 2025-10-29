using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelService.Controllers.NovelService.Controllers;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Novel;
using System;
using System.Security.Claims;

namespace PublicationService.Controllers
{
    [ApiController]
    [Route("api/series/{series_Id:int}/novels")]
    public class NovelController : ControllerBase
    {
        private readonly NovelDbContext _context;
        private readonly INovelService _novelService;
        private readonly ILogger<NovelController> _logger;
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 15 * 1024 * 1024;
        public NovelController(NovelDbContext context, INovelService novelService, ILogger<NovelController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _novelService = novelService;
            _logger = logger;
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


        // POST: api/novels
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<NovelDetailDto>> CreateNovel(int series_Id, [FromBody] CreateNovelDto dto)
        {

            if (dto == null)
                return BadRequest("Invalid request data");

            //var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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
        [Authorize]
        public async Task<IActionResult> UpdateNovel(int id, [FromBody] NovelUpdateDto dto, [FromRoute] int series_Id)
        {

            if (dto == null)
                return BadRequest("Invalid request data");
            try
            {
                var uploaderId = GetUserIdFromToken();
                var result = await _novelService.UpdateNovelAsync(id, dto, uploaderId, series_Id);
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
        [Authorize]
        public async Task<IActionResult> DeleteNovelById([FromRoute] int series_Id, [FromRoute] int novel_Id)
        {
            try
            {
                var uploader_Id = GetUserIdFromToken();
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
        [Authorize]
        public async Task<IActionResult> ReorderNovels([FromBody] NovelReoderRequest request)
        {
            var result = await _novelService.ReorderNovelsAsync(request);
            if (!result) return BadRequest("Cannot reorder novels.");
            return Ok("Reorder success");
        }



        [HttpPost("series/{id:int}/cover")]
        [Authorize]
        [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
        public async Task<IActionResult> UploadNovelCover(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không có file nào ???c t?i lên." });
            }


            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { message = $"Kích th??c file v??t quá gi?i h?n {MaxFileSize / 1024 / 1024}MB." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Lo?i file không h?p l?. Ch? ch?p nh?n .jpg, .jpeg, .png." });
            }

            try
            {
                var currentUserId = GetUserIdFromToken();

                var novel = await _context.Novels
                                      .Include(n => n.NovelSeries) 
                                      .FirstOrDefaultAsync(n => n.novel_Id == id);

     

                if ( novel == null)
                {
                    return NotFound(new { message = "Không tìm th?y novel." });
                }

                if(novel.NovelSeries == null)
                {
                    return BadRequest(new { message = "Novel này không thu?c v? series nào." });
                }

                if (novel.NovelSeries.uploader_id != currentUserId)
                {
                    return Forbid("B?n không có quy?n thay ??i ?nh bìa cho novel này.");
                }

               
                var uploadsFolderPath = Path.Combine(_environment.WebRootPath, "images", "covers");
             
                if (!Directory.Exists(uploadsFolderPath))
                    Directory.CreateDirectory(uploadsFolderPath);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";

                var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var oldRelativePath = novel.cover_images;

                var defaultPath = "/images/covers/default_cover.jpg";

                if (!string.IsNullOrEmpty(oldRelativePath) && oldRelativePath.Trim() != defaultPath)
                {
                    var oldFileName = Path.GetFileName(oldRelativePath);
                    var oldFilePath = Path.Combine(uploadsFolderPath, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); }
                        catch (IOException ex) { _logger.LogWarning(ex, "Không th? xóa file ?nh bìa c? c?a novel: {OldFilePath}", oldFilePath); }
                    }
                }


                //Update database
                var relativePath = $"/images/covers/{uniqueFileName}";

                novel.cover_images = relativePath;
                
                novel.updated_at = DateTime.Now;

                _context.Novels.Update(novel);

                novel.NovelSeries.updated_at = DateTime.UtcNow;
                _context.Novel_Series.Update(novel.NovelSeries);


                await _context.SaveChangesAsync();

                var fullUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";
                return Ok(new { coverUrl = fullUrl });


            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "?ã x?y ra l?i máy ch? trong quá trình t?i file." });
            }
        }
    }
}
