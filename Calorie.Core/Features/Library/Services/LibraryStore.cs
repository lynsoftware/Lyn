using Calorie.Core.Data;
using Calorie.Core.Features.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.Library.Services;

/// <summary>
/// Biblioteket i lokal SQLite: brukerens ingredienser og måltider.
/// Kortlevde contexter per kall; entitetene er frakoblet mellom kallene
/// og lagres via upsert. Tilstandsløs — registrert som singleton.
/// </summary>
public class LibraryStore : ILibraryStore
{
    public async Task<List<LibraryIngredient>> GetIngredientsAsync()
    {
        await using var db = LocalDb.CreateContext();
        return await db.Ingredients.AsNoTracking().OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<List<LibraryMeal>> GetMealsAsync()
    {
        await using var db = LocalDb.CreateContext();
        return await db.Meals.AsNoTracking()
            .Include(m => m.Components)
            .ThenInclude(c => c.Ingredient)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task SaveIngredientAsync(LibraryIngredient ingredient)
    {
        await using var db = LocalDb.CreateContext();

        var exists = await db.Ingredients.AnyAsync(i => i.Id == ingredient.Id);
        if (exists)
        {
            ingredient.UpdatedAtUtc = DateTime.UtcNow;
            db.Ingredients.Update(ingredient);
        }
        else
        {
            db.Ingredients.Add(ingredient);
        }

        await db.SaveChangesAsync();
    }

    public async Task SaveMealAsync(LibraryMeal meal)
    {
        await using var db = LocalDb.CreateContext();

        var existing = await db.Meals.Include(m => m.Components).FirstOrDefaultAsync(m => m.Id == meal.Id);
        if (existing == null)
        {
            // Nullstill Ingredient-navigasjonene så EF ikke prøver å
            // opprette ingrediensene på nytt — FK-id-ene er nok
            foreach (var component in meal.Components)
            {
                component.MealId = meal.Id;
                component.IngredientId = component.Ingredient?.Id ?? component.IngredientId;
                component.Ingredient = null!;
            }

            db.Meals.Add(meal);
        }
        else
        {
            existing.Name = meal.Name;
            existing.Brand = meal.Brand;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            // Komponentene erstattes i sin helhet — enklere enn diffing
            db.MealComponents.RemoveRange(existing.Components);
            existing.Components.Clear();

            foreach (var component in meal.Components)
            {
                db.MealComponents.Add(new MealComponent
                {
                    MealId = existing.Id,
                    IngredientId = component.Ingredient?.Id ?? component.IngredientId,
                    AmountGrams = component.AmountGrams
                });
            }
        }

        await db.SaveChangesAsync();
    }

    // Sjekk interface for summary
    public async Task<bool> DeleteIngredientAsync(Guid id)
    {
        await using var db = LocalDb.CreateContext();

        var inUse = await db.MealComponents.AnyAsync(c => c.IngredientId == id);
        if (inUse)
            return false;

        var ingredient = await db.Ingredients.FirstOrDefaultAsync(i => i.Id == id);
        if (ingredient == null)
            return true;

        db.Ingredients.Remove(ingredient);
        await db.SaveChangesAsync();
        return true;
    }

    // Sjekk interface for summary
    public async Task DeleteMealAsync(Guid id)
    {
        await using var db = LocalDb.CreateContext();

        var meal = await db.Meals.Include(m => m.Components).FirstOrDefaultAsync(m => m.Id == id);
        if (meal == null)
            return;

        db.MealComponents.RemoveRange(meal.Components);
        db.Meals.Remove(meal);
        await db.SaveChangesAsync();
    }
}
