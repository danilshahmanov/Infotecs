using System.ComponentModel.DataAnnotations;

namespace Infotecs.Source.Data.Models
{
    public class Value
    {
        [Key]
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        //дата и время
        public DateTime StartDateTime { get; set; }

        //время
        public int Duration { get; set; }

        //показатель
        public float IndicatorValue { get; set; }
    }
}
