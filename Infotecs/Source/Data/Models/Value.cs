using System.ComponentModel.DataAnnotations;

namespace Infotecs.Source.Data.Models
{
    public class Value
    {
        [Key]
        public int Id { get; set; }
        //имя файла
        public string FileName { get; set; } = string.Empty;

        //дата и время
        public DateTime StartDateTime { get; set; }

        //продолжительность
        public int Duration { get; set; }

        //показатель
        public float IndicatorValue { get; set; }
    }
}
