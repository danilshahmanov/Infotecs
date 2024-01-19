using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infotecs.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", nullable: false),
                    DateTimeOfUpload = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileName);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FirstExperimentStartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    LastExperimentStartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    MinDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    MinIndicatorValue = table.Column<float>(type: "REAL", nullable: false),
                    MaxIndicatorValue = table.Column<float>(type: "REAL", nullable: false),
                    AverageIndicatorValue = table.Column<float>(type: "REAL", nullable: false),
                    MedianIndicatorValue = table.Column<float>(type: "REAL", nullable: false),
                    ExperimentsCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.FileName);
                });

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    IndicatorValue = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Values");
        }
    }
}
