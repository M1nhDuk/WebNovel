using AuthService.Data;
using AuthService.Models;
using AuthService.Services.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using Shareds.DTOs;
using Shareds.DTOs.AuthService;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services
{
    public class AuthenticationService(AuthDbContext context, IConfiguration configuration, IEmailService emailService) : IAutheService
    {
        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var sb = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    sb.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return sb.ToString();
        }


        //Create Token
        private async Task<TokenResponseDto> CreateTokenResponse(User? user, bool rememberMe = false)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user, rememberMe)
            };
        }



        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);

            if (string.IsNullOrEmpty(request.Username) || user is null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (user.IsLocked)
            {
                throw new Exception("This account is Locked.");
            }

            if (!user.IsEmailConfirmed)
            {
                throw new Exception("Unvalid Email, check your email to verify");
            }

            return await CreateTokenResponse(user, request.RememberMe);
        }


        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var passwordVerificationResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, dto.OldPassword);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return false; 
            }

            var newHashedPassword = new PasswordHasher<User>().HashPassword(user, dto.NewPassword);
            user.PasswordHash = newHashedPassword;

            await context.SaveChangesAsync();
            return true;
        }


        public async Task<TokenResponseDto?> ChangeUsernameAsync(Guid userId, ChangeUsernameDto dto)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }


            var newUsernameLower = dto.NewUsername.ToLower();
            if (await context.Users.AnyAsync(u => u.Username.ToLower() == newUsernameLower && u.UserId != userId))
            {
                throw new Exception("Username already taken."); 
            }


            user.Username = dto.NewUsername;
            await context.SaveChangesAsync();

            return await CreateTokenResponse(user, false);
        }

        //Register
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
                EmailConfirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Created_At = DateTime.UtcNow,
                Avatar = "/uploads/default_avatar.png",
                AvatarThumbnail = "/uploads/default_avatar_thumb.png",
                BackgroundImage = "/uploads/default_background.jpg"
            };

            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
            user.PasswordHash = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:7154/api/Auth/confirm-email?userId={user.UserId}&token={Uri.EscapeDataString(user.EmailConfirmationToken)}";

            var emailBody = $"<p>CLick on the link:</p>" +
                            $"<p><a href='{confirmationLink}'>to confirm your account</a></p>";

            await emailService.SendEmailAsync(user.Email, "Xác thực tài khoản", emailBody);

            return user;
        }



        public async Task ForgotPasswordAsync(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {

                string newPassword = GenerateRandomPassword();


                var hashedPassword = new PasswordHasher<User>().HashPassword(user, newPassword);
                user.PasswordHash = hashedPassword;


                user.PasswordResetToken = null;
                user.ResetTokenExpires = null;

                await context.SaveChangesAsync();

                string emailBody = $@"
                <p>Your new password:</p>
                <p><strong>{newPassword}</strong></p>
                <br>
                <p>Login your account with this password.</p>";

                await emailService.SendEmailAsync(email, "Your new password", emailBody);
            }
        }



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

            if (user is null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }


        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
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

        private async Task<string> GenerateAndSaveRefreshToken(User user, bool rememberMe)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;

            if (rememberMe)
            {
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30); // Thời hạn dài
            }

            else
            {
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(2); // Thời hạn ngắn
            }
            await context.SaveChangesAsync();
            return refreshToken;
        }

        


        //Log out
        public async Task LogoutAsync(Guid userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await context.SaveChangesAsync();
        }
    }
}
