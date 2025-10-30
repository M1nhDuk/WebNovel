using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class UpdateRoleDto
    {
        [Required]
        public string NewRole { get; set; }
    }
}
