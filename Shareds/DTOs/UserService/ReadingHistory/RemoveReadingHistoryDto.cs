using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService.ReadingHistory
{
    public class RemoveReadingHistoryDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Need HistoryId or SeriesId.")]
        public List<Guid>? HistoryIds { get; set; }
    }
}
