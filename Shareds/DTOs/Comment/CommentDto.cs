using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Comment
{
    public class CommentDto
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? SeriesId { get; set; }
        public int? ChapterId { get; set; }
        public Guid? ParentCommentId { get; set; }

        // Thông tin User (lấy từ service khác)
        public string? UserName { get; set; }
        public string? UserAvatarThumbnail { get; set; }

        // Có thể thêm số lượng replies hoặc danh sách replies trực tiếp tùy nhu cầu
        public int ReplyCount { get; set; } = 0;

    }
}
