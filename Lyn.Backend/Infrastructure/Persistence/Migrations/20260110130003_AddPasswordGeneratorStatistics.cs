using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lyn.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordGeneratorStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordGeneratorUsageStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PasswordsGenerated = table.Column<int>(type: "integer", nullable: false),
                    WindowsDownloads = table.Column<int>(type: "integer", nullable: false),
                    ApkDownloads = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordGeneratorUsageStatistics", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PasswordGeneratorUsageStatistics",
                columns: new[] { "Id", "ApkDownloads", "PasswordsGenerated", "WindowsDownloads" },
                values: new object[] { 1, 0, 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordGeneratorUsageStatistics");
        }
    }
}
