using Calorie.Core.Features.DailyLog;
using Calorie.Core.Common;
using System.Globalization;
using System.Windows.Input;
using Calorie.Core.Common.Enums;
using Calorie.Core.Features.DailyLog.Models;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.DailyLog.Components;

public partial class DaySummaryCard : ContentView
{
    /// <summary>Dagen kortet viser — bindes fra sidens ViewModel.</summary>
    public static readonly BindableProperty DayProperty = BindableProperty.Create(
        nameof(Day), typeof(DayLog), typeof(DaySummaryCard),
        propertyChanged: (bindable, _, newValue) =>
        {
            if (newValue is DayLog day)
                ((DaySummaryCard)bindable).Update(day);
        });
    
    public static readonly BindableProperty PreviousDayCommandProperty = 
        BindableProperty.Create(nameof(PreviousDayCommand), typeof(ICommand), typeof(DaySummaryCard));
        
    public static readonly BindableProperty NextDayCommandProperty = 
        BindableProperty.Create(nameof(NextDayCommand), typeof(ICommand), typeof(DaySummaryCard));
    
    /// <summary>Kjøres ved datovalg (kalender eller I dag-pillen) — får DateOnly som parameter.</summary>
    public static readonly BindableProperty SelectDateCommandProperty =
        BindableProperty.Create(nameof(SelectDateCommand), typeof(ICommand), typeof(DaySummaryCard));

    public event EventHandler? PreviousDayClicked;
    public event EventHandler? NextDayClicked;

    public DaySummaryCard()
    {
        InitializeComponent();
    }

    public DayLog? Day
    {
        get => (DayLog?)GetValue(DayProperty);
        set => SetValue(DayProperty, value);
    }

    public ICommand? PreviousDayCommand
    {
        get => (ICommand?)GetValue(PreviousDayCommandProperty);
        set => SetValue(PreviousDayCommandProperty, value);
    }
    
    public ICommand? NextDayCommand
    {
        get => (ICommand?)GetValue(NextDayCommandProperty);
        set => SetValue(NextDayCommandProperty, value);
    }
    
    public ICommand? SelectDateCommand
    {
        get => (ICommand?)GetValue(SelectDateCommandProperty);
        set => SetValue(SelectDateCommandProperty, value);
    }

    /// <summary>
    /// Oppdaterer kortet med en dags data. Limit-modus: rødt ved overskridelse.
    /// Target-modus: gull når målet er nådd — å nå det er suksess.
    /// </summary>
    private void Update(DayLog day)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var isToday = day.Date == today;

        DateLabel.Text = isToday
            ? AppResources.Today
            : day.Date.ToString("dddd d. MMMM", CultureInfo.CurrentCulture);

        TodayButton.IsEnabled = !isToday;
        TodayButton.Opacity = isToday ? 0.4 : 1;

        TotalsLabel.Text = $"{day.TotalEatenCalories} / {day.GoalCalories} kcal";

        var overLimit = day.Mode == GoalMode.Limit && day.RemainingCalories < 0;

        UpdateProgressSegments(day, overLimit);
        UpdateTreatLine(day);

        RemainingLabel.Text = $"{day.RemainingCalories} kcal {AppResources.Remaining}";
        RemainingLabel.TextColor = overLimit
            ? ThemeService.GetColor("OverLimit")
            : ThemeService.GetColor("TextSecondary");

        ProteinLabel.Text = FormatMacro(AppResources.ProteinShort, day.ProteinGrams, day.ProteinGoalGrams);
        CarbsLabel.Text = FormatMacro(AppResources.CarbsShort, day.CarbsGrams, day.CarbsGoalGrams);
        FatLabel.Text = FormatMacro(AppResources.FatShort, day.FatGrams, day.FatGoalGrams);
        
        HiddenDatePicker.MaximumDate = DateTime.Today;
        HiddenDatePicker.Date = day.Date.ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Segmentert fremdriftsbar: sunt (Primary) + kos (Treat) — kos skilles
    /// alltid ut, som i statistikk-søylene. Med kos-andel satt for dagen
    /// splittes kos videre i innenfor (Treat) og over (TreatOver). Ved
    /// dagsoverskridelse i Limit-modus vises sunt-delen i OverLimit — kos
    /// beholder sine farger.
    /// Bredder = andel av dagsmålet; over målet skaleres alt så baren er full.
    /// </summary>
    private void UpdateProgressSegments(DayLog day, bool overLimitDay)
    {
        ProgressSegments.Children.Clear();
        ProgressSegments.ColumnDefinitions.Clear();

        if (day.GoalCalories <= 0 || day.TotalEatenCalories <= 0)
            return;

        // Kos skilles ALLTID ut visuelt (som i statistikk-søylene) — andelen
        // styrer kun regnskapet: uten andel for dagen finnes ingen "over"-del
        decimal healthy = day.TotalEatenCalories - day.TreatCalories;
        decimal treatWithin, treatOver;

        if (day.TreatAllowanceCalories is { } allowance)
        {
            treatWithin = Math.Min(day.TreatCalories, allowance);
            treatOver = day.TreatCalories - treatWithin;
        }
        else
        {
            treatWithin = day.TreatCalories;
            treatOver = 0;
        }

        decimal goal = day.GoalCalories;
        decimal total = day.TotalEatenCalories;
        var scale = total > goal ? goal / total : 1m;

        var healthyFraction = (double)(healthy * scale / goal);
        var withinFraction = (double)(treatWithin * scale / goal);
        var overFraction = (double)(treatOver * scale / goal);
        var restFraction = Math.Max(0, 1 - healthyFraction - withinFraction - overFraction);

        AddSegment(healthyFraction, overLimitDay ? "OverLimit" : "Primary");
        AddSegment(withinFraction, "Treat");
        AddSegment(overFraction, "TreatOver");
        AddSegment(restFraction, null);   // tom rest — sporet skinner gjennom
    }

    private void AddSegment(double fraction, string? colorToken)
    {
        if (fraction <= 0)
            return;

        ProgressSegments.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(fraction, GridUnitType.Star) });

        ProgressSegments.Add(new BoxView
        {
            Color = colorToken == null ? Colors.Transparent : ThemeService.GetColor(colorToken)
        }, ProgressSegments.ColumnDefinitions.Count - 1, 0);
    }

    /// <summary>
    /// "400 kcal kos igjen" under igjen-teksten — kun når kos-andel er satt.
    /// Over grensen: eget uttrykk i TreatOver-fargen.
    /// </summary>
    private void UpdateTreatLine(DayLog day)
    {
        if (day.TreatRemainingCalories is not { } remaining)
        {
            TreatRemainingLabel.IsVisible = false;
            return;
        }

        TreatRemainingLabel.IsVisible = true;

        if (remaining >= 0)
        {
            TreatRemainingLabel.Text = string.Format(AppResources.TreatRemainingFormat, remaining);
            TreatRemainingLabel.TextColor = ThemeService.GetColor("TextSecondary");
        }
        else
        {
            TreatRemainingLabel.Text = string.Format(AppResources.TreatOverFormat, -remaining);
            TreatRemainingLabel.TextColor = ThemeService.GetColor("TreatOver");
        }
    }

    // "P 61 g" uten mål, "P 61 / 120 g" med mål
    private static string FormatMacro(string prefix, int eaten, int? goal) =>
        goal is { } target ? $"{prefix} {eaten} / {target} g" : $"{prefix} {eaten} g";

    private void OnPreviousDayClicked(object sender, EventArgs e)
    {
        PreviousDayClicked?.Invoke(this, e);

        if (PreviousDayCommand?.CanExecute(null) == true)
            PreviousDayCommand.Execute(null);
    }

    private void OnNextDayClicked(object sender, EventArgs e)
    {
        NextDayClicked?.Invoke(this, e);
        
        if (NextDayCommand?.CanExecute(null) == true)
            NextDayCommand.Execute(null);
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        if (e.NewDate is not { } selected)
            return;

        var date = DateOnly.FromDateTime(selected);
        if (Day?.Date == date)
            return;   // programmatisk synk fra Update — ikke et brukervalg

        ExecuteSelectDate(date);
    }

    private void OnTodayClicked(object sender, EventArgs e) => ExecuteSelectDate(DateOnly.FromDateTime(DateTime.Now));

    private void ExecuteSelectDate(DateOnly date)
    {
        if (SelectDateCommand?.CanExecute(date) == true)
            SelectDateCommand.Execute(date);
    }
}
