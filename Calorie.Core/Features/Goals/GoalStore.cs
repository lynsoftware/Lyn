using Calorie.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.Goals;

/// <summary>
/// Mål i lokal SQLite med datert historikk (som backend): én rad per
/// endringsdag — ny endring samme dag oppdaterer raden, ellers opprettes
/// ny. Gamle dager måles dermed alltid mot målet som gjaldt da.
/// </summary>
public static class GoalStore
{
    public static Task<GoalSettings> GetCurrentAsync() =>
        GetForDateAsync(DateOnly.FromDateTime(DateTime.Now));

    public static async Task<GoalSettings> GetForDateAsync(DateOnly date)
    {
        await using var db = LocalDb.CreateContext();

        // Siste mål som gjaldt på datoen — fallback til eldste hvis datoen
        // er før første mål (f.eks. logger på gamle dager)
        var record = await db.Goals.AsNoTracking()
                         .Include(g => g.MealBudgets)
                         .Where(g => g.EffectiveFromDate <= date)
                         .OrderByDescending(g => g.EffectiveFromDate)
                         .FirstOrDefaultAsync()
                     ?? await db.Goals.AsNoTracking()
                         .Include(g => g.MealBudgets)
                         .OrderBy(g => g.EffectiveFromDate)
                         .FirstAsync();

        return new GoalSettings
        {
            DailyCalories = record.DailyCalories,
            Mode = record.Mode,
            ProteinGoalGrams = record.ProteinGoalGrams,
            CarbsGoalGrams = record.CarbsGoalGrams,
            FatGoalGrams = record.FatGoalGrams,
            MealBudgets = record.MealBudgets.ToDictionary(b => b.MealType, b => b.MaxCalories)
        };
    }

    public static async Task SaveAsync(GoalSettings settings)
    {
        await using var db = LocalDb.CreateContext();
        var today = DateOnly.FromDateTime(DateTime.Now);

        var record = await db.Goals.Include(g => g.MealBudgets)
            .FirstOrDefaultAsync(g => g.EffectiveFromDate == today);

        if (record == null)
        {
            record = new UserGoalRecord { EffectiveFromDate = today };
            db.Goals.Add(record);
        }
        else
        {
            record.UpdatedAtUtc = DateTime.UtcNow;
            db.MealBudgets.RemoveRange(record.MealBudgets);
        }

        record.DailyCalories = settings.DailyCalories;
        record.Mode = settings.Mode;
        record.ProteinGoalGrams = settings.ProteinGoalGrams;
        record.CarbsGoalGrams = settings.CarbsGoalGrams;
        record.FatGoalGrams = settings.FatGoalGrams;
        record.MealBudgets = settings.MealBudgets
            .Select(kvp => new MealBudgetRecord { UserGoalId = record.Id, MealType = kvp.Key, MaxCalories = kvp.Value })
            .ToList();

        await db.SaveChangesAsync();
    }
}
