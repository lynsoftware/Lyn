using Calorie.Core.Features.Library.Models;

namespace Calorie.Core.Features.Library.ViewModels;

/// <summary>
/// Én komponentrad i måltidsbyggeren — ferdig formatert for visning.
/// Uforanderlig: radlisten bygges på nytt ved hver endring.
/// </summary>
public sealed record MealComponentRow(
    MealComponent Component,
    string Name,
    string AmountText,
    string CaloriesText);
