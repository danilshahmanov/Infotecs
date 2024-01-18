using Infotecs.Source.Data.Models;

namespace Infotecs.Source.Helpers
{
    /// <summary>
    /// Класс используется для получения сущности Result для определенного файла на основе значений сущностей Value, полученных из буфера.
    /// Значения сущностей Value из буфера обрабатываются и обновляют статистическую информацию использующуюся для вычисления необходимых значений сущности Result.
    /// </summary>
    public class ResultProcessor
    {
        //Делегат для подсчета медианы по показателям
        public delegate Task<float> CalculateMedianDelegate(string fileName);
        private readonly CalculateMedianDelegate _calculateMedianDelegate;
        private long _durationSum;
        private int _minDuration = int.MaxValue;
        private int _maxDuration = int.MinValue;
        private double _indicatorValueSum;
        private float _minIndicatorValue = float.MaxValue;
        private float _maxIndicatorValue = float.MinValue;
        private DateTime _lastExperimentStart = DateTime.MinValue;
        private DateTime _firstExperimentStart = DateTime.MaxValue;
        private int _experimentsCount;

        /// <summary>
        /// Создает новый объект ResultProcessor с определенным делегатом для подсчета медианы по показателям.
        /// </summary>
        /// <param name="calculateMedian">Делегат для подсчета медианы по показателям.</param>
        public ResultProcessor(CalculateMedianDelegate calculateMedian) =>
            _calculateMedianDelegate = calculateMedian;

        /// <summary>
        /// Создает сущность Result на основе статистических характеристик.
        /// </summary>
        /// <param name="fileName">Имя файла для которого нужно получить Result.</param>
        /// <returns><c>Result</c> вычисленный на основе статистических характеристик.</returns>
        public async Task<Result> GenerateResultAsync(string fileName)
        {
            var medianIndicatorValue = await _calculateMedianDelegate(fileName);
            var result = new Result()
            {
                FileName = fileName,
                FirstExperimentStartTime = TimeOnly.FromDateTime(_firstExperimentStart),
                LastExperimentStartTime = TimeOnly.FromDateTime(_lastExperimentStart),
                MaxDuration = _maxDuration,
                MinDuration = _minDuration,
                MaxIndicatorValue = _maxIndicatorValue,
                MinIndicatorValue = _minIndicatorValue,
                AverageIndicatorValue = (float)(_indicatorValueSum / _experimentsCount),
                AverageDuration = (int)(_durationSum / _experimentsCount),
                MedianIndicatorValue = medianIndicatorValue,
                ExperimentsCount = _experimentsCount
            };
            Reset();
            return result;
        }

        /// <summary>
        /// Обновляет статистическую информацию на основе значений сущностей Value в буфере для вычисления значений сущности Result.
        /// </summary>
        /// <param name="buffer"<c>List<Value></c> - буфер с сущностями Value.</param>
        public void UpdateResultInfoByBuffer(List<Value> buffer)
        {
            _experimentsCount += buffer.Count;
            foreach (var value in buffer)
            {
                _minIndicatorValue = Math.Min(_minIndicatorValue, value.IndicatorValue);
                _maxIndicatorValue = Math.Max(_maxIndicatorValue, value.IndicatorValue);
                _minDuration = Math.Min(_minDuration, value.Duration);
                _maxDuration = Math.Max(_maxDuration, value.Duration);
                _durationSum += value.Duration;
                _indicatorValueSum += value.IndicatorValue;
                _firstExperimentStart =
                    value.StartDateTime < _firstExperimentStart
                        ? value.StartDateTime
                        : _firstExperimentStart;
                _lastExperimentStart =
                    value.StartDateTime > _lastExperimentStart
                        ? value.StartDateTime
                        : _lastExperimentStart;
            }
        }

        /// <summary>
        /// Устанавливает всей статистической информации для создания сущности Result значения по умолчанию.
        /// </summary>
        private void Reset()
        {
            _durationSum = 0;
            _minDuration = int.MaxValue;
            _maxDuration = int.MinValue;
            _indicatorValueSum = 0;
            _minIndicatorValue = float.MaxValue;
            _maxIndicatorValue = float.MinValue;
            _lastExperimentStart = DateTime.MinValue;
            _firstExperimentStart = DateTime.MaxValue;
            _experimentsCount = 0;
        }
    }
}
