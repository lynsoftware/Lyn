namespace Calorie.Core.Features.DailyLog.ViewModels;

/// <summary>
/// Én stykk-snarvei ("1 stk" / "2 stk") for valgt vare — Label vises på
/// knappen, Grams er ferdig utregnet mengde.
/// </summary>
public sealed record UnitOption(string Label, decimal Grams);