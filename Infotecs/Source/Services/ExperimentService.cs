using CsvHelper;
using EFCore.BulkExtensions;
using Infotecs.Source.Data;
using Infotecs.Source.Data.DTOs;
using Infotecs.Source.Data.Models;
using Infotecs.Source.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace Infotecs.Source.Services
{
    public class ExperimentService : IExperimentService
    {
        private readonly ExperimentContext _context;
        private readonly CSVFileReader _csvFileReader;
        private readonly ResultProcessor _resultProcessor;

        public ExperimentService(ExperimentContext context)
        {
            _context = context;
            _csvFileReader = new CSVFileReader(ShouldSkipValue, BufferFilledHandlerAsync);
            _resultProcessor = new ResultProcessor(CalculateMedianOfIndicatorValue);
        }

        /// <summary>
        /// Удаляет все данные связанные с файлом из базы данных.
        /// </summary>
        /// <param name="fileName">Имя файла данные которого должны быть удалены.</param>
        /// <remarks>
        /// Этот метод выполняет транзакцию для удаления всех связанных данных с файлом для всех таблиц в базе данных.
        /// </remarks>
        private async Task DeleteFileData(string fileName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var deleteValues = _context
                .Values.Where(value => value.FileName == fileName)
                .ExecuteDeleteAsync();
            var deleteFile = _context
                .Files.Where(file => file.FileName == fileName)
                .ExecuteDeleteAsync();
            var deleteResult = _context
                .Results.Where(result => result.FileName == fileName)
                .ExecuteDeleteAsync();
            await Task.WhenAll(deleteValues, deleteFile, deleteResult);
            await transaction.CommitAsync();
        }

        /// <summary>
        /// Проверяет существует ли уже в базе данных файл с таким именем.
        /// </summary>
        /// <param name="fileName">Имя файла для проверки.</param>
        /// <returns>Возвращает <c>true</c> если файл существует и <c>false</c> если не существует.</returns>
        /// <remarks>
        /// Проверяет существование только в таблице Results, потому что если данные этого файла есть в этой таблице, то есть и во всех остальных.
        /// </remarks>
        public async Task<bool> FileExistsInDB(string fileName) =>
            await _context.Results.AnyAsync(result => result.FileName == fileName);

        /// <summary>
        /// Вычисляет медиану для показателя для определенного файла.
        /// </summary>
        /// <param name="fileName">Имя файла для сущностей Value которого должна быть вычислена медиана показателей.</param>
        /// <returns>Значение медианы типа <c>float</c>.</returns>
        /// <remarks>
        /// Метод находит средние элементы сортированных записей по показателю и вычисляет медиану.
        /// Если число записей четное, то возвращается среднее значение показателей 2х средних записей.
        /// Если число записей нечетное, то возвращается значение показателя средней записи.
        /// </remarks>
        private async Task<float> CalculateMedianOfIndicatorValue(string fileName)
        {
            var query = _context.Values.Where(value => value.FileName == fileName);
            int valuesCount = await query.CountAsync();

            //считаем опорные средние точки
            int rightMidPoint = valuesCount / 2;
            int leftMidPoint = (valuesCount - 1) / 2;

            var medians = await query
                .OrderBy(value => value.IndicatorValue)
                .Skip(leftMidPoint) //пропускаем значения до левой опорной точки
                .Take(valuesCount % 2 == 0 ? 2 : 1) //берем необходимое количество точек в зависимости от четности или нечетности количества
                .Select(value => value.IndicatorValue)
                .ToListAsync();
            return medians.Count == 1 ? medians[0] : (medians[0] + medians[1]) / 2;
        }

        /// <summary>
        /// Получает результаты из базы данных на основе параметров фильтрации.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="minAverageIndicatorValue">Минимальное значение среднего показателя.</param>
        /// <param name="maxAverageIndicatorValue">Максимальное значение среднего показателя.</param>
        /// <param name="minAverageDuration">Минимальное значение средней длительности.</param>
        /// <param name="maxAverageDuration">Максимальное значение средней длительности.</param>
        /// <returns>List сущностей Result подходящих условиям фильтрации или пустой List.</returns>
        /// <remarks>
        /// Все параметры фильтрации имеют тип nullable, но определенные комбинации должны содержать значения вместе.
        /// Обоим значениям для фильтрации по диапазону должны быть присвоены значения.
        /// Так же хотя бы один параметр должен содержать значение (или одна комбинация для определенных фильтраций).
        /// </remarks>
        public async Task<List<Result>> GetResultsByQueryParams(
            string? fileName,
            double? minAverageIndicatorValue,
            double? maxAverageIndicatorValue,
            int? minAverageDuration,
            int? maxAverageDuration
        )
        {
            var query = _context.Results.AsQueryable();
            if (!fileName.IsNullOrEmpty())
                query = query.Where(result => result.FileName == fileName);
            if (minAverageIndicatorValue.HasValue)
                query = query.Where(result =>
                    result.AverageIndicatorValue >= minAverageIndicatorValue
                    && result.AverageIndicatorValue <= maxAverageIndicatorValue
                );
            if (minAverageDuration.HasValue)
                query = query.Where(result =>
                    result.AverageDuration >= minAverageDuration
                    && result.AverageDuration <= maxAverageDuration
                );
            return await query.ToListAsync();
        }

        /// <summary>
        /// Получает пакет сущностей Value для определенного файла определенного размера.
        /// Сортирует записи в таблице Values и получает записи для мапинга с (batchNumber*batchSize - batchSize) до (batchNumber*batchSize).
        /// Размер пакета по умолчанию = 1000.
        /// </summary>
        /// <param name="fileName">Имя файла для получения сущностей Value.</param>
        /// <param name="batchNumber">Порядковый номер пакета.</param>
        /// <returns><c>List<ValueDto></c> содержащий в себе batchSize или существующее число в базе данных элементов с учетом порядкового номера пакета.</returns>
        /// <remarks>
        /// Метод используется для эффективного получения сущностей пакетами без необходимости их полной загрузки в память.
        /// </remarks>
        public async Task<List<ValueDto>> GetValuesBatchAsync(string fileName, int batchNumber)
        {
            int batchSize = 1000;
            return await _context
                .Values.Where(value => value.FileName == fileName)
                .AsNoTracking()
                .OrderBy(value => value.StartDateTime)
                .Skip(batchNumber * batchSize)
                .Take(batchSize)
                .Select(value => new ValueDto
                {
                    StartDateTime = value.StartDateTime,
                    Duration = value.Duration,
                    IndicatorValue = value.IndicatorValue
                })
                .ToListAsync();
        }

        /// <summary>
        /// Обрабатывает файл - верифицирует значения файла для мапинга сущности Value,
        /// на основе верифицированных Value вычисляет значения сущности Result и после записывает файл в базу данных как blob.
        /// </summary>
        /// <param name="file">Обрабатываемый файл.</param>
        /// <param name="authorName">Имя автора файла.</param>
        /// <remarks>
        /// Метод проверяет существует ли информация об этом файле в базе данных.
        /// Если да, то происходит ее удаление и запись файла заново.
        /// Если происходит ошибка во время обработки, то генерируется исключение.
        /// </remarks>
        public async Task ProcessFileAsync(IFormFile file, string authorName)
        {
            if (await FileExistsInDB(file.FileName))
                await DeleteFileData(file.FileName);
            int minValidatedValuesCount = 1;
            int maxValidatedValuesCount = 10000;
            int bufferSize = 1000;
            try
            {
                await _csvFileReader.ReadValuesWithBufferAsync(
                    file,
                    minValidatedValuesCount,
                    maxValidatedValuesCount,
                    bufferSize
                );
                var result = await _resultProcessor.GenerateResultAsync(file.FileName);
                await _context.Results.AddAsync(result);
                await _context.SaveChangesAsync();
                await SaveFileAsBlobAsync(file, authorName, DateTime.Now);
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка во время обработки файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохраняет файл в базу данных как BLOB.
        /// </summary>
        /// <param name="file">Файл для сохранения.</param>
        /// <param name="authorName">Имя автора файла.</param>
        /// <param name="dateTimeOfUpload">Дата и время загрузки файла.</param>
        /// <remarks>
        /// Использует sql запрос напрямую к базе данных для эффективной записи файла без необходимости полной загрузки в память.
        /// </remarks>
        public async Task SaveFileAsBlobAsync(
            IFormFile file,
            string authorName,
            DateTime dateTimeOfUpload
        )
        {
            using var inputStream = file.OpenReadStream();
            using var connection = new SqliteConnection(_context.Database.GetConnectionString());

            await connection.OpenAsync();
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText =
                @"INSERT INTO Files (FileName, AuthorName, DateTimeOfUpload, Data) 
                VALUES (@fileName, @authorName, @dateTimeOfUpload, zeroblob(@length));
                SELECT last_insert_rowid();";

            insertCommand.Parameters.AddWithValue("@fileName", file.FileName);
            insertCommand.Parameters.AddWithValue("@authorName", authorName);
            insertCommand.Parameters.AddWithValue("@dateTimeOfUpload", dateTimeOfUpload.ToString());
            insertCommand.Parameters.AddWithValue("@length", inputStream.Length);

            var result = await insertCommand.ExecuteScalarAsync();
            long? id = result as long?;
            if (id is null)
                throw new Exception("Ошибка загрузки файла в базу данных.");
            using var writeStream = new SqliteBlob(connection, "Files", "Data", id.Value);
            await inputStream.CopyToAsync(writeStream);

            await connection.CloseAsync();
        }

        /// <summary>
        /// Обработчик события использующийся для обработки сущностей Value буфера полученного из CSVFileReader.
        /// Записывает сущности из буфера в базу данных и обрабатывает их для вычисления статистики для Result через вспомогательные методы.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="buffer">Буфер с сущностями для обработки.</param>
        private async Task BufferFilledHandlerAsync(object sender, List<Value> buffer)
        {
            try
            {
                await _context.BulkInsertAsync(buffer);
                _resultProcessor.UpdateResultInfoByBuffer(buffer);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка во время обработки файла.", ex);
            }
        }

        /// <summary>
        /// Используется для валидации строки из файла, обрабатываемого CSVFileReader.
        /// Верифицирует значения строки для мапинга сущности Value где:
        /// row[0] - представляет дату и время эксперимента в формате "ГГГГ-мм-дд_чч-мм-сс", значение должно быть от 2000-01-01 00:00:00 до настоящего момента;
        /// row[1] - представляет собой целое число - длительность эксперимента в секундах >0;
        /// row[2] - представляет собой число с плавающей запятой - показатель >0;
        /// </summary>
        /// <param name="row">строка для валидации.</param>
        /// <returns>Возвращает <c>true</c> если строка не прошла валидацию и должна быть пропущена, <c>false</c> если строка прошла валидацию и не должна быть пропущена.</returns>
        private bool ShouldSkipValue(IReaderRow readerRow)
        {
            const string dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
            const int maxRowLength = 3;
            var minDateTime = new DateTime(2000, 1, 1, 0, 0, 0);

            //если в строке больше 3 параметров, то Value не будет создаваться и такая строка отбрасывается
            var row = readerRow.Parser.Record;
            if (row is null || row.Length > maxRowLength)
                return true;

            if (!int.TryParse(row[1], out int duration) || duration <= 0)
                return true;

            if (!float.TryParse(row[2], out float indicatorValue) || indicatorValue <= 0)
                return true;
            //если дата и время не соответствуют формату
            if (!DateTime.TryParseExact(row[0], dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
                return true;

            if (parsedDateTime > DateTime.UtcNow || parsedDateTime < minDateTime)
                return true;

            return false;
        }
    }
}
