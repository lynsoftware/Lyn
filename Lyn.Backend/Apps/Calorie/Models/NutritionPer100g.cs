namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Næringsverdier per 100 gram. Delt value object (owned type) mellom
/// Ingredient og LogEntryIngredient — nye felter (f.eks. fiber, sukker)
/// legges til her én gang og gjelder begge.
/// </summary>
public class NutritionPer100G
{
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
}
