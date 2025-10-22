using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserSetting
    {
        [Key]
        public Guid UserId {  get; set; }

        [Required]
        [StringLength(50)]
        public string FontFamily { get; set; } = "Times New Roman";

        [Required]
        public int FontSize { get; set; } = 18;

        [Required]
        [StringLength(7)]
        public string BackgroundColor { get; set; } = "#FFFFFF";

        [Required]
        [StringLength(7)]
        public string FontColor { get; set; } = "#000000";

        [Required]
        [StringLength(20)] 
        public string Alignment { get; set; } = "left";

        [Required]
        public int PaddingPx { get; set; } = 0;
    }
}
