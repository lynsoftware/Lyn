namespace Calorie.Core.Features.Stats.ViewModels;

/// <summary>
/// Én dagssøyle i ukesvisningen — ferdig beregnet for visning som to
/// stablede segmenter: sunt nederst, kos øverst (Treat-fargen).
/// Fargevalget for sunt-delen uttrykkes som flagg (gull = på mål,
/// rød = sprakk, begge false = dempet stubb uten logg) — XAML setter fargene.
/// </summary>
public sealed record DayBar(
    string CaloriesText,
    string DayLetter,
    double HealthyBarHeight,
    double TreatBarHeight,
    bool IsOnTrack,
    bool IsOverLimit);
