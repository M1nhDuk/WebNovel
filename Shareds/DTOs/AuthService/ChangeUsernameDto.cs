using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class ChangeUsernameDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(6, ErrorMessage = "New username must be at least 6 characters")]
        public string NewUsername { get; set; } = string.Empty;
    }
}
