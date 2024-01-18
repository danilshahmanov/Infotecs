using System.ComponentModel.DataAnnotations;

namespace Infotecs.Source.Data.Models
{
    public class CSVFile
    {
        [Key]
        //имя файла
        public string FileName { get; set; } = string.Empty;
        //имя автора файла
        public string AuthorName { get; set; } = string.Empty;
        //дата и время загрузки файла
        public DateTime DateTimeOfUpload { get; set; }
        //данные файла
        public byte[]? Data { get; set; }
    }
}
