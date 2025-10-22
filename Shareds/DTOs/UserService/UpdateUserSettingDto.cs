using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class UpdateUserSettingDto
    {
        [StringLength(50)]
        public string? FontFamily { get; set; }

        [Range(0, 100)]
        public int? FontSize { get; set; }

        [StringLength(7)]
        public string? BackgroundColor { get; set; }

        [StringLength(20)]
        public string? Alignment { get; set; }

        [Range(0, 200)]
        public int? PaddingPx { get; set; }

        [StringLength(7)]
        public string? FontColor { get; set; }
    }
}
