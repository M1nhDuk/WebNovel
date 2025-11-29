using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.UserService;
using System.Security.Claims;
using UserService.UserSettingService.Interface;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user/favorites")]
    [Authorize] 
    public class FavoritesController: ControllerBase
    {
        private readonly IUserFavoriteService _favoriteService;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(IUserFavoriteService favoriteService, ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
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

        [HttpPost("toggle")]
        public async Task<ActionResult> ToggleFavorite([FromBody] AddFavoriteDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _favoriteService.ToggleFavoriteAsync(userId, dto);

                if (result.IsFavorited)
                {
                   
                    return Ok(new
                    {
                        message = "Đã yêu thích series này/Unfollow this series.",
                        isFavorited = result.IsFavorited, 
                        data = result.Data
                    });
                }
                else
                {
                    // Vừa xóa thành công
                    return Ok(new
                    {
                        message = "Đã bỏ yêu thích series này.",
                        isFavorited = result.IsFavorited 
                    });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (HttpRequestException ex) 
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Connection Erro" });
            }
            catch (KeyNotFoundException ex) //Series không tồn tại
            {
                return NotFound(new { message = ex.Message });
            }           
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet]
        public async Task<ActionResult<PagedResult<UserFavoriteDto>>> GetUserFavorites(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var favorites = await _favoriteService.GetAllFavoriteAsync(userId, page, pageSize);

                return Ok(favorites);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged favorites for User {UserId}", GetUserIdFromToken());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete] 
        public async Task<IActionResult> RemoveSelectedFavorites([FromBody] RemoveFavoritesDto dto)
        {
            if (dto == null || dto.SeriesIds == null || !dto.SeriesIds.Any())
            {
                return BadRequest(new { message = "Series required to delete." });
            }

            try
            {
                var userId = GetUserIdFromToken();

                var deletedCount = await _favoriteService.RemoveFavoriteAsync(userId, dto.SeriesIds);

                if (deletedCount == 0)
                {
                    return NotFound(new { message = "Cannot find series to remove" });
                }

                return Ok(new
                {
                    message = $"Remove successfully {deletedCount} series from favorite list.",
                    deletedCount = deletedCount
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("sync-counts")]
        public async Task<IActionResult> SyncFavoriteCounts([FromBody] List<FavoriteReadUpdateDto> updates)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var success = await _favoriteService.SyncFavoriteCountsAsync(userId, updates);
                if (!success)
                {
                    return Ok("No counts to sync.");
                }
                return Ok("Favorite counts reset successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}

