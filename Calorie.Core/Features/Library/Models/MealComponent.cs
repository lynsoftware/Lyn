namespace Calorie.Core.Features.Library.Models;

/// <summary>
/// Én ingrediens i et måltid, med mengde i gram. Speiler MealIngredient
/// i backend. Ingredient-navigasjonen må lastes (Include) for beregning.
/// </summary>
public class MealComponent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MealId { get; set; }
    public Guid IngredientId { get; set; }

    public LibraryIngredient Ingredient { get; set; } = null!;

    public decimal AmountGrams { get; set; }

    public int Calories => Ingredient?.CaloriesFor(AmountGrams) ?? 0;
}
