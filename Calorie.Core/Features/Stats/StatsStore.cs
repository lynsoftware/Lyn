using Calorie.Core.Common;
using Calorie.Core.Data;
using Calorie.Core.Features.Stats;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.Stats;

/// <summary>
/// Statistikk beregnet lokalt fra dagsloggen — ingen backend involvert
/// (lokal-først). Hver dag måles mot målet som gjaldt den dagen.
/// </summary>
public static class StatsStore
{
    public static async Task<WeekStats> GetWeekAsync(DateOnly reference)
    {
        // Mandag som ukestart
        var weekStart = reference.AddDays(-(((int)reference.DayOfWeek + 6) % 7));
        var weekEnd = weekStart.AddDays(6);

        await using var db = LocalDb.CreateContext();

        var entries = await db.LogEntries.AsNoTracking()
            .Where(e => e.LoggedDate >= weekStart && e.LoggedDate <= weekEnd)
            .ToListAsync();

        // Målhistorikken er liten — lastes én gang og slås opp per dag
        var goals = await db.Goals.AsNoTracking()
            .OrderBy(g => g.EffectiveFromDate)
            .ToListAsync();

        var stats = new WeekStats { WeekStart = weekStart };

        for (var i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            var dayEntries = entries.Where(e => e.LoggedDate == date).ToList();
            var goal = goals.LastOrDefault(g => g.EffectiveFromDate <= date) ?? goals.FirstOrDefault();

            stats.Days.Add(new DayStat
            {
                Date = date,
                Calories = dayEntries.Sum(e => e.Calories),
                GoalCalories = goal?.DailyCalories ?? 0,
                Mode = goal?.Mode ?? GoalMode.Limit,
                HasEntries = dayEntries.Count > 0
            });
        }

        stats.MealTypeCalories = entries
            .GroupBy(e => e.MealType)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Calories));

        return stats;
    }
}
