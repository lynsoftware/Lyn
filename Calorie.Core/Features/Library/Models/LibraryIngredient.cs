namespace Calorie.Core.Features.Library.Models;

/// <summary>
/// En ingrediens i brukerens bibliotek — næringsverdier per 100 g, valgfri
/// stykkvekt. Lagres i lokal SQLite; Guid-id genereres på klienten
/// (offline-trygt) og UpdatedAtUtc brukes til last-write-wins ved sync.
/// Speiler backend-entiteten Ingredient.
/// </summary>
public class LibraryIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }

    // Valgfri stykk-støtte: "1 stk = 60 g"
    public decimal? UnitWeightGrams { get; set; }
    public string? UnitName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public int CaloriesFor(decimal grams) => (int)Math.Round(CaloriesPer100g * grams / 100m);
}
