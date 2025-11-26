using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/interaction")]
    [Authorize(Roles = "Admin")]
    public class AdminInteractionController: ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminInteractionController> _logger;

        public AdminInteractionController(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AdminInteractionController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private HttpClient GetInteractionClient()
        {
            var client = _httpClientFactory.CreateClient("InteractionServiceClient");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
