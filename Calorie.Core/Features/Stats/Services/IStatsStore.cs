using Calorie.Core.Features.Stats.Models;

namespace Calorie.Core.Features.Stats.Services;

/// <summary>
/// Statistikk sett fra ViewModels. Implementeres av StatsStore
/// (lokal SQLite); testene mocker den.
/// </summary>
public interface IStatsStore
{
    /// <summary>Uken som inneholder referansedatoen (mandag som ukestart).</summary>
    Task<WeekStats> GetWeekAsync(DateOnly reference);
}
