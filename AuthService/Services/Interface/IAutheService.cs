using AuthService.Models;
using Shareds.DTOs;
using Shareds.DTOs.AuthService;


namespace AuthService.Services.Interface
{
    public interface IAutheService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task <TokenResponseDto?> LoginAsync( LoginDto request);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> ConfirmEmailAsync(Guid userId, string token);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}

