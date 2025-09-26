using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.Chapter
{
    public class ReorderChaptersRequest
    {
        public int novel_Id { get; set; }
        public List<ChapterRecorder> Chapters { get; set; } = new();
    }
}
