using System.ComponentModel.DataAnnotations;

namespace Infotecs.Source.Data.Models
{
    public class Result
    {
        [Key]
        public string FileName { get; set; } = string.Empty;
        public TimeOnly FirstExperimentStartTime { get; set; }
        public TimeOnly LastExperimentStartTime { get; set; }
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
        public int AverageDuration { get; set; }
        public float MinIndicatorValue { get; set; }
        public float MaxIndicatorValue { get; set; }
        public float AverageIndicatorValue { get; set; }
        public float MedianIndicatorValue { get; set; }
        public int ExperimentsCount { get; set; }
    }
}
