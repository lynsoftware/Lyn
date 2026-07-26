using Calorie.Core.Data;
using Calorie.Core.Features.Goals.Models;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.Goals.Services;

/// <summary>
/// Mål i lokal SQLite med datert historikk (som backend): én rad per
/// endringsdag — ny endring samme dag oppdaterer raden, ellers opprettes
/// ny. Gamle dager måles dermed alltid mot målet som gjaldt da.
/// Tilstandsløs — registrert som singleton.
/// </summary>
public class GoalStore : IGoalStore
{
    private readonly TimeProvider _time;

    public GoalStore(TimeProvider time)
    {
        _time = time;
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    public Task<GoalSettings> GetCurrentAsync() => GetForDateAsync(Today);

    // Sjekk interface for summary
    public async Task<GoalSettings> GetForDateAsync(DateOnly date)
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
            TreatPercent = record.TreatPercent,
            MealBudgets = record.MealBudgets.ToDictionary(b => b.MealType, b => b.MaxCalories)
        };
    }

    // Sjekk interface for summary
    public async Task SaveAsync(GoalSettings settings)
    {
        await using var db = LocalDb.CreateContext();
        var today = Today;

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
        record.TreatPercent = settings.TreatPercent;

        // Budsjettene må Add-es EKSPLISITT via DbSet-et: Guid-id-en settes av
        // initialisereren, så navigasjons-oppdagelse ville merket dem Modified
        // — UPDATE mot rader som ikke finnes → DbUpdateConcurrencyException.
        record.MealBudgets.Clear();
        foreach (var (mealType, maxCalories) in settings.MealBudgets)
        {
            db.MealBudgets.Add(new MealBudgetRecord
            {
                UserGoalId = record.Id,
                MealType = mealType,
                MaxCalories = maxCalories
            });
        }

        await db.SaveChangesAsync();
    }
}
