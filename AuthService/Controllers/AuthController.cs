using AuthService.Models;
using AuthService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs;
using System.Data;

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
        public async Task<ActionResult<TokenResponseDto>>Login(UserDto request)
        {
            var result = await autheService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid username or password");
            return Ok(result);
        }


        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await autheService.RefreshTokenAsync(request);
            if (request is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid Refresh Token");

            return Ok(result);

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
