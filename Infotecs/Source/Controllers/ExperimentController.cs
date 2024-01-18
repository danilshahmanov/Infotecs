using System.Text.Json;
using Infotecs.Source.Data;
using Infotecs.Source.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Infotecs.Source.Controllers
{
    [Route("science/")]
    [ApiController]
    public class ExperimentController : ControllerBase
    {
        private readonly IExperimentService _experimentService;

        public ExperimentController(IExperimentService experimentService) => _experimentService = experimentService; 

        /// <summary>
        /// Отправляет загруженный файл на обработку.
        /// </summary>
        /// <param name="file">Файл для обработки.</param>
        /// <param name="authorName">Имя автора файла.</param>
        /// <returns>IActionResult сообщающий об успехе или неудачи обработки файла.</returns>
        [HttpPost("files")]
        public async Task<IActionResult> ProcessFile(IFormFile? file, [FromQuery] string authorName)
        {
            if (file is null)
                return BadRequest("Файл не загружен");
            try
            {
                await _experimentService.ProcessFileAsync(file, authorName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //быстро работает сам по себе, с postman тоже хорошая скорость, но со swagger отображение большого json занимает много времени

        /// <summary>
        /// Получает все сущности Value для файла с указанным именем из базы данных.
        /// </summary>
        /// <param name="fileName">Имя файла для получения сущностей Value.</param>
        /// <remarks>
        /// Метод напрямую записывает данные сущностей Value в тело ответа для предотвращения загрузки в память больших массивово данных.
        /// Если файла с таким именем не существует то вернет 404 status code.
        /// </remarks>
        [HttpGet("values/{fileName}")]
        public async Task GetAllValuesByFileName([FromRoute] string fileName)
        {
            if (!await _experimentService.FileExistsInDB(fileName))
            {
                Response.StatusCode = 404;
                await Response.WriteAsync($"file with name '{fileName}' is not found.");
                return;
            }

            Response.ContentType = "application/json";
            await Response.StartAsync();

            await using var stream = Response.BodyWriter.AsStream();
            await using var writer = new Utf8JsonWriter(
                stream,
                new JsonWriterOptions { Indented = true }
            );

            writer.WriteStartArray();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            int batchNumber = 0;
            while (true)
            {
                var batch = await _experimentService.GetValuesBatchAsync(fileName, batchNumber);
                if (batch.Count == 0)
                    break;

                foreach (var value in batch)
                {
                    JsonSerializer.Serialize(writer, value, jsonOptions);
                }
                await writer.FlushAsync();
                batchNumber++;
            }

            writer.WriteEndArray();
            await writer.FlushAsync();
        }

        /// <summary>
        /// Получает сущности Result из базы данных на основе переданных параметров.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="minAverageIndicatorValue">Минимальное значение среднего показателя.</param>
        /// <param name="maxAverageIndicatorValue">Максимальное значение среднего показателя.</param>
        /// <param name="minAverageDuration">Минимальное значение средней длительности.</param>
        /// <param name="maxAverageDuration">Максимальное значение средней длительности.</param>
        /// <returns>IActionResult содержащий отфильтрованные сущности Result или сообщение об ошибки.</returns>
        /// <remarks>
        /// Все параметры фильтрации имеют тип nullable, но определенные комбинации должны содержать значения вместе.
        /// Обоим значениям для фильтрации по диапазону должны быть присвоены значения.
        /// Так же хотя бы один параметр должен содержать значение (или одна комбинация для определенных фильтраций).
        /// </remarks>
        [HttpGet("results")]
        public async Task<IActionResult> GetResultsByQueryParams(
            [FromQuery] string? fileName,
            [FromQuery] double? minAverageIndicatorValue,
            [FromQuery] double? maxAverageIndicatorValue,
            [FromQuery] int? minAverageDuration,
            [FromQuery] int? maxAverageDuration
        )
        {
            if (minAverageIndicatorValue.HasValue ^ maxAverageIndicatorValue.HasValue)
                return BadRequest(
                    "both boundaries for range of average indicator value must be provided."
                );
            if (minAverageDuration.HasValue ^ maxAverageDuration.HasValue)
                return BadRequest(
                    "both boundaries for range of average duration must be provided."
                );
            if (
                fileName.IsNullOrEmpty()
                && !minAverageDuration.HasValue
                && !minAverageIndicatorValue.HasValue
            )
                return BadRequest("at least one query parameter must be provided.");
            var results = await _experimentService.GetResultsByQueryParams(
                fileName,
                minAverageIndicatorValue,
                maxAverageIndicatorValue,
                minAverageDuration,
                maxAverageDuration
            );
            if (results.Count == 0)
                return NotFound("no results found matching the provided parameters.");
            return Ok(results);
        }
    }
}
