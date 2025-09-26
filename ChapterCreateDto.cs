using System;

public class ChapterCreateDto
{
	public ChapterCreateDto()
	{
        public int NovelId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
