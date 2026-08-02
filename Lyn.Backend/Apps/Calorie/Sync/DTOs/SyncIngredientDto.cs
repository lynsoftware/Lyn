namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

/// <summary>
/// Sync-radformat for en ingrediens — speiler klientens LibraryIngredient fra Calorie.Core/Library.
/// Brukes begge veier (push og state). Alle verdier kommer fra klienten;
/// ingen genererende defaults, så manglende felt blir synlig søppel
/// (Guid.Empty / år 0001) som servicen kan avvise.
/// </summary>
public class SyncIngredientDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }

    // Valgfri stykk-støtte: "1 stk = 60 g"
    public decimal? UnitWeightGrams { get; set; }
    public string? UnitName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}