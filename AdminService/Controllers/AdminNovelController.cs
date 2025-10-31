using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs.Category;
using Shareds.DTOs.NovelStatus;
using Shareds.DTOs.Tag;
using System.Net.Http.Headers;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/publication")]
    [Authorize(Roles = "Admin")]
    public class AdminNovelController: ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminNovelController> _logger;

        public AdminNovelController(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AdminNovelController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        private HttpClient GetNovelClient()
        {
            var client = _httpClientFactory.CreateClient("NovelServiceClient");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        //CATEGORY

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var client = GetNovelClient();

            var response = await client.GetAsync("api/categories");

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>());

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            var client = GetNovelClient();

            var response = await client.PostAsJsonAsync("api/categories", dto);

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadFromJsonAsync<CategoryDto>());

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            var client = GetNovelClient();

            var response = await client.PutAsJsonAsync($"api/categories/{id}", dto);

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = GetNovelClient();

            var response = await client.DeleteAsync($"api/categories/{id}");

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        //TAG

        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            var client = GetNovelClient();

            var response = await client.GetAsync("api/tags");

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadFromJsonAsync<IEnumerable<TagDto>>());

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpPost("tags")]
        public async Task<IActionResult> CreateTag([FromBody] TagCreateDto dto)
        {
            var client = GetNovelClient();

            var response = await client.PostAsJsonAsync("api/tags", dto);

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadFromJsonAsync<TagDto>());

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpPut("tags/{id:int}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] TagUpdateDto dto)
        {
            var client = GetNovelClient();

            var response = await client.PutAsJsonAsync($"api/tags/{id}", dto);

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpDelete("tags/{id:int}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var client = GetNovelClient();

            var response = await client.DeleteAsync($"api/tags/{id}");

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }



        //STATUS
        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var client = GetNovelClient();
            var response = await client.GetAsync("api/statuses");

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadFromJsonAsync<IEnumerable<NovelStatusDto>>());

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpPost("statuses")]
        public async Task<IActionResult> CreateStatus([FromBody] StatusCreateDto dto)
        {
            var client = GetNovelClient();

            var response = await client.PostAsJsonAsync("api/statuses", dto);

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadFromJsonAsync<NovelStatusDto>());

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpPut("statuses/{id:int}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var client = GetNovelClient();

            var response = await client.PutAsJsonAsync($"api/statuses/{id}", dto);
            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [HttpDelete("statuses/{id:int}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var client = GetNovelClient();

            var response = await client.DeleteAsync($"api/statuses/{id}");
            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }



        //SERIES -- NOVEL -- CHAPTER

        [HttpDelete("series/{id:int}")]
        public async Task<IActionResult> DeleteSeries(int id)
        {
            var client = GetNovelClient();

            var response = await client.DeleteAsync($"api/internal/publication/admin/series/{id}");

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }



        [HttpDelete("novels/{id:int}")]
        public async Task<IActionResult> DeleteNovel(int id)
        {

            var client = GetNovelClient();
            var response = await client.DeleteAsync($"api/internal/publication/admin/novels/{id}");

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }



        [HttpDelete("chapters/{id:int}")]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var client = GetNovelClient();

            var response = await client.DeleteAsync($"api/internal/publication/admin/chapters/{id}");

            if (response.IsSuccessStatusCode)
                return NoContent();

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
