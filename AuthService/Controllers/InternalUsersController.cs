using AuthService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.AuthService; // Thêm using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

        [ApiController]
        [Route("api/internal/users")] 
        public class InternalUsersController : ControllerBase
        {
            private readonly AuthDbContext _context;
            private readonly ILogger<InternalUsersController> _logger;

            public InternalUsersController(AuthDbContext context, ILogger<InternalUsersController> logger)
            {
                _context = context;
                _logger = logger;
            }

            [HttpGet("batch")] // Endpoint: /api/internal/users/batch?ids=guid1,guid2,...
            public async Task<ActionResult<List<UserInfoDto>>> GetUsersBatch([FromQuery] string ids)
            {
                if (string.IsNullOrEmpty(ids))
                {
                    return BadRequest("User IDs are required.");
                }

                var userGuids = new List<Guid>();
                var stringIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var idStr in stringIds)
                {
                    if (Guid.TryParse(idStr, out Guid guid))
                    {
                        userGuids.Add(guid);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid GUID format received in batch request: {InvalidId}", idStr);
                    }
                }

                if (!userGuids.Any())
                {
                    return Ok(new List<UserInfoDto>()); 
                }

                try
                {
                    // Truy vấn DB
                    var usersInfo = await _context.Users
                        .Where(u => userGuids.Contains(u.UserId))
                        .Select(u => new UserInfoDto
                        {
                            UserId = u.UserId,
                            UserName = u.Username, 
                            AvatarThumbnail = u.AvatarThumbnail 
                        })
                        .ToListAsync();

                    return Ok(usersInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching batch user info for IDs: {UserIds}", string.Join(",", userGuids));
                    return StatusCode(500, "Internal server error while fetching user data.");
                }
            }
}