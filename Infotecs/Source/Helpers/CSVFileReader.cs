using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Infotecs.Source.Data.Mappings;
using Infotecs.Source.Data.Models;

namespace Infotecs.Source.Helpers
{
    /// <summary>
    /// Класс используется для чтения сущностей Value из определенного файла с помощью буфера, не загружая файл полностью в память.
    /// </summary>
    public class CSVFileReader
    {
        //Обработчик события заполнения буфера, принимает буфер на обработку
        public delegate Task BufferFilledEventHandlerAsync(object sender, List<Value> buffer);
        public event BufferFilledEventHandlerAsync BufferFilled;
        private readonly CsvConfiguration _csvConfig;
        private readonly List<Value> buffer;

        /// <summary>
        /// Очищает буфер.
        /// </summary>
        private void ClearBuffer() => buffer.Clear();

        /// <summary>
        /// Создает новый объект CSVFileReader.
        /// </summary>
        /// <param name="shouldSkipRowDelegate">Делегат использующийся для валидации строки для мапинга сущности Value и пропуска этой строки при непрохождении валидации.</param>
        /// <param name="bufferFilledEventHandlerAsync">Обработчик события заполнения буфера, принимает буфер на обработку.</param>
        public CSVFileReader(
            Func<IReaderRow, bool> shouldSkipRowDelegate,
            BufferFilledEventHandlerAsync bufferFilledEventHandlerAsync
        )
        {
            buffer = new();
            BufferFilled += bufferFilledEventHandlerAsync;
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                AllowComments = false,
                ReadingExceptionOccurred = ex => false,
                ShouldSkipRecord = (record) => shouldSkipRowDelegate(record.Row),
            };
        }

        /// <summary>
        /// Читает значения для мапинга сущности Value из файла используя буфер.
        /// </summary>
        /// <param name="file">Файл из которого берутся значения.</param>
        /// <param name="minValuesCount">Минимальное число прошедших валидацию строк для мапинга.</param>
        /// <param name="maxValuesCount">Максимальное число прошедших валидацию строк для мапинга.</param>
        /// <param name="bufferSize">Размер буфера.</param>
        /// <remarks>
        /// Когда буфер заполняется происходит событие BufferFilled и после обработки этого события обработчиками буфер очищается и переходит к чтению следующего пакета значений.
        /// </remarks>
        public async Task ReadValuesWithBufferAsync(
            IFormFile file,
            int minValuesCount,
            int maxValuesCount,
            int bufferSize
        )
        {
            string fileName = file.FileName;
            using var streamReader = new StreamReader(file.OpenReadStream());
            using var csvReader = new CsvReader(streamReader, _csvConfig);
            csvReader.Context.RegisterClassMap<ValueMap>();
            //используется для подсчета общего количества строк,прошедших валидацию для мапинга
            int validatedValuesCount = 0;
            int bufferCount = 0;           
            while(await csvReader.ReadAsync())
            {
                if (validatedValuesCount == maxValuesCount)
                    break;
                if (bufferCount == bufferSize)
                {
                    await BufferFilled(this, buffer);
                    bufferCount = 0;
                    ClearBuffer();
                }
                var record = csvReader.GetRecord<Value>();
                if( record != null ) 
                {
                    record.FileName = fileName;
                    buffer.Add(record);
                    bufferCount++; 
                    validatedValuesCount++;
                }               
            }
            if (validatedValuesCount < minValuesCount)
                throw new ArgumentException(
                    $"Файл содержит менее чем {minValuesCount} валидную строку для мапинга сущности Value"
                );
            if (buffer.Count > 0)
                await BufferFilled(this, buffer);
        }
    }
}
