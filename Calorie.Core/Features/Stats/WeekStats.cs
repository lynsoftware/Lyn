using Calorie.Core.Common;

namespace Calorie.Core.Features.Stats;

/// <summary>
/// Ukesstatistikk: én DayStat per dag (man-søn) + fordeling per måltidstype.
/// </summary>
public class WeekStats
{
    public DateOnly WeekStart { get; set; }

    public List<DayStat> Days { get; set; } = new();

    // Sum kcal per måltidstype for hele uken — til fordelingsvisningen
    public Dictionary<MealType, int> MealTypeCalories { get; set; } = new();

    public int LoggedDays => Days.Count(d => d.HasEntries);

    public int TotalCalories => Days.Sum(d => d.Calories);

    public int AverageCalories => LoggedDays > 0
        ? (int)Math.Round((double)TotalCalories / LoggedDays)
        : 0;

    public int DaysOnTrack => Days.Count(d => d.HasEntries && d.OnTrack);
}
