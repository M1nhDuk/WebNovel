using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class SeriesGeneralNotificationDto
    {
        public int SeriesId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "SeriesUpdate";

        public string? LinkUrl { get; set; }
    }
}
