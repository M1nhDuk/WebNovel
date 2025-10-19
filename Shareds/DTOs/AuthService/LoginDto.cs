using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username  required")]
        [MinLength(6, ErrorMessage = "Require at least 6 character")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password required")]
        [MinLength(6, ErrorMessage = "Require at least 6 character")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
