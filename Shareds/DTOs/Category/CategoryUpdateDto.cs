using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Category
{
    public class CategoryUpdateDto
    {
        [Required(ErrorMessage = "Bắt Buộc")]
        [MaxLength(100, ErrorMessage = "Không được vượt quá 100 ký tự")]
        public string category_name { get; set; } = null!;
    }
}
