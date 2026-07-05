using Calorie.Core.Data;
using Calorie.Core.Features.Library;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Features.Library;

/// <summary>
/// Biblioteket i lokal SQLite: brukerens ingredienser og måltider.
/// Kortlevde contexter per kall; entitetene er frakoblet mellom kallene
/// og lagres via upsert.
/// </summary>
public static class LibraryStore
{
    public static async Task<List<LibraryIngredient>> GetIngredientsAsync()
    {
        await using var db = LocalDb.CreateContext();
        return await db.Ingredients.AsNoTracking().OrderBy(i => i.Name).ToListAsync();
    }

    public static async Task<List<LibraryMeal>> GetMealsAsync()
    {
        await using var db = LocalDb.CreateContext();
        return await db.Meals.AsNoTracking()
            .Include(m => m.Components)
            .ThenInclude(c => c.Ingredient)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public static async Task SaveIngredientAsync(LibraryIngredient ingredient)
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

    public static async Task SaveMealAsync(LibraryMeal meal)
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
            existing.Components = meal.Components.Select(c => new MealComponent
            {
                MealId = existing.Id,
                IngredientId = c.Ingredient?.Id ?? c.IngredientId,
                AmountGrams = c.AmountGrams
            }).ToList();
        }

        await db.SaveChangesAsync();
    }
}
