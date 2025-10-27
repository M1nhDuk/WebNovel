using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class UserInfoDto
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? AvatarThumbnail { get; set; }
    }
}
