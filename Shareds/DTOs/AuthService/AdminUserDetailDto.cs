using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class AdminUserDetailDto
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string PasswordHash { get; set; }
        public string? Avatar { get; set; }
        public string? AvatarThumbnail { get; set; }
        public string? BackgroundImage { get; set; }
    }
}
