using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calorie.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatPercentToUserGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TreatPercent",
                table: "Goals",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TreatPercent",
                table: "Goals");
        }
    }
}
