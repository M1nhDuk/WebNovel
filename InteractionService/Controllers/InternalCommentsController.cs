using InteractionService.Service.Inteface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace InteractionService.Controllers
{
    [ApiController]
    [Route("api/internal/comments")]
    public class InternalCommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<InternalCommentsController> _logger;

        public InternalCommentsController(ICommentService commentService, ILogger<InternalCommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }



        [HttpDelete("by-series/{seriesId:int}")]
        public async Task<IActionResult> DeleteCommentsBySeries(int seriesId)
        {
            _logger.LogInformation("Received internal request to delete comments for SeriesId {SeriesId}", seriesId);

            await _commentService.DeleteCommentsBySeriesAsync(seriesId);

            return Ok(new { message = $"Comments for SeriesId {seriesId} processed for deletion." });
        }



        [HttpDelete("by-chapter/{chapterId:int}")]
        public async Task<IActionResult> DeleteCommentsByChapter(int chapterId)
        {
            _logger.LogInformation("Received internal request to delete comments for ChapterId {ChapterId}", chapterId);

            await _commentService.DeleteCommentsByChapterAsync(chapterId);

            return Ok(new { message = $"Comments for ChapterId {chapterId} processed for deletion." });
        }



        [HttpDelete("admin/comments/{id:guid}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AdminDeleteComment(Guid id)
        {
            _logger.LogWarning("Admin executing delete for CommentId {CommentId}", id);

            var success = await _commentService.AdminDeleteCommentAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Comment not found." });
            }
            return NoContent();
        }
    }

}
