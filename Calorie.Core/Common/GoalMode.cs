namespace Calorie.Core.Common;

/// <summary>
/// Hvordan kalorimålet tolkes. MÅ speile GoalMode-enumen i backend
/// (Lyn.Backend.Apps.Calorie.Enums.GoalMode) — del av sync-kontrakten.
/// </summary>
public enum GoalMode
{
    // Maksgrense — ikke overskrid
    Limit = 0,

    // Mål å nå — spis minst dette
    Target = 1
}
