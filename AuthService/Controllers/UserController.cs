using AuthService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using AuthService.Services.Interface; 
using Shareds.DTOs.AuthService; 
using Shareds.DTOs;
using Microsoft.EntityFrameworkCore;
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
        private const long MaxFileSize = 15 * 1024 * 1024; 
        private readonly IAutheService _autheService;
        public UserController(AuthDbContext context, IWebHostEnvironment environment, ILogger<UserController> logger, IAutheService autheService)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _autheService = autheService; 
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


        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var userProfile = new UserProfileDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Avatar = user.Avatar,
                    AvatarThumbnail = user.AvatarThumbnail,
                    BackgroundImage = user.BackgroundImage,
                    Role = user.Role
                };

                return Ok(userProfile);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching profile for user {UserId}", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }




        [HttpPost("change-username")]
        public async Task<ActionResult<TokenResponseDto>> ChangeUsername([FromBody] ChangeUsernameDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = GetUserIdFromToken();
                var tokenResponse = await _autheService.ChangeUsernameAsync(userId, dto);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to change username for user {UserId}", GetUserIdFromToken());
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = GetUserIdFromToken();
                var success = await _autheService.ChangePasswordAsync(userId, dto);

                if (!success)
                {
                    return BadRequest(new { message = "Password not correct." });
                }

                return Ok(new { message = "Change Password successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", GetUserIdFromToken());
                return StatusCode(500, "Lỗi máy chủ nội bộ.");
            }
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

            // ngăn tên file trùng lặp khi upload file 
            var mainAvatarFileName = $"{Guid.NewGuid()}_main{extension}";
            var mainAvatarFilePath = Path.Combine(uploads, mainAvatarFileName);

            
            var thumbnailAvatarFileName = $"{Guid.NewGuid()}_thumb{extension}";
            var thumbnailAvatarFilePath = Path.Combine(uploads, thumbnailAvatarFileName);

            using (var originalImage = await Image.LoadAsync(file.OpenReadStream()))
            {
                // Xử lý và lưu avatar chính 
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

            // Cập nhật đường dẫn vào DB 
            user.Avatar = $"/uploads/{mainAvatarFileName}";
            user.AvatarThumbnail = $"/uploads/{thumbnailAvatarFileName}";

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


            user.BackgroundImage = $"/uploads/{fileName}";
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

        //Xem profile user #
        [HttpGet("{id:guid}/public")]
        [AllowAnonymous] 
        public async Task<ActionResult<UserProfileDto>> GetPublicProfile(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var userProfile = new UserProfileDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Avatar = user.Avatar,
                    AvatarThumbnail = user.AvatarThumbnail,
                    BackgroundImage = user.BackgroundImage,
                    Role = "User" 
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching public profile for user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }


}

