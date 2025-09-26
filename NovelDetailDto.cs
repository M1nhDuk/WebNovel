using System;

public class NovelDetailDto
{
	public NovelDetailDto()
	{
        public int NovelId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string StatusName { get; set; } = null!;
        public string Author { get; set; };
        public string? Artist { get; set; };
        public string? CoverImage { get; set; }; 
        public int UploaderId { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<ChapterSummaryDto> Chapters { get; set; } = new();
    }
}
