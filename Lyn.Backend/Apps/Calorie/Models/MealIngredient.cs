namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Koblingsrad mellom Meal og Ingredient med mengde i gram.
/// Mengder lagres alltid i gram — stykk-visning er et klientanliggende.
/// </summary>
public class MealIngredient
{
    // ================== PRIMARY KEY ================== 
    public Guid Id { get; set; }
    
    // ================== FOREIGN KEYS ================== 
    public Guid MealId { get; set; }
    public Guid IngredientId { get; set; }
    
    // ================== METADATA ================== 

    public decimal AmountGrams { get; set; }
    
    // ================== NAVIGATION PROPERTIES ================== 
    public Meal Meal { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
