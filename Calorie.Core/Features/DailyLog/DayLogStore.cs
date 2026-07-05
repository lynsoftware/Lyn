using Calorie.Core.Features.Library;
using Calorie.Core.Common;
using Calorie.Core.Features.Goals;
using Calorie.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.DailyLog;

/// <summary>
/// Dagsloggen i lokal SQLite. Bygger DayLog-visningen for en dato fra
/// loggradene + gjeldende mål (datert historikk via GoalStore).
/// </summary>
public static class DayLogStore
{
    public static Task<DayLog> GetTodayAsync() =>
        GetDayAsync(DateOnly.FromDateTime(DateTime.Now));

    public static async Task<DayLog> GetDayAsync(DateOnly date)
    {
        await using var db = LocalDb.CreateContext();

        var entries = await db.LogEntries.AsNoTracking()
            .Where(e => e.LoggedDate == date)
            .OrderBy(e => e.LoggedAtUtc)
            .ToListAsync();

        var goal = await GoalStore.GetForDateAsync(date);

        var day = new DayLog
        {
            Date = date,
            GoalCalories = goal.DailyCalories,
            Mode = goal.Mode,
            ProteinGoalGrams = goal.ProteinGoalGrams,
            CarbsGoalGrams = goal.CarbsGoalGrams,
            FatGoalGrams = goal.FatGoalGrams,
            ProteinGrams = (int)Math.Round(entries.Sum(e => e.ProteinGrams)),
            CarbsGrams = (int)Math.Round(entries.Sum(e => e.CarbsGrams)),
            FatGrams = (int)Math.Round(entries.Sum(e => e.FatGrams)),
            Sections = entries
                .GroupBy(e => e.MealType)
                .Select(g => new MealSection
                {
                    MealType = g.Key,
                    Items = g.Select(e => new LoggedItem
                    {
                        Name = e.Name,
                        AmountGrams = e.AmountGrams,
                        Calories = e.Calories
                    }).ToList()
                })
                .ToList()
        };

        // Budsjetterte måltidstyper uten logg får tom seksjon ("legg til"-kort)
        foreach (var mealType in Enum.GetValues<MealType>())
        {
            var hasBudget = goal.MealBudgets.TryGetValue(mealType, out var budget);
            var section = day.Sections.FirstOrDefault(s => s.MealType == mealType);

            if (section == null && hasBudget)
            {
                section = new MealSection { MealType = mealType };
                day.Sections.Add(section);
            }

            if (section != null)
                section.MaxCalories = hasBudget ? budget : null;
        }

        return day;
    }

    /// <summary>
    /// Logger et bibliotek-innslag: flatt snapshot av navn, mengde og
    /// næringsverdier for mengden — historikken står seg uansett hva som
    /// senere endres i biblioteket.
    /// </summary>
    public static async Task AddItemAsync(MealType mealType, LibraryItem item, decimal grams)
    {
        await using var db = LocalDb.CreateContext();

        db.LogEntries.Add(new LogEntryRecord
        {
            MealType = mealType,
            LoggedDate = DateOnly.FromDateTime(DateTime.Now),
            Name = item.Name,
            AmountGrams = grams,
            Calories = item.CaloriesFor(grams),
            ProteinGrams = grams * item.ProteinPer100g / 100m,
            CarbsGrams = grams * item.CarbsPer100g / 100m,
            FatGrams = grams * item.FatPer100g / 100m
        });

        await db.SaveChangesAsync();
    }
}
