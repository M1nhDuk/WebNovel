using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserFavorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int seriesId { get; set; }

        [Required]
        public DateTime TimeAdded { get; set; }
        public int LastKnownChapterCount { get; set; } = 0;

        public int UnreadCount { get; set; } = 0;
    }
}
