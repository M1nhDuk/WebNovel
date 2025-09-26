using System;

public class CreateNovelDTO
{
	public CreateNovelDTO()
	{
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;
        public int? CategoryId { get; set; }
        public int StatusId { get; set; }
        public string Author { get; set; };
        public string? Artist { get; set; };
        public string? CoverImage { get; set; };
        public int UploaderId { get; set; } // từ User service
        public List<int>? TagIds { get; set; }
    }
}
