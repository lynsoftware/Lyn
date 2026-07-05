namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Snapshot av én ingrediens i en logget spising. Navn og næringsverdier
/// kopieres fra Ingredient på loggetidspunktet — bevisst ingen FK til
/// Ingredient, slik at historikken står seg selv om biblioteket endres.
/// </summary>
public class LogEntryIngredient
{
    // ================== PRIMARY KEY ================== 
    public Guid Id { get; set; }

    // ================== FOREIGN KEYS ================== 
    public Guid LogEntryId { get; set; }
    
    // ================== METADATA ================== 
    
    // Kopi av Ingredient.Name på loggetidspunktet
    public string IngredientName { get; set; } = string.Empty;

    public NutritionPer100G Nutrition { get; set; } = new();
    
    public decimal AmountGrams { get; set; }
    
    // ================== NAVIGATION PROPERTIES ================== 
    public LogEntry LogEntry { get; set; } = null!;
    
}
