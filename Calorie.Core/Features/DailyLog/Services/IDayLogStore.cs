using Calorie.Core.Common.Enums;
using Calorie.Core.Features.DailyLog.Models;
using Calorie.Core.Features.Library.ViewModels;

namespace Calorie.Core.Features.DailyLog.Services;

/// <summary>
/// Dagsloggen sett fra ViewModels. Implementeres av DayLogStore
/// (lokal SQLite); testene mocker den.
/// </summary>
public interface IDayLogStore
{
    /// <summary>Bygger DayLog-visningen for en dato fra loggradene + målet som gjaldt da.</summary>
    Task<DayLog> GetDayAsync(DateOnly date);

    /// <summary>
    /// Logger et bibliotek-innslag på gitt dato: flatt snapshot av navn,
    /// mengde og næringsverdier for mengden — historikken står seg uansett
    /// hva som senere endres i biblioteket.
    /// </summary>
    Task AddItemAsync(MealType mealType, LibraryItem item, decimal grams, DateOnly date);

    /// <summary>
    /// Endrer mengde og/eller måltidstype på en loggrad. Snapshot-verdiene
    /// er lineære i gram og skaleres proporsjonalt med ny mengde.
    /// </summary>
    Task UpdateEntryAsync(Guid id, decimal newGrams, MealType newMealType);

    Task DeleteEntryAsync(Guid id);
}
