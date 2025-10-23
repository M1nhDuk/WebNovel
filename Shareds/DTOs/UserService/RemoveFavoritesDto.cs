using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class RemoveFavoritesDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Cần cung cấp ít nhất một SeriesId.")]
        public List<int> SeriesIds { get; set; } = new List<int>();
    }
}
