using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Tag
{
    public class TagCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string tagName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
