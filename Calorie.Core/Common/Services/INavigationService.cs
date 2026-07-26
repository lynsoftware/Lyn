using Calorie.Core.Common.Enums;
using Calorie.Core.Features.Library.Models;

namespace Calorie.Core.Common.Services;

/// <summary>
/// Navigasjon sett fra ViewModels — semantiske mål, ingen rute-strenger
/// eller MAUI-typer. Implementeres av Shell-navigasjonen i appen;
/// testene mocker den.
/// </summary>
public interface INavigationService
{
    Task GoBackAsync();

    /// <summary>Tilbake til forrige side med et resultat (Shell-parameter).</summary>
    Task GoBackAsync(string resultKey, object resultValue);

    Task GoToIngredientEditorAsync(string? initialName = null);

    Task GoToMealBuilderAsync(string? initialName = null);

    /// <summary>Åpner måltidet i byggeren som engangs-kopi (Tilpass-modus).</summary>
    Task GoToCustomizeMealAsync(LibraryMeal meal);

    Task GoToEditIngredientAsync(LibraryIngredient ingredient);

    Task GoToEditMealAsync(LibraryMeal meal);

    /// <summary>Åpner logge-flyten — valgfri måltidstype og valgfri måldato (utelatt = i dag).</summary>
    Task GoToAddLogEntryAsync(MealType? mealType = null, DateOnly? date = null);

    /// <summary>Tilbake til dagsloggen (roten av navigasjonsstacken).</summary>
    Task GoHomeAsync();

    Task GoToLibraryAsync();

    Task GoToStatsAsync();

    Task GoToSettingsAsync();

    Task GoToGoalSettingsAsync();
}
