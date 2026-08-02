using Lyn.Backend.Apps.Calorie.Models;
using Lyn.Backend.Apps.Calorie.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Apps.Calorie.Sync.Repositories;

// Sjekk interface for summaries. Hvert kall filtrerer på UserId — en annen
// brukers Guid gir bare "finnes ikke", aldri tilgang til fremmede rader.
public class SyncRepository(CalorieDbContext db) : ISyncRepository
{
    // ===== State — ren lesing =====

    public async Task<List<Ingredient>> GetAllIngredientsAsync(string userId, CancellationToken ct = default) =>
        await db.Ingredients.AsNoTracking()
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

    public async Task<List<Meal>> GetAllMealsAsync(string userId, CancellationToken ct = default) =>
        await db.Meals.AsNoTracking()
            .Where(m => m.UserId == userId)
            .Include(m => m.Ingredients)
            .ToListAsync(ct);

    public async Task<List<LogEntry>> GetAllLogEntriesAsync(string userId, CancellationToken ct = default) =>
        await db.LogEntries.AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Ingredients)
            .ToListAsync(ct);

    public async Task<List<UserGoal>> GetAllGoalsAsync(string userId, CancellationToken ct = default) =>
        await db.UserGoals.AsNoTracking()
            .Where(g => g.UserId == userId)
            .Include(g => g.MealBudgets)
            .ToListAsync(ct);

    // ===== Push — sporede oppslag for LWW =====

    public async Task<Dictionary<Guid, Ingredient>> GetIngredientsByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Ingredients
            .Where(i => i.UserId == userId && ids.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, ct);

    public async Task<Dictionary<Guid, Meal>> GetMealsByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Meals
            .Where(m => m.UserId == userId && ids.Contains(m.Id))
            .Include(m => m.Ingredients)
            .ToDictionaryAsync(m => m.Id, ct);

    public async Task<Dictionary<Guid, LogEntry>> GetLogEntriesByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.LogEntries
            .Where(e => e.UserId == userId && ids.Contains(e.Id))
            .Include(e => e.Ingredients)
            .ToDictionaryAsync(e => e.Id, ct);

    public async Task<Dictionary<Guid, UserGoal>> GetGoalsByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.UserGoals
            .Where(g => g.UserId == userId && ids.Contains(g.Id))
            .Include(g => g.MealBudgets)
            .ToDictionaryAsync(g => g.Id, ct);

    // ===== Push — skriveoperasjoner (kun change-trackeren, ingen lagring her) =====

    public void AddIngredient(Ingredient ingredient) => db.Ingredients.Add(ingredient);
    public void AddMeal(Meal meal) => db.Meals.Add(meal);
    public void AddLogEntry(LogEntry logEntry) => db.LogEntries.Add(logEntry);
    public void AddUserGoal(UserGoal goal) => db.UserGoals.Add(goal);

    public void RemoveIngredient(Ingredient ingredient) => db.Ingredients.Remove(ingredient);
    public void RemoveMeal(Meal meal) => db.Meals.Remove(meal);
    public void RemoveLogEntry(LogEntry logEntry) => db.LogEntries.Remove(logEntry);
    public void RemoveUserGoal(UserGoal goal) => db.UserGoals.Remove(goal);

    public void RemoveMealIngredients(IEnumerable<MealIngredient> components) =>
        db.MealIngredients.RemoveRange(components);

    public void RemoveMealBudgets(IEnumerable<MealBudget> budgets) =>
        db.MealBudgets.RemoveRange(budgets);

    public void AddMealIngredients(IEnumerable<MealIngredient> components) =>
        db.MealIngredients.AddRange(components);

    public void AddMealBudgets(IEnumerable<MealBudget> budgets) =>
        db.MealBudgets.AddRange(budgets);

    public void AddLogEntryIngredients(IEnumerable<LogEntryIngredient> snapshotRows) =>
        db.LogEntryIngredients.AddRange(snapshotRows);

    public void RemoveLogEntryIngredients(IEnumerable<LogEntryIngredient> snapshotRows) =>
        db.LogEntryIngredients.RemoveRange(snapshotRows);

    // ===== Sletteretten og commit =====

    public async Task DeleteAllForUserAsync(string userId, CancellationToken ct = default)
    {
        // ExecuteDelete går utenom change-trackeren og kjører umiddelbart —
        // derfor egen transaksjon rundt alle syv, og barn før foreldre
        // (MealIngredient→Ingredient er Restrict; rekkefølgen er ikke valgfri)
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await db.MealIngredients.Where(mi => mi.Meal.UserId == userId).ExecuteDeleteAsync(ct);
        await db.LogEntryIngredients.Where(li => li.LogEntry.UserId == userId).ExecuteDeleteAsync(ct);
        await db.MealBudgets.Where(b => b.UserGoal.UserId == userId).ExecuteDeleteAsync(ct);

        await db.Meals.Where(m => m.UserId == userId).ExecuteDeleteAsync(ct);
        await db.LogEntries.Where(e => e.UserId == userId).ExecuteDeleteAsync(ct);
        await db.Ingredients.Where(i => i.UserId == userId).ExecuteDeleteAsync(ct);
        await db.UserGoals.Where(g => g.UserId == userId).ExecuteDeleteAsync(ct);

        await tx.CommitAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
