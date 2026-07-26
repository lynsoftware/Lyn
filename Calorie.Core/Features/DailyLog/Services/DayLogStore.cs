using Calorie.Core.Common.Enums;
using Calorie.Core.Data;
using Calorie.Core.Features.DailyLog.Models;
using Calorie.Core.Features.Goals;
using Calorie.Core.Features.Goals.Services;
using Calorie.Core.Features.Library.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.DailyLog.Services;

/// <summary>
/// Dagsloggen i lokal SQLite. Bygger DayLog-visningen for en dato fra
/// loggradene + gjeldende mål (datert historikk via IGoalStore).
/// Tilstandsløs — registrert som singleton.
/// </summary>
public class DayLogStore : IDayLogStore
{
    private readonly IGoalStore _goalStore;

    public DayLogStore(IGoalStore goalStore)
    {
        _goalStore = goalStore;
    }

    // Sjekk interface for summary
    public async Task<DayLog> GetDayAsync(DateOnly date)
    {
        await using var db = LocalDb.CreateContext();

        var entries = await db.LogEntries.AsNoTracking()
            .Where(e => e.LoggedDate == date)
            .OrderBy(e => e.LoggedAtUtc)
            .ToListAsync();

        var goal = await _goalStore.GetForDateAsync(date);

        var day = new DayLog
        {
            Date = date,
            GoalCalories = goal.DailyCalories,
            Mode = goal.Mode,
            TreatPercent = goal.TreatPercent,
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
                        Id = e.Id,
                        MealType = e.MealType,
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

    // Sjekk interface for summary
    public async Task AddItemAsync(MealType mealType, LibraryItem item, decimal grams, DateOnly date)
    {
        await using var db = LocalDb.CreateContext();

        db.LogEntries.Add(new LogEntryRecord
        {
            MealType = mealType,
            LoggedDate = date,
            Name = item.Name,
            AmountGrams = grams,
            Calories = item.CaloriesFor(grams),
            ProteinGrams = grams * item.ProteinPer100g / 100m,
            CarbsGrams = grams * item.CarbsPer100g / 100m,
            FatGrams = grams * item.FatPer100g / 100m
        });

        await db.SaveChangesAsync();
    }

    // Sjekk interface for summary
    public async Task UpdateEntryAsync(Guid id, decimal newGrams, MealType newMealType)
    {
        if (newGrams <= 0)
            return;

        await using var db = LocalDb.CreateContext();

        var entry = await db.LogEntries.FirstOrDefaultAsync(e => e.Id == id);
        if (entry == null || entry.AmountGrams <= 0)
            return;

        var factor = newGrams / entry.AmountGrams;

        entry.Calories = (int)Math.Round(entry.Calories * factor);
        entry.ProteinGrams *= factor;
        entry.CarbsGrams *= factor;
        entry.FatGrams *= factor;
        entry.AmountGrams = newGrams;
        entry.MealType = newMealType;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task DeleteEntryAsync(Guid id)
    {
        await using var db = LocalDb.CreateContext();

        var entry = await db.LogEntries.FirstOrDefaultAsync(e => e.Id == id);
        if (entry == null)
            return;

        db.LogEntries.Remove(entry);
        await db.SaveChangesAsync();
    }
}
