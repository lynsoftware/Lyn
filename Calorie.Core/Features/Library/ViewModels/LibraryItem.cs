using Calorie.Core.Features.Library.Models;

namespace Calorie.Core.Features.Library.ViewModels;

/// <summary>
/// Visningsadapter for bibliotek-lister (logge-flyten og bibliotek-siden) —
/// flater ut LibraryIngredient/LibraryMeal til én listeform.
/// Alt regnes i gram: kcal = gram x per-100g.
/// </summary>
public class LibraryItem
{
    // Original-objektet fra LibraryStore (LibraryIngredient eller LibraryMeal)
    public object? Source { get; set; }

    public LibraryItemKind Kind { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public decimal CaloriesPer100g { get; set; }

    // Makroer per 100 g — brukes til snapshot ved logging
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }

    // Foreslått mengde ved valg: måltid = porsjonen, stykk-vare = stykkvekten, ellers 100 g
    public decimal DefaultAmountGrams { get; set; } = 100;

    // Valgfri stykk-støtte: "1 stk = 60 g"
    public decimal? UnitWeightGrams { get; set; }
    public string? UnitName { get; set; }

    // Sekundærlinje i listen — språknøytral (kun tall og enheter)
    public string Details => Kind == LibraryItemKind.Meal
        ? $"{CaloriesFor(DefaultAmountGrams)} kcal · {DefaultAmountGrams:0.#} g"
        : $"{CaloriesPer100g:0.#} kcal / 100 g";

    public int CaloriesFor(decimal grams) => (int)Math.Round(CaloriesPer100g * grams / 100m);

    public static LibraryItem From(LibraryIngredient ingredient) => new()
    {
        Source = ingredient,
        Kind = LibraryItemKind.Ingredient,
        Name = ingredient.Name,
        Brand = ingredient.Brand,
        CaloriesPer100g = ingredient.CaloriesPer100g,
        ProteinPer100g = ingredient.ProteinPer100g,
        CarbsPer100g = ingredient.CarbsPer100g,
        FatPer100g = ingredient.FatPer100g,
        UnitWeightGrams = ingredient.UnitWeightGrams,
        UnitName = ingredient.UnitName,
        DefaultAmountGrams = ingredient.UnitWeightGrams ?? 100
    };

    public static LibraryItem From(LibraryMeal meal) => new()
    {
        Source = meal,
        Kind = LibraryItemKind.Meal,
        Name = meal.Name,
        Brand = meal.Brand,
        CaloriesPer100g = meal.CaloriesPer100g,
        ProteinPer100g = meal.ProteinPer100g,
        CarbsPer100g = meal.CarbsPer100g,
        FatPer100g = meal.FatPer100g,
        DefaultAmountGrams = meal.TotalGrams > 0 ? meal.TotalGrams : 100
    };
}
