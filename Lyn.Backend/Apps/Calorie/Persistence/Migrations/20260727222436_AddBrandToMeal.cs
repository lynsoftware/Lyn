using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyn.Backend.Apps.Calorie.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandToMeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Meals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Meals");
        }
    }
}
