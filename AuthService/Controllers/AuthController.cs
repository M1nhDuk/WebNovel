using AuthService.Models;
using AuthService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shareds.DTOs;
using Shareds.DTOs.AuthService;
using System.Data;
using System.Security.Claims;


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
            if(user is null)
            {
                return BadRequest("Username or email is already in use.");
            }
            return Ok(user);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            var success = await autheService.ConfirmEmailAsync(userId, token);
            if (success)
            {
                return Ok("Account verification successful!");
            }
            return BadRequest("The authentication link is invalid or has expired.");
        }



        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>>Login(LoginDto request)
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
            if (request is null || result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid Refresh Token");

            return Ok(result);

        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndPoint()
        {
            return Ok("You are admin");
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {

            await autheService.ForgotPasswordAsync(dto.Email);

            return Ok("If your email exists, a new password has been sent.");
        }



        //Logout
        [Authorize] 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(); 
            }

            await autheService.LogoutAsync(userId);
            return Ok("Logged out successfully.");
        }
    }
}
