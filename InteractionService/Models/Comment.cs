using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteractionService.Models
{
    public class Comment
    {
        [Key]
        public Guid CommentId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string CommentText { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


        //Phân loại
        public int? SeriesId { get; set; } 
        public int? ChapterId { get; set; }

        //replies
        public Guid? ParentCommentId { get; set; }
        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();


        //Lấy từ service khác
        [NotMapped] // Không lưu vào DB, chỉ dùng để hiển thị
        public string? UserName { get; set; }
        [NotMapped]
        public string? UserAvatarThumbnail { get; set; }


    }
}
