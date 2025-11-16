using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.UserService;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using UserService.Data;
using UserService.Models;


namespace UserService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("api/user")]
    [Authorize] 
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly ILogger<UserController> _logger;

        private static readonly List<string> ValidFonts = new List<string> { "Noto Sans", "Times New Roman", "Merriweather", "Lora", "Roboto" };

        private static readonly List<string> ValidAlignments = new List<string> { "left", "center", "right", "justify" };

        public UserController(UserDbContext context, ILogger<UserController> logger)
        {
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


        [HttpGet("settings")]
        public async Task<ActionResult<UserSettingDto>> GetUserSetting()
        {
            try
            {
                var userId = GetUserIdFromToken();

                var settings = await _context.UserSettings.FindAsync(userId);
                if (settings == null)
                {
                    _logger.LogInformation("No settings found for user {UserId}. Creating defaults.", userId);
                    var defaultSettings = new UserSetting
                    {
                        UserId = userId,
                        FontFamily = "Times New Roman",
                        FontSize = 18,
                        BackgroundColor = "#FFFFFF",
                        FontColor = "#000000",
                        Alignment = "left",
                        PaddingPx = 0
                    };
                    _context.UserSettings.Add(defaultSettings);
                    await _context.SaveChangesAsync();
                    return Ok(MapToDto(defaultSettings));
                }
                return Ok(MapToDto(settings));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user settings for {UserId}.", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("settings")]
        public async Task<ActionResult<UserSettingDto>> UpdateUserSettings([FromBody] UpdateUserSettingDto dto)
        {
            var userId = GetUserIdFromToken();

            try
            {
                var settings = await _context.UserSettings.FindAsync(userId);
                if (settings == null)
                {
                    settings = new UserSetting { UserId = userId };
                    _context.UserSettings.Add(settings);
                }

                if (dto.BackgroundColor != null)
                    settings.BackgroundColor = dto.BackgroundColor;

                if (dto.FontColor != null)
                    settings.FontColor = dto.FontColor;

                if(dto.FontFamily != null)
                {
                    if (!ValidFonts.Contains(dto.FontFamily))
                        throw new ValidationException("Invalid Font Family.");
                    settings.FontFamily = dto.FontFamily;
                }

                if(dto.FontSize.HasValue)
                {
                    if(dto.FontSize.Value < 0 || dto.FontSize.Value > 100)
                        throw new ValidationException("Font Size must be between 0 and 100.");
                    settings.FontSize = dto.FontSize.Value;
                }

                if(dto.Alignment != null)
                {
                    if (!ValidAlignments.Contains(dto.Alignment))
                        throw new ValidationException("Invalid Aligment");
                    settings.Alignment = dto.Alignment;
                }

                if(dto.PaddingPx.HasValue)
                {
                    if(dto.PaddingPx.Value < 0 || dto.PaddingPx.Value > 200)
                        throw new ValidationException("Padding must be between 0 and 200, and a multiple of 20.");
                    settings.PaddingPx = dto.PaddingPx.Value;
                }

                await _context.SaveChangesAsync();
                return Ok(MapToDto(settings));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for user {UserId}: {Message}", GetUserIdFromToken(), ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user settings for {UserId}.", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }

        // Helper map 
        private UserSettingDto MapToDto(UserSetting settings)
        {
            return new UserSettingDto
            {
                UserId = settings.UserId,
                FontFamily = settings.FontFamily,
                FontSize = settings.FontSize,
                BackgroundColor = settings.BackgroundColor,
                FontColor = settings.FontColor,
                Aligment = settings.Alignment,
                PaddingPx = settings.PaddingPx
            };
        }
    }
}
