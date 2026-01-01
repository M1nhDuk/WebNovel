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

        //Delete all comment, 

        //When Delete Series
        [HttpDelete("by-series/{seriesId:int}")]
        public async Task<IActionResult> DeleteCommentsBySeries(int seriesId)
        {
            _logger.LogInformation("Received internal request to delete comments for SeriesId {SeriesId}", seriesId);

            await _commentService.DeleteCommentsBySeriesAsync(seriesId);

            return Ok(new { message = $"Comments for SeriesId {seriesId} processed for deletion." });
        }


        //When Delete chapter
        [HttpDelete("by-chapter/{chapterId:int}")]
        public async Task<IActionResult> DeleteCommentsByChapter(int chapterId)
        {
            _logger.LogInformation("Received internal request to delete comments for ChapterId {ChapterId}", chapterId);

            await _commentService.DeleteCommentsByChapterAsync(chapterId);

            return Ok(new { message = $"Comments for ChapterId {chapterId} processed for deletion." });
        }

    }

}
