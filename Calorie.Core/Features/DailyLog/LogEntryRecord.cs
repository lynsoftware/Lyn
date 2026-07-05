using Calorie.Core.Common;

namespace Calorie.Core.Features.DailyLog;

/// <summary>
/// Én loggført spising i lokal SQLite. Flatt SNAPSHOT: navn, mengde og
/// næringsverdier kopieres inn ved logging, så senere endringer i
/// biblioteket aldri endrer historikken. Per-ingrediens-snapshot
/// (backend LogEntryIngredient) mappes i sync-laget når det bygges.
/// </summary>
public class LogEntryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public MealType MealType { get; set; }

    // Brukerens LOKALE dato — all dags-gruppering bruker denne, aldri UTC-datoen
    public DateOnly LoggedDate { get; set; }

    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal AmountGrams { get; set; }

    // Snapshot av totalverdiene for mengden (ikke per 100 g)
    public int Calories { get; set; }
    public decimal ProteinGrams { get; set; }
    public decimal CarbsGrams { get; set; }
    public decimal FatGrams { get; set; }
}
