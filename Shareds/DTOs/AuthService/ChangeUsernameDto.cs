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
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 30 characters")]
        public string NewUsername { get; set; } = string.Empty;
    }
}
