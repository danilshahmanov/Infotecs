using CsvHelper;
using Infotecs.Source.Data.DTOs;
using Infotecs.Source.Data.Models;

namespace Infotecs.Source.Services
{
    public interface IExperimentService
    {
        Task<bool> FileExistsInDB(string fileName);
        Task ProcessFileAsync(IFormFile file, string authorName);
        Task<List<ValueDto>> GetValuesBatchAsync(string fileName, int batchNumber);
        Task<List<Result>> GetResultsByQueryParams(
            string? fileName,
            double? minAverageIndicatorValue,
            double? maxAverageIndicatorValue,
            int? minAverageDuration,
            int? maxAverageDuration
        );
        bool ShouldSkipValue(IReaderRow readerRow);
    }

}
