using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

public class SyncUserGoalDto
{
    public Guid Id { get; set; }

    public int DailyCalories { get; set; }
    public GoalMode Mode { get; set; }

    public int? ProteinGoalGrams { get; set; }
    public int? CarbsGoalGrams { get; set; }
    public int? FatGoalGrams { get; set; }

    // Valgfri kos-andel av dagsmålet i prosent (null = av) — skalerer med dagsmålet
    public int? TreatPercent { get; set; }

    // Første dag målet gjelder for (lokal dato, samme akse som LogEntryRecord.LoggedDate)
    public DateOnly EffectiveFromDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public List<SyncMealBudgetDto> MealBudgets { get; set; } = [];
}