using System.ComponentModel.DataAnnotations;

namespace Infotecs.Source.Data.Models
{
    public class Result
    {
        [Key]
        //имя файла
        public string FileName { get; set; } = string.Empty;
        //время запуска первого по времени эксперимента
        public TimeOnly FirstExperimentStartTime { get; set; }
        //время запуска последнего по времени эксперимента
        public TimeOnly LastExperimentStartTime { get; set; }
        //Минимальное время проведения эксперимента
        public int MinDuration { get; set; }
        //Максимальное время проведения эксперимента
        public int MaxDuration { get; set; }
        //Среднее время проведения эксперимента
        public int AverageDuration { get; set; }
        //Минимальное значение показателя
        public float MinIndicatorValue { get; set; }
        //Максимальное значение показателя
        public float MaxIndicatorValue { get; set; }
        //Среднее значение по показателям
        public float AverageIndicatorValue { get; set; }
        //Медиана по показателям
        public float MedianIndicatorValue { get; set; }
        //Количество выполненных экспериментов
        public int ExperimentsCount { get; set; }
    }
}
