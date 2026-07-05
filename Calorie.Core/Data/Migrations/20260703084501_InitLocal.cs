using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calorie.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DailyCalories = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    ProteinGoalGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    CarbsGoalGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    FatGoalGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    EffectiveFromDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: true),
                    CaloriesPer100g = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProteinPer100g = table.Column<decimal>(type: "TEXT", nullable: false),
                    CarbsPer100g = table.Column<decimal>(type: "TEXT", nullable: false),
                    FatPer100g = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitWeightGrams = table.Column<decimal>(type: "TEXT", nullable: true),
                    UnitName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MealType = table.Column<int>(type: "INTEGER", nullable: false),
                    LoggedDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LoggedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AmountGrams = table.Column<decimal>(type: "TEXT", nullable: false),
                    Calories = table.Column<int>(type: "INTEGER", nullable: false),
                    ProteinGrams = table.Column<decimal>(type: "TEXT", nullable: false),
                    CarbsGrams = table.Column<decimal>(type: "TEXT", nullable: false),
                    FatGrams = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MealBudgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserGoalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MealType = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxCalories = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealBudgets_Goals_UserGoalId",
                        column: x => x.UserGoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MealId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountGrams = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealComponents_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealComponents_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_EffectiveFromDate",
                table: "Goals",
                column: "EffectiveFromDate");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_LoggedDate",
                table: "LogEntries",
                column: "LoggedDate");

            migrationBuilder.CreateIndex(
                name: "IX_MealBudgets_UserGoalId_MealType",
                table: "MealBudgets",
                columns: new[] { "UserGoalId", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealComponents_IngredientId",
                table: "MealComponents",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_MealComponents_MealId",
                table: "MealComponents",
                column: "MealId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "MealBudgets");

            migrationBuilder.DropTable(
                name: "MealComponents");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Meals");
        }
    }
}
