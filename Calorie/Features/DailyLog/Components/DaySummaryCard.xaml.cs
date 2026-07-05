using Calorie.Core.Features.DailyLog;
using Calorie.Core.Common;
using System.Globalization;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.DailyLog.Components;

public partial class DaySummaryCard : ContentView
{
    public event EventHandler? PreviousDayClicked;
    public event EventHandler? NextDayClicked;

    public DaySummaryCard()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Oppdaterer kortet med en dags data. Limit-modus: rødt ved overskridelse.
    /// Target-modus: gull når målet er nådd — å nå det er suksess.
    /// </summary>
    public void Update(DayLog day)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        DateLabel.Text = day.Date == today
            ? AppResources.Today
            : day.Date.ToString("dddd d. MMMM", CultureInfo.CurrentCulture);

        TotalsLabel.Text = $"{day.TotalEatenCalories} / {day.GoalCalories} kcal";

        var progress = day.GoalCalories > 0 ? (double)day.TotalEatenCalories / day.GoalCalories : 0;
        GoalProgress.Progress = Math.Min(progress, 1.0);

        var overLimit = day.Mode == GoalMode.Limit && day.RemainingCalories < 0;
        GoalProgress.ProgressColor = overLimit
            ? ThemeService.GetColor("OverLimit")
            : ThemeService.GetColor("Primary");

        RemainingLabel.Text = $"{day.RemainingCalories} kcal {AppResources.Remaining}";
        RemainingLabel.TextColor = overLimit
            ? ThemeService.GetColor("OverLimit")
            : ThemeService.GetColor("TextSecondary");

        ProteinLabel.Text = FormatMacro(AppResources.ProteinShort, day.ProteinGrams, day.ProteinGoalGrams);
        CarbsLabel.Text = FormatMacro(AppResources.CarbsShort, day.CarbsGrams, day.CarbsGoalGrams);
        FatLabel.Text = FormatMacro(AppResources.FatShort, day.FatGrams, day.FatGoalGrams);
    }

    // "P 61 g" uten mål, "P 61 / 120 g" med mål
    private static string FormatMacro(string prefix, int eaten, int? goal) =>
        goal is { } target ? $"{prefix} {eaten} / {target} g" : $"{prefix} {eaten} g";

    private void OnPreviousDayClicked(object sender, EventArgs e) => PreviousDayClicked?.Invoke(this, e);
    private void OnNextDayClicked(object sender, EventArgs e) => NextDayClicked?.Invoke(this, e);
}
