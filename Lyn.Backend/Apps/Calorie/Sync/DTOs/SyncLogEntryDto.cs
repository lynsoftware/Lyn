using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

public class SyncLogEntryDto
{
    public Guid Id { get; set; }

    public MealType MealType { get; set; }

    // Brukerens LOKALE dato — all dags-gruppering bruker denne, aldri UTC-datoen
    public DateOnly LoggedDate { get; set; }

    public DateTime LoggedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal AmountGrams { get; set; }

    // Snapshot av totalverdiene for mengden (ikke per 100 g)
    public int Calories { get; set; }
    public decimal ProteinGrams { get; set; }
    public decimal CarbsGrams { get; set; }
    public decimal FatGrams { get; set; }
}