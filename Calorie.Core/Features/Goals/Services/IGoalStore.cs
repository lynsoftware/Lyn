using Calorie.Core.Features.Goals.Models;

namespace Calorie.Core.Features.Goals.Services;

/// <summary>
/// Mål sett fra ViewModels, med datert historikk (som backend).
/// Implementeres av GoalStore (lokal SQLite); testene mocker den.
/// </summary>
public interface IGoalStore
{
    /// <summary>Målet som gjelder i dag.</summary>
    Task<GoalSettings> GetCurrentAsync();

    /// <summary>
    /// Målet som gjaldt på datoen — fallback til eldste hvis datoen er før
    /// første mål (f.eks. logging på gamle dager).
    /// </summary>
    Task<GoalSettings> GetForDateAsync(DateOnly date);

    /// <summary>
    /// Lagrer målet med virkning fra i dag: ny endring samme dag oppdaterer
    /// dagens rad, ellers opprettes ny — gamle dager måles mot målet som gjaldt da.
    /// </summary>
    Task SaveAsync(GoalSettings settings);
}
