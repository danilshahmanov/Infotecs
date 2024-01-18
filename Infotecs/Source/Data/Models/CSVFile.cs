using System.ComponentModel.DataAnnotations;

namespace Infotecs.Source.Data.Models
{
    public class CSVFile
    {
        [Key]
        public string FileName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime DateTimeOfUpload { get; set; }
        public byte[]? Data { get; set; }
    }
}
