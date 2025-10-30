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

      

    }
}
