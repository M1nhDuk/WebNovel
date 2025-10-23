using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty ;
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
