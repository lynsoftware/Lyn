using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyn.Backend.Apps.Calorie.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatPercentToUserGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TreatPercent",
                table: "UserGoals",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TreatPercent",
                table: "UserGoals");
        }
    }
}
