using Calorie.Core.Features.Library;
using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using Calorie.Core.Features.Goals;
using Calorie.Core.Features.Goals.Models;
using Calorie.Core.Features.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Data;

/// <summary>
/// Inngangspunkt for den lokale databasen. Appen kaller Initialize ved
/// oppstart (med sti i appens sandkasse) — migrerer skjemaet og seeder
/// startdata første gang. Stores/repositories lager kortlevde contexter
/// via CreateContext.
/// </summary>
public static class LocalDb
{
    private static DbContextOptions<LocalDbContext>? _options;

    public static void Initialize(string databasePath)
    {
        _options = new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        using var db = CreateContext();
        db.Database.Migrate();
        SeedIfEmpty(db);
    }

    public static LocalDbContext CreateContext() =>
        new(_options ?? throw new InvalidOperationException("LocalDb.Initialize må kalles ved oppstart"));

    /// <summary>
    /// Førstegangsoppsett: standardmål (2000 kcal, maksgrense) og et lite
    /// startbibliotek av vanlige ingredienser, slik at appen ikke er tom.
    /// Ingen loggrader — dagsloggen starter blank.
    /// </summary>
    private static void SeedIfEmpty(LocalDbContext db)
    {
        if (!db.Goals.Any())
        {
            db.Goals.Add(new UserGoalRecord
            {
                DailyCalories = 2000,
                Mode = GoalMode.Limit,
                EffectiveFromDate = DateOnly.FromDateTime(DateTime.Now)
            });
        }

        if (!db.Ingredients.Any())
        {
            var oats = new LibraryIngredient { Name = "Havregryn", CaloriesPer100g = 370, ProteinPer100g = 13, CarbsPer100g = 58, FatPer100g = 7 };
            var milk = new LibraryIngredient { Name = "Lettmelk", Brand = "Tine", CaloriesPer100g = 37, ProteinPer100g = 3.5m, CarbsPer100g = 4.5m, FatPer100g = 0.5m };

            db.Ingredients.AddRange(
                new LibraryIngredient { Name = "Egg", CaloriesPer100g = 143, ProteinPer100g = 12.6m, CarbsPer100g = 0.7m, FatPer100g = 9.5m, UnitWeightGrams = 60, UnitName = "stk" },
                new LibraryIngredient { Name = "Brødskive, grov", CaloriesPer100g = 245, ProteinPer100g = 9, CarbsPer100g = 41, FatPer100g = 4, UnitWeightGrams = 40, UnitName = "skive" },
                new LibraryIngredient { Name = "Kyllingfilet", CaloriesPer100g = 106, ProteinPer100g = 23, CarbsPer100g = 0, FatPer100g = 1.5m },
                new LibraryIngredient { Name = "Ris, kokt", CaloriesPer100g = 130, ProteinPer100g = 2.7m, CarbsPer100g = 28, FatPer100g = 0.3m },
                new LibraryIngredient { Name = "Banan", CaloriesPer100g = 89, ProteinPer100g = 1.1m, CarbsPer100g = 23, FatPer100g = 0.3m, UnitWeightGrams = 120, UnitName = "stk" },
                oats, milk);

            db.Meals.Add(new LibraryMeal
            {
                Name = "Havregrøt med melk",
                Components =
                [
                    new MealComponent { Ingredient = oats, AmountGrams = 60 },
                    new MealComponent { Ingredient = milk, AmountGrams = 300 }
                ]
            });
        }

        db.SaveChanges();
    }
}
