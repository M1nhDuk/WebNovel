using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationsId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public string Message { get; set; }

        public string? LinkUrl { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
