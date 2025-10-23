using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class RemoveNotificationsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Cần cung cấp ít nhất một NotificationId.")]
        public List<Guid> NotificationIds { get; set; } = new List<Guid>();
    }
}
