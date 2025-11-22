using System;

public class ChapterSummaryDto
{
	public ChapterSummaryDto()
	{
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = null!;
     public DateTime CreatedOn { get; set; }
    }
}
