using InteractionService.Service.Inteface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.Comment;
using Shareds.DTOs.NovelSeries;
using System.Security.Claims;

namespace InteractionService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        private string GetUserRoleFromToken()
        {
            var roleStr = User.FindFirstValue(ClaimTypes.Role);
            return roleStr ?? "User";
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

        // --- Comment cho Series ---

        [HttpPost("series/{seriesId:int}/comments")]
        [Authorize]
        public async Task<ActionResult<CommentDto>> CreateSeriesComment(int seriesId, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var comment = await _commentService.CreateCommentAsync(userId, seriesId, null, dto);
                
                return CreatedAtAction(nameof(GetCommentById), new { commentId = comment.CommentId }, comment);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for Series {SeriesId}", seriesId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("series/{seriesId:int}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetSeriesComments(int seriesId, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            try
            {
                var comments = await _commentService.GetCommentsAsync(seriesId, null, page, size);
                return Ok(comments);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for Series {SeriesId}", seriesId);
                return StatusCode(500, "Internal server error");
            }
        }



        // --- Comment cho Chapter ---

        [HttpPost("chapters/{chapterId:int}/comments")]
        [Authorize]
        public async Task<ActionResult<CommentDto>> CreateChapterComment(int chapterId, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var comment = await _commentService.CreateCommentAsync(userId, null, chapterId, dto);
                return CreatedAtAction(nameof(GetCommentById), new { commentId = comment.CommentId }, comment);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for Chapter {ChapterId}", chapterId);
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("chapters/{chapterId:int}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetChapterComments(int chapterId, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            try
            {
                var comments = await _commentService.GetCommentsAsync(null, chapterId, page, size);
                return Ok(comments);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for Chapter {ChapterId}", chapterId);
                return StatusCode(500, "Internal server error");
            }
        }




        [HttpGet("comments/{parentCommentId:guid}/replies")]
        [Authorize]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetCommentReplies(Guid parentCommentId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var replies = await _commentService.GetRepliesAsync(parentCommentId, page, size);
                return Ok(replies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting replies for comment {ParentCommentId}", parentCommentId);
                return StatusCode(500, "Internal server error");
            }
        }




        // --- Manage Comment (Chung) ---


        [HttpGet("comments/{commentId:guid}", Name = "GetCommentById")]
        [ApiExplorerSettings(IgnoreApi = true)] 
        public async Task<ActionResult<CommentDto>> GetCommentById(Guid commentId)
        {
            return NotFound(); 
        }


        [HttpPut("comments/{commentId:guid}")]
        [Authorize]
        public async Task<ActionResult<CommentDto>> UpdateComment(Guid commentId, [FromBody] UpdateCommentDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var updatedComment = await _commentService.UpdateCommentAsync(commentId, userId, dto);
                if (updatedComment == null)
                {
                    return NotFound(new { message = "Comment not found." });
                }
                return Ok(updatedComment);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("comments/{commentId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var userRole = GetUserRoleFromToken();
                var success = await _commentService.DeleteCommentAsync(commentId, userId, userRole);
                if (!success)
                {
                    return NotFound(new { message = "Comment not found." });
                }
                return NoContent(); 
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
