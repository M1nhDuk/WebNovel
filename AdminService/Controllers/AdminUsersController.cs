using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.AuthService;
using Shareds.DTOs.NovelSeries;
using System.Net.Http.Headers;
using System.Text.Json;


namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<AdminUsersController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        private HttpClient GetAuthClient()
        {
            var client = _httpClientFactory.CreateClient("AuthServiceClient");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isVerified = null)
        {
            var client = GetAuthClient();
            var url = $"api/internal/admin/users?page={page}&pageSize={pageSize}&search={search}&role={role}&isVerified={isVerified}";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<AdminUserDetailDto>>();
                return Ok(pagedResult);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        // GET: api/admin/users/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserDetail(Guid id)
        {
            var client = GetAuthClient();
            var response = await client.GetAsync($"api/internal/admin/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var userDetail = await response.Content.ReadFromJsonAsync<AdminUserDetailDto>();
                return Ok(userDetail);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        // PUT: api/admin/users/{id}/role
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            var client = GetAuthClient();
            var response = await client.PutAsJsonAsync($"api/internal/admin/users/{id}/role", dto);

            if (response.IsSuccessStatusCode)
                return NoContent();
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        // POST: api/admin/users/{id}/lock 
        [HttpPost("{id:guid}/lock")]
        public async Task<IActionResult> LockUser(Guid id)
        {
            var client = GetAuthClient();
            var response = await client.PostAsync($"api/internal/admin/users/{id}/lock", null);

            if (response.IsSuccessStatusCode)
                return NoContent();
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        // POST: api/admin/users/{id}/unlock 
        [HttpPost("{id:guid}/unlock")]
        public async Task<IActionResult> UnlockUser(Guid id)
        {
            var client = GetAuthClient();
            var response = await client.PostAsync($"api/internal/admin/users/{id}/unlock", null);

            if (response.IsSuccessStatusCode)
                return NoContent();
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        // DELETE: api/admin/users/{id} 
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var client = GetAuthClient();
            var response = await client.DeleteAsync($"api/internal/admin/users/{id}");

            if (response.IsSuccessStatusCode)
                return NoContent();
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

    }
}
