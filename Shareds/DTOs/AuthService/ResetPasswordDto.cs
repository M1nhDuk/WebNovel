using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password at least 6 character")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Confirm password is not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
