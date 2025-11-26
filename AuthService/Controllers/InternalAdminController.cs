using AuthService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shareds.DTOs.AuthService;
using Shareds.DTOs.NovelSeries;
using System.IO;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/internal/admin/users")]
    [Authorize(Roles = "Admin")]
    public class InternalAdminController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public InternalAdminController(AuthDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }



        // GET: api/internal/admin/users
        [HttpGet]
        public async Task<ActionResult<PagedResult<AdminUserDetailDto>>> GetUsers(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] string? role = null,
                [FromQuery] bool? isVerified = null)
        {
            var query = _context.Users.AsQueryable();


            //Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(u => u.IsEmailConfirmed == isVerified.Value);
            }


            var totalRecords = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = users.Select(u => new AdminUserDetailDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                PasswordHash = u.PasswordHash,
                IsEmailConfirmed = u.IsEmailConfirmed,
                IsLocked = u.IsLocked,
                CreatedAt = u.Created_At,
                AvatarThumbnail = u.AvatarThumbnail
            }).ToList();

            return Ok(new PagedResult<AdminUserDetailDto>
            {
                Items = userDtos,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
        }


        // GET: api/internal/admin/users/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AdminUserDetailDto>> GetUserDetail(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound();

            return Ok(new AdminUserDetailDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                IsEmailConfirmed = user.IsEmailConfirmed,
                IsLocked = user.IsLocked,
                CreatedAt = user.Created_At,
                AvatarThumbnail = user.AvatarThumbnail,
                Avatar = user.Avatar,
                BackgroundImage = user.BackgroundImage
            });
        }

        // PUT: api/internal/admin/users/{id}/role
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(dto.NewRole) || (dto.NewRole != "Admin" && dto.NewRole != "User"))
            {
                return BadRequest("Role Invalid");
            }

            user.Role = dto.NewRole;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost("{id:guid}/lock")]
        public async Task<IActionResult> LockUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsLocked = true;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/internal/admin/users/{id}/unlock
        [HttpPost("{id:guid}/unlock")]
        public async Task<IActionResult> UnlockUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsLocked = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE: api/internal/admin/users/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            //Default images link
            var defaultImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/uploads/default_avatar.png",
                "/uploads/default_avatar_thumb.png",
                "/uploads/default_background.jpg"
            };

            //Gom các đường dẫn ảnh của user cần xóa
            var imagesToDelete = new List<string?>
            {
                user.Avatar,
                user.AvatarThumbnail,
                user.BackgroundImage
            };

            //Duyệt và xóa file vật lý
            foreach (var relativePath in imagesToDelete)
            {
                // Chỉ xóa nếu đường dẫn tồn tại và KHÔNG phải là ảnh mặc định
                if (!string.IsNullOrEmpty(relativePath) && !defaultImages.Contains(relativePath))
                {
                    try
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
