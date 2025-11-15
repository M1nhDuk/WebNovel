using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace NovelService.Models
{
    public class ClassicSeries: NovelSeries
    {
   
        public string? iSBN_10 { get; set; }
        public required string iSBN_13 { get; set; }
        public string? publisher { get; set; }
        public DateTime? publish_date { get; set; }
        public string? edition { get; set; }
        public ClassicSeries()
        {
            this.type = type.TRADITIONAL;
        }
    }
}
