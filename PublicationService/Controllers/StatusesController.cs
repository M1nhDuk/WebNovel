using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelService.Service.Interfaces;
using Shareds.DTOs.NovelStatus;

namespace NovelService.Controllers
{
    [ApiController]
    [Route("api/statuses")]
    [Authorize(Roles = "Admin")]
    public class StatusesController: ControllerBase
    {
        private readonly IStatusService _statusService;
        private readonly ILogger<StatusesController> _logger;

        public StatusesController(IStatusService statusService, ILogger<StatusesController> logger)
        {
            _statusService = statusService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NovelStatusDto>>> GetStatuses()
        {
            var statuses = await _statusService.GetAllStatusesAsync();
            return Ok(statuses);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<NovelStatusDto>> GetStatus(int id)
        {
            var status = await _statusService.GetStatusByIdAsync(id);
            if (status == null)
            {
                return NotFound(new { message = "Status not found" });
            }
            return Ok(status);
        }


        [HttpPost]
        public async Task<ActionResult<NovelStatusDto>> CreateStatus([FromBody] StatusCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdStatus = await _statusService.CreateStatusAsync(dto);
                return CreatedAtAction(nameof(GetStatus), new { id = createdStatus.statusId }, createdStatus);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create status.");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _statusService.UpdateStatusAsync(id, dto);
                if (!success)
                {
                    return NotFound(new { message = "Status not found." });
                }
                return NoContent(); 
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Failed to update status {StatusId}.", id);
                return Conflict(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            try
            {
                var success = await _statusService.DeleteStatusAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Status not found" });
                }
                return NoContent();
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting status {StatusId}.", id);
                return StatusCode(500, "Erro");
            }
        }
    }
}
