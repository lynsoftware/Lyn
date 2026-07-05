using Calorie.Core.Common;

namespace Calorie.Core.Features.Stats;

/// <summary>
/// Én dag i statistikken: kcal spist mot målet som gjaldt DEN dagen
/// (datert målhistorikk).
/// </summary>
public class DayStat
{
    public DateOnly Date { get; set; }
    public int Calories { get; set; }
    public int GoalCalories { get; set; }
    public GoalMode Mode { get; set; }
    public bool HasEntries { get; set; }

    // Limit: innenfor grensen. Target: målet nådd.
    public bool OnTrack => Mode == GoalMode.Limit
        ? Calories <= GoalCalories
        : Calories >= GoalCalories;
}
