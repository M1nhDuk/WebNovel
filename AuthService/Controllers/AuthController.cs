using AuthService.Models;
using AuthService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAutheService autheService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await autheService.RegisterAsync(request);
            if(request is null)
            {
                return BadRequest("User name already exist");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>>Login(UserDto request)
        {
            var token = await autheService.LoginAsync(request);
            if (token is null)
                return BadRequest("Invalid username or password");
            return Ok(token);
        }

        //endpoint
        [Authorize]
        [HttpGet]
        public IActionResult AuthenticantedOnlyEndPoint()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndPoint()
        {
            return Ok("You are admin");
        }
    }
}
