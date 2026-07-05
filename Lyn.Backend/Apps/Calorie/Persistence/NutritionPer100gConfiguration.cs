using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

/// <summary>
/// Felles kolonne-mapping for NutritionPer100g (owned type). Brukes av både
/// Ingredient og LogEntryIngredient slik at nye næringsfelter konfigureres
/// ett sted. Kolonnenavn får Per100g-suffiks for tydelighet i databasen.
/// </summary>
internal static class NutritionPer100gConfiguration
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, NutritionPer100G> nutrition)
        where TOwner : class
    {
        nutrition.Property(n => n.Calories).HasColumnName("CaloriesPer100g").HasPrecision(7, 2);
        nutrition.Property(n => n.Protein).HasColumnName("ProteinPer100g").HasPrecision(7, 2);
        nutrition.Property(n => n.Carbs).HasColumnName("CarbsPer100g").HasPrecision(7, 2);
        nutrition.Property(n => n.Fat).HasColumnName("FatPer100g").HasPrecision(7, 2);
    }
}
