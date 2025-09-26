using System;

public class ChapterDetailDto
{
	public ChapterDetailDto()
	{

        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = null!;  
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
}
}
