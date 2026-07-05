namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Måltidsmal eid av en bruker. Logging tar snapshot av malen
/// (LogEntryIngredient-rader), så endringer her påvirker aldri historikken.
/// </summary>
public class Meal
{
    // ================== PRIMARY KEY ================== 
    public Guid Id { get; set; }
    
    // ================== FOREIGN KEYS ================== 

    // Eier — AppUser.Id (Identity, string) fra Platform/Auth
    public string UserId { get; set; } = string.Empty;
    
    // ================== METADATA ================== 

    public string Name { get; set; } = string.Empty;

    // Valgfritt bilde — null hvis brukeren ikke har lastet opp noe
    public StoredImage? Image { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Til last-write-wins ved synk på tvers av enheter — null = aldri endret
    public DateTime? UpdatedAtUtc { get; set; }

    // ================== NAVIGATION PROPERTIES ==================
    public ICollection<MealIngredient> Ingredients { get; set; } = new List<MealIngredient>();
}
