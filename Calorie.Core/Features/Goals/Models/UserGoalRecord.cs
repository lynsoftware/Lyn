using Calorie.Core.Common.Enums;

namespace Calorie.Core.Features.Goals.Models;

/// <summary>
/// Brukerens mål i lokal SQLite — datert historikk som backend-entiteten
/// UserGoal: én rad per endringsdag, gamle dager måles mot målet som gjaldt da.
/// </summary>
public class UserGoalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int DailyCalories { get; set; }
    public GoalMode Mode { get; set; }

    public int? ProteinGoalGrams { get; set; }
    public int? CarbsGoalGrams { get; set; }
    public int? FatGoalGrams { get; set; }

    // Valgfri kos-andel av dagsmålet i prosent (null = av) — skalerer med dagsmålet
    public int? TreatPercent { get; set; }

    // Første dag målet gjelder for (lokal dato, samme akse som LogEntryRecord.LoggedDate)
    public DateOnly EffectiveFromDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<MealBudgetRecord> MealBudgets { get; set; } = new();
}
