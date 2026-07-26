using Calorie.Core.Features.Library.Models;

namespace Calorie.Core.Features.Library.Services;

/// <summary>
/// Biblioteket sett fra ViewModels: brukerens ingredienser og måltider.
/// Implementeres av LibraryStore (lokal SQLite); testene mocker den.
/// </summary>
public interface ILibraryStore
{
    Task<List<LibraryIngredient>> GetIngredientsAsync();

    Task<List<LibraryMeal>> GetMealsAsync();

    Task SaveIngredientAsync(LibraryIngredient ingredient);

    Task SaveMealAsync(LibraryMeal meal);

    /// <summary>
    /// Sletter en ingrediens — men ALDRI en som brukes i et måltid (Restrict,
    /// samme regel som backend): da returneres false og ingenting slettes.
    /// </summary>
    Task<bool> DeleteIngredientAsync(Guid id);

    /// <summary>
    /// Sletter et måltid med komponentene. Alltid lov — loggen er snapshot
    /// og peker aldri på måltidet.
    /// </summary>
    Task DeleteMealAsync(Guid id);
}
