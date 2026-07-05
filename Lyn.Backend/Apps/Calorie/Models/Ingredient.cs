namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// En ingrediens i brukerens personlige bibliotek. Næringsverdier angis
/// alltid per 100 g. UnitWeightGrams/UnitName gir valgfri stykk-støtte
/// ("1 stk = 60 g") — klienten regner om til gram før lagring, slik at
/// all kaloriberegning har én kodevei (gram x per-100g).
/// </summary>
public class Ingredient
{
    // ================== PRIMARY KEY ================== 
    public Guid Id { get; set; }
    
    // ================== FOREIGN KEYS ==================

    // Eier — AppUser.Id (Identity, string) fra Platform/Auth. Ingen navigasjon
    // på tvers av bounded contexts.
    public string UserId { get; set; } = string.Empty;

    // Kobling til felleskatalogen hvis ingrediensen ble lagt til via EAN-skann.
    // Verdiene er KOPIERT inn hit — katalogendringer flyter aldri automatisk
    // inn i brukerens bibliotek. SetNull hvis produktet slettes.
    public Guid? FoodProductId { get; set; }
    
    // ================== METADATA ================== 
    
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public NutritionPer100G Nutrition { get; set; } = new();

    // Valgfri stykk-støtte: "1 stk = 60 g", "1 skive = 35 g"
    public decimal? UnitWeightGrams { get; set; }
    public string? UnitName { get; set; }

    // Valgfritt bilde — null hvis brukeren ikke har lastet opp noe
    public StoredImage? Image { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Til last-write-wins ved synk på tvers av enheter — null = aldri endret
    public DateTime? UpdatedAtUtc { get; set; }

    // ================== NAVIGATION PROPERTIES ==================
    public FoodProduct? FoodProduct { get; set; }
}
