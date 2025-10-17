using AuthService.Models;
using Shareds.DTOs;


namespace AuthService.Services.Interface
{
    public interface IAutheService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task <TokenResponseDto?> LoginAsync( UserDto request);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
