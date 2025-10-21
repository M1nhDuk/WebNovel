using AuthService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 15 * 1024 * 1024; //15mb
        public UserController(AuthDbContext context, IWebHostEnvironment environment, ILogger logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("avatar")]
        [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if( file == null ||file.Length == 0) 
            {
                return BadRequest("No file found");
            }

            if ( file.Length > MaxFileSize )
            {
                return BadRequest($"File size exceeds the limit of {MaxFileSize / 1024 / 1024}MB.");
            }

           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
              return Unauthorized();
            }

            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return NotFound("User not found.");
            }


            //Save avatar cũ
            var oldAvatarUrl = user.Avatar;
            var oldThumbnailUrl = user.AvatarThumbnail;


            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
            {
                return BadRequest("Invalid file type for avatar. Only JPG, PNG, and GIF are allowed.");
            }

            // ngăn tên file trùng lặp khi upload file (300x300)
            var mainAvatarFileName = $"{Guid.NewGuid()}_main{extension}";
            var mainAvatarFilePath = Path.Combine(uploads, mainAvatarFileName);

            
            var thumbnailAvatarFileName = $"{Guid.NewGuid()}_thumb{extension}";
            var thumbnailAvatarFilePath = Path.Combine(uploads, thumbnailAvatarFileName);

            using (var originalImage = await Image.LoadAsync(file.OpenReadStream()))
            {
                // Xử lý và lưu avatar chính (300x300)
                using (var mainAvatarImage = originalImage.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Crop
                })))
                {
                    await mainAvatarImage.SaveAsync(mainAvatarFilePath);
                }

                // Xử lý và lưu avatar thumbnail (46x46)
                using (var thumbnailAvatarImage = originalImage.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(46, 46),
                    Mode = ResizeMode.Crop
                })))
                {
                    await thumbnailAvatarImage.SaveAsync(thumbnailAvatarFilePath);
                }
            }

            // Cập nhật đường dẫn vào DB (chỉnh lại)
            user.Avatar = $"{Request.Scheme}://{Request.Host}/uploads/{mainAvatarFileName}";
            user.AvatarThumbnail = $"{Request.Scheme}://{Request.Host}/uploads/{thumbnailAvatarFileName}";

            await _context.SaveChangesAsync();

            try
            {
                // Xóa file avatar  cũ
                if (!string.IsNullOrEmpty(oldAvatarUrl))
                {
                    // Trích xuất tên file từ URL đầy đủ
                    var oldAvatarFileName = Path.GetFileName(new Uri(oldAvatarUrl).LocalPath);
                    var oldAvatarPath = Path.Combine(uploads, oldAvatarFileName);
                    if (System.IO.File.Exists(oldAvatarPath))
                    {
                        System.IO.File.Delete(oldAvatarPath);
                    }
                }

                // Xóa file thumbnail cũ
                if (!string.IsNullOrEmpty(oldThumbnailUrl))
                {
                    var oldThumbnailFileName = Path.GetFileName(new Uri(oldThumbnailUrl).LocalPath);
                    var oldThumbnailPath = Path.Combine(uploads, oldThumbnailFileName);
                    if (System.IO.File.Exists(oldThumbnailPath))
                    {
                        System.IO.File.Delete(oldThumbnailPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old avatar files.");
            }
         

            return Ok(new
            {
                avatarUrl = user.Avatar,
                avatarThumbnailUrl = user.AvatarThumbnail
            });
        }

        [HttpPost("background")]
        [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
        public async Task<IActionResult> UploadBackGroundImage(IFormFile file)
        {
    
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file found");
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest($"File size exceeds the limit of {MaxFileSize / 1024 / 1024}MB.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(Guid.Parse(userId));

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var oldBackgroundUrl = user.BackgroundImage;

            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            
            //Check valid file type
            if (!(new[] { ".jpg", ".jpeg", ".png" }.Contains(extension)))
            {
                return BadRequest("Invalid file type for background image. Only JPG and PNG are allowed.");
            }


            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploads, fileName);


            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(1200, 300),
                    Mode = ResizeMode.Crop
                }));
                await image.SaveAsync(filePath);
            }


            user.BackgroundImage = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            await _context.SaveChangesAsync();

            try
            {
                // Xóa file avatar  cũ
                if (!string.IsNullOrEmpty(oldBackgroundUrl))
                {
                    // Trích xuất tên file từ URL đầy đủ
                    var oldBackgroundFileName = Path.GetFileName(new Uri(oldBackgroundUrl).LocalPath);
                    var oldBackgroundPath = Path.Combine(uploads, oldBackgroundFileName);
                    if (System.IO.File.Exists(oldBackgroundPath))
                    {
                        System.IO.File.Delete(oldBackgroundPath);
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old avatar files.");
            }

            return Ok(new { url = user.BackgroundImage });
        }
    }
}

