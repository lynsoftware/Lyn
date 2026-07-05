namespace Lyn.Backend.Apps.Calorie.Enums;

/// <summary>
/// Hvordan brukerens kalorimål skal tolkes. Ren tolkningsbryter for
/// dashboard og varsler — beregningene er identiske, bare feedbacken snur.
/// </summary>
public enum GoalMode
{
    // Maksgrense — ikke overskrid (underskudd/vedlikehold)
    Limit = 0,

    // Mål å nå — spis minst dette (bulk, appetittproblemer)
    Target = 1
}
