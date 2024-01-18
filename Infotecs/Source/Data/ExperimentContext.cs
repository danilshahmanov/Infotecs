using System.Data;
using Infotecs.Source.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Infotecs.Source.Data
{
    public class ExperimentContext : DbContext
    {
        public ExperimentContext(DbContextOptions<ExperimentContext> options)
            : base(options) { }

        public DbSet<Result> Results => Set<Result>();
        public DbSet<Value> Values => Set<Value>();
        public DbSet<CSVFile> Files => Set<CSVFile>();

        /// <summary>
        /// Проверяет существуют ли все необходимые таблицы в базе данных.
        /// </summary>
        /// <returns>
        /// Возвращает <c>true</c> если все необходимые таблицы существуют и <c>false</c> если хотя бы 1 необходимая таблица не существует.
        /// </returns>
        /// <remarks>
        /// Метод итерируется через все сущности в контексте и проверяет существование соответсвующих им таблиц в базе данных.
        /// </remarks>
        public async Task<bool> CheckDbAsync()
        {
            var tables = Model
                .GetEntityTypes()
                .Select(e => Model.FindEntityType(e.Name)?.GetTableName())
                .Distinct()
                .ToList();

            foreach (var table in tables)
            {
                if (!await CheckTableExistsInDbAsync(table))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Пересоздает базу данных: удаляет существующую и создает новую.
        /// </summary>
        public async Task RecreateDb()
        {
            await Database.EnsureDeletedAsync();
            await Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Проверяет существование таблицы в базе данных.
        /// </summary>
        /// <param name="tableName">Имя таблицы для проверки.</param>
        /// <returns>
        /// Возвращает <c>true</c> если таблица существует и <c>false</c> если не существует.
        /// </returns>
        /// <remarks>
        /// Этот метод использует прямой sql запрос в базу данных для проверки существования таблицы.
        /// </remarks>
        private async Task<bool> CheckTableExistsInDbAsync(string tableName)
        {
            using var connection = Database.GetDbConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            return Convert.ToInt32(result) > 0;
        }
    }
}
