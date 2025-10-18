using AuthService.Data;
using AuthService.Models;
using AuthService.Services.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using Shareds.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services
{
    public class AuthenticationService(AuthDbContext context, IConfiguration configuration, IEmailService emailService) : IAutheService
    {
        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.UserName || u.Email == request.UserName);

            if (string.IsNullOrEmpty(request.UserName) || user is null)
            {
                return null;
            }

            if(string.IsNullOrEmpty(request.Password))
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (!user.IsEmailConfirmed)
            {
                throw new Exception("Unvalid Email, check your email to verify");
            }

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user)
            };
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            var validationErrors = new List<string>();

            //User name
            if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length <= 5)
            {
                validationErrors.Add("User name must be at least 6 character");
            }


            //Email
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                validationErrors.Add("Enter your email");
            }
            else
            {
                try
                {
                    var mailAddress = new MailAddress(request.Email);
                    if (!request.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                    {
                        validationErrors.Add("Enter valid email format");
                    }
                }
                catch (FormatException)
                {
                    validationErrors.Add("Invalid email format");
                }
            }


            //Password
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length <= 5)
            {
                validationErrors.Add("Password must be at least 6 character");
            }
            else if (request.Password != request.ConfirmPassword)
            {
                validationErrors.Add("Password not match");
            }

            if (validationErrors.Any())
            {
                 throw new Exception(string.Join("\n", validationErrors));         
            }

            //Check trong db
            if (await context.Users.AnyAsync(u => u.Username.ToLower() == request.UserName.ToLower()))
            {
                validationErrors.Add("Username already exist");
            }

            if (await context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                validationErrors.Add("Email have been used");
            }

            if (validationErrors.Any())
            {
                throw new Exception(string.Join("\n", validationErrors));
            }

            var user = new User()
            {
                Username = request.UserName,
                Email = request.Email,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };

            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
            user.PasswordHash = hashedPassword;

            context.Users.Add(user); 
            await context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:7154/api/Auth/confirm-email?userId={user.UserId}&token={Uri.EscapeDataString(user.EmailConfirmationToken)}";
            var emailBody = $"<p>Vui lòng nhấp vào liên kết dưới đây để hoàn tất:</p><p><a href='{confirmationLink}'>Kích hoạt tài khoản</a></p>";

            await emailService.SendEmailAsync(user.Email, "Xác thực tài khoản", emailBody);

            return user;
        }


        // Thêm hàm mới ConfirmEmailAsync
        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.EmailConfirmationToken == token);

            if (user == null)
            {
                return false; 
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null; // Vô hiệu hóa token sau khi dùng
            await context.SaveChangesAsync();

            return true;
        }


        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var key = new SymmetricSecurityKey(
                 Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);  
        }


        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if(user is null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }


        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string  refreshToken)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }
    }
}
