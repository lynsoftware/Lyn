namespace Calorie.Core.Features.Stats.ViewModels;

/// <summary>
/// Én rad i fordelingen per måltidstype: navn, andel (0–1) og detaljtekst.
/// Kos-raden flagges — den vises med Treat-fargen for kobling til dagsbaren.
/// </summary>
public sealed record DistributionRow(
    string Label,
    double Fraction,
    string DetailText,
    bool IsTreat);
