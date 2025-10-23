using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class CreateNotificationDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Message { get; set; }

        public string? LinkUrl { get; set; }
    }
}
