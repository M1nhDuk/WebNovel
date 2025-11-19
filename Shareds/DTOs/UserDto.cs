using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs
{
    public class UserDto
    {
        [Required(ErrorMessage = "Username  required")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 30 characters")]
        public string UserName {  get; set; } = string.Empty;

        [Required(ErrorMessage = "Password required")]
        [MinLength(6, ErrorMessage = "Require at least 6 character")]
        public string Password{ get; set; } = string.Empty ;

        [Required(ErrorMessage = "Email required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password verifycation required")]
        [Compare("Password", ErrorMessage = "Your verification password is not match with your password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
