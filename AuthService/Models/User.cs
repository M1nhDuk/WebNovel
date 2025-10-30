namespace AuthService.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public string Role { get; set; } = "User";
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? Created_At { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public bool IsLocked { get; set; }  = false;

        public string? Avatar {  get; set; }
        public string? AvatarThumbnail { get; set; }
        public string? BackgroundImage { get; set; }

    }
}
