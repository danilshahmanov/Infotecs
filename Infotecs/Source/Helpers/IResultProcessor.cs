using Infotecs.Source.Data.Models;

namespace Infotecs.Source.Helpers
{
    /// <summary>
    /// Интерфейс для обработчика вычислений для сущности Result.
    /// </summary>
    public interface IResultProcessor
    {
        Task<Result> GenerateResultAsync(string fileName);
        void UpdateResultInfoByBuffer(List<Value> buffer);
    }
}
