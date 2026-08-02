namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

/// <summary>
/// Sync-radformat for et måltid — speiler klientens LibraryMeal.
/// Brukes begge veier (push og state). Komponentene reiser alltid med
/// måltidet; ved push erstattes serverens komponenter i sin helhet
/// (samme semantikk som klientens SaveMealAsync — enklere enn diffing).
/// Ingen TotalGrams/CaloriesPer100g: avledede verdier beregnes, aldri synces.
/// </summary>
public class SyncMealDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public List<SyncMealComponentDto> Components { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
