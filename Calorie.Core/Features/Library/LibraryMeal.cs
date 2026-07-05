namespace Calorie.Core.Features.Library;

/// <summary>
/// Et måltid i brukerens bibliotek — en komposisjon av ingredienser med
/// mengder i gram. Næringsverdiene beregnes alltid fra komponentene
/// (én kodevei: gram x per-100g). Lagres i lokal SQLite; speiler Meal
/// i backend.
/// </summary>
public class LibraryMeal
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<MealComponent> Components { get; set; } = new();

    public decimal TotalGrams => Components.Sum(c => c.AmountGrams);
    public int TotalCalories => Components.Sum(c => c.Calories);

    public decimal CaloriesPer100g => TotalGrams > 0 ? TotalCalories * 100m / TotalGrams : 0;

    public decimal ProteinPer100g => MacroPer100g(i => i.ProteinPer100g);
    public decimal CarbsPer100g => MacroPer100g(i => i.CarbsPer100g);
    public decimal FatPer100g => MacroPer100g(i => i.FatPer100g);

    private decimal MacroPer100g(Func<LibraryIngredient, decimal> selector) =>
        TotalGrams > 0
            ? Components.Sum(c => c.AmountGrams * (c.Ingredient == null ? 0 : selector(c.Ingredient)) / 100m) * 100m / TotalGrams
            : 0;
}
