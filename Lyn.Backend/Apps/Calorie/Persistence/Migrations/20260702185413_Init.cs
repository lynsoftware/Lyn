using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyn.Backend.Apps.Calorie.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ean = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    CaloriesPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    ProteinPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    CarbsPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    FatPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    UnitWeightGrams = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    UnitName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ImageStorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageThumbnailKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImageUploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageStorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageThumbnailKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImageUploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DailyCalories = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    ProteinGrams = table.Column<int>(type: "integer", nullable: true),
                    CarbsGrams = table.Column<int>(type: "integer", nullable: true),
                    FatGrams = table.Column<int>(type: "integer", nullable: true),
                    EffectiveFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    FoodProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CaloriesPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    ProteinPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    CarbsPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    FatPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    UnitWeightGrams = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    UnitName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ImageStorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageThumbnailKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImageUploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_FoodProducts_FoodProductId",
                        column: x => x.FoodProductId,
                        principalTable: "FoodProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    MealId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MealType = table.Column<int>(type: "integer", nullable: false),
                    LoggedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LoggedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogEntries_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MealBudgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealType = table.Column<int>(type: "integer", nullable: false),
                    MaxCalories = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealBudgets_UserGoals_UserGoalId",
                        column: x => x.UserGoalId,
                        principalTable: "UserGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountGrams = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealIngredients_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogEntryIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CaloriesPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    ProteinPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    CarbsPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    FatPer100g = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    AmountGrams = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntryIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogEntryIngredients_LogEntries_LogEntryId",
                        column: x => x.LogEntryId,
                        principalTable: "LogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodProducts_Ean",
                table: "FoodProducts",
                column: "Ean",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_FoodProductId",
                table: "Ingredients",
                column: "FoodProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_UserId",
                table: "Ingredients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_MealId",
                table: "LogEntries",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_UserId_LoggedDate",
                table: "LogEntries",
                columns: new[] { "UserId", "LoggedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LogEntryIngredients_LogEntryId",
                table: "LogEntryIngredients",
                column: "LogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_MealBudgets_UserGoalId_MealType",
                table: "MealBudgets",
                columns: new[] { "UserGoalId", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealIngredients_IngredientId",
                table: "MealIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_MealIngredients_MealId_IngredientId",
                table: "MealIngredients",
                columns: new[] { "MealId", "IngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meals_UserId",
                table: "Meals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_UserId_EffectiveFromDate",
                table: "UserGoals",
                columns: new[] { "UserId", "EffectiveFromDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogEntryIngredients");

            migrationBuilder.DropTable(
                name: "MealBudgets");

            migrationBuilder.DropTable(
                name: "MealIngredients");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropTable(
                name: "UserGoals");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Meals");

            migrationBuilder.DropTable(
                name: "FoodProducts");
        }
    }
}
