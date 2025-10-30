using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.NovelStatus
{
    public class StatusCreateDto
    {
        [Required(ErrorMessage = "Required")]
        [MaxLength(50, ErrorMessage = "Limit 50 char")]
        public string statusName { get; set; } = null!;
    }
}
