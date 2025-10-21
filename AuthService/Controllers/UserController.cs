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
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserController(AuthDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
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

            // Tạo tên file duy nhất cho phiên bản chính (300x300)
            var mainAvatarFileName = $"{Guid.NewGuid()}_main{extension}";
            var mainAvatarFilePath = Path.Combine(uploads, mainAvatarFileName);

            // Tạo tên file duy nhất cho phiên bản thumbnail (46x46)
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

            return Ok(new
            {
                avatarUrl = user.Avatar,
                avatarThumbnailUrl = user.AvatarThumbnail
            });
        }

        [HttpPost("background")]
        public async Task<IActionResult> UploadBackGroundImage(IFormFile file)
        {
            // Kích thước mong muốn cho ảnh nền là 1200x300
            // Logic này giữ nguyên như trước
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

            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
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

            return Ok(new { url = user.BackgroundImage });
        }
    }
}

