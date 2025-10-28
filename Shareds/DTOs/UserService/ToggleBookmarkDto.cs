using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class ToggleBookmarkDto
    {
        [Required]
        public int SeriesId { get; set; } 

        [Required]
        public int ChapterId { get; set; }

        [Required]
        [MaxLength(255)]
        public string LocationIdentifier { get; set; } = string.Empty; // Vị trí mới

        [MaxLength(200)]
        public string? ContextSnippet { get; set; }
    }
}
