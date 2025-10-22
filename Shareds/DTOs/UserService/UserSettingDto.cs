using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.UserService
{
    public class UserSettingDto
    {
        public Guid UserId { get; set; }
        public string FontFamily { get; set; } = string.Empty;
        public int FontSize { get; set; }
        public string BackgroundColor { get; set; } = string.Empty;
        public string FontColor {  get; set; } = string.Empty;
        public string Aligment {  get; set; } = string.Empty;
        public int? PaddingPx { get; set; }


    }
}
