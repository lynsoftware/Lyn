using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// En logget spising. Navn og ingredienser er SNAPSHOT av måltidet på
/// loggetidspunktet — redigering eller sletting av mal/ingrediens endrer
/// aldri loggen. MealId er kun en løs referanse tilbake til malen
/// (settes til null hvis malen slettes).
/// </summary>
public class LogEntry
{
    // ================== PRIMARY KEY ================== 
    public Guid Id { get; set; }
    
    // ================== FOREIGN KEYS ================== 
    
    // Eier — AppUser.Id (Identity, string) fra Platform/Auth
    public string UserId { get; set; } = string.Empty;
    
    public Guid? MealId { get; set; }
    
    // ================== METADATA ================== 

    // Kopi av måltidsnavnet på loggetidspunktet
    public string Name { get; set; } = string.Empty;

    // Frokost/lunsj/middag/snack — klienten foreslår, brukeren kan overstyre
    public MealType MealType { get; set; }

    // Brukerens LOKALE dato, satt av klienten. All dags-gruppering (dashboard,
    // dagslogg) bruker denne — aldri UTC-datoen, ellers havner kveldsmat kl. 23:30
    // på feil dag.
    public DateOnly LoggedDate { get; set; }

    // Eksakt tidspunkt (UTC) — til sortering innad i dagen og revisjon
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;

    // Til last-write-wins ved synk på tvers av enheter — null = aldri endret
    public DateTime? UpdatedAtUtc { get; set; }

    
    // ================== NAVIGATION PROPERTIES ================== 
    
    public Meal? Meal { get; set; }

    public ICollection<LogEntryIngredient> Ingredients { get; set; } = new List<LogEntryIngredient>();
}
