using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Produkt i den felles EAN-katalogen — delt av alle brukere, eid av ingen.
/// Én rad per EAN. Vedlikeholdes wiki-stil: alle kan endre, siste endring
/// vinner, men IsVerified-produkter (admin-innlagt) er låst for vanlige
/// brukere. Brukernes egne Ingredient-rader er KOPIER av produktet, så
/// endringer her flyter aldri automatisk inn i noens bibliotek eller logg.
/// </summary>
public class FoodProduct
{
    // ================== PRIMARY KEY ==================
    public Guid Id { get; set; }

    // ================== METADATA ==================

    // Strekkoden på pakken (EAN-8/13, opptil 14 siffer). Unik i katalogen.
    public string Ean { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    // Ingrediens eller ferdigmåltid — kun et UI-hint ved skanning, wiki-redigerbart
    public ProductKind Kind { get; set; } = ProductKind.Ingredient;

    public NutritionPer100G Nutrition { get; set; } = new();

    // Valgfri stykk-støtte: "1 stk = 60 g", "1 skive = 35 g"
    public decimal? UnitWeightGrams { get; set; }
    public string? UnitName { get; set; }

    // Valgfritt bilde — null hvis ingen har lastet opp noe
    public StoredImage? Image { get; set; }

    // ================== FELLESSKAPS-SPORING ==================

    // Løse referanser til AppUser.Id — ingen navigasjon på tvers av bounded contexts
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserId { get; set; }

    public ProductSource Source { get; set; } = ProductSource.User;

    // Kvalitetsstempel — admin-verifiserte produkter er låst for vanlige brukere
    public bool IsVerified { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
