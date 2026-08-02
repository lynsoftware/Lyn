namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

/// <summary>
/// Sync-radformat for én komponent i et måltid — speiler klientens
/// MealComponent. Reiser alltid nøstet i SyncMealDto, så MealId ligger
/// implisitt i nøstingen (serveren setter FK selv). Ingen nærings-
/// eller kcal-felt: de beregnes alltid fra ingrediensen (én beregningsvei).
/// </summary>
public class SyncMealComponentDto
{
    public Guid Id { get; set; }

    public Guid IngredientId { get; set; }

    public decimal AmountGrams { get; set; }
}
