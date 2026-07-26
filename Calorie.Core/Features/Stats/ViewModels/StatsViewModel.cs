using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Extensions;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Stats.Models;
using Calorie.Core.Features.Stats.Services;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.Stats.ViewModels;

/// <summary>
/// ViewModel for statistikken: ukessøyler, nøkkeltall og fordeling per
/// måltidstype — beregnet lokalt via StatsStore. Hver dag måles mot målet
/// som gjaldt DEN dagen (datert målhistorikk).
/// </summary>
public partial class StatsViewModel : ObservableObject
{
    // Søylefeltets høyde — må matche raden i XAML-malen (RowDefinitions="130,...")
    private const double BarZoneHeight = 130;

    private readonly INavigationService _navigation;
    private readonly IStatsStore _stats;
    private readonly TimeProvider _time;

    private DateOnly _weekReference;

    [ObservableProperty]
    private string _weekLabel = string.Empty;

    [ObservableProperty]
    private string _goalCaption = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextWeekCommand))]
    private bool _canGoNextWeek;

    [ObservableProperty]
    private List<DayBar> _dayBars = [];

    [ObservableProperty]
    private string _averageText = string.Empty;

    [ObservableProperty]
    private string _onTrackText = string.Empty;

    [ObservableProperty]
    private string _loggedDaysText = string.Empty;

    [ObservableProperty]
    private List<DistributionRow> _distributionRows = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasData))]
    private bool _isEmpty;

    public StatsViewModel(INavigationService navigation, IStatsStore statsStore, TimeProvider timeProvider)
    {
        _navigation = navigation;
        _stats = statsStore;
        _time = timeProvider;

        _weekReference = Today;
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    public bool HasData => !IsEmpty;

    // ===== Uke-navigasjon =====

    [RelayCommand]
    private async Task PreviousWeekAsync()
    {
        _weekReference = _weekReference.AddDays(-7);
        await LoadWeekAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNextWeek))]
    private async Task NextWeekAsync()
    {
        _weekReference = _weekReference.AddDays(7);
        await LoadWeekAsync();
    }

    // ===== Lasting =====

    [RelayCommand]
    private async Task LoadWeekAsync()
    {
        var stats = await _stats.GetWeekAsync(_weekReference);
        var today = Today;

        var weekEnd = stats.WeekStart.AddDays(6);
        WeekLabel = $"{stats.WeekStart.ToString("d. MMM", CultureInfo.CurrentCulture)} – {weekEnd.ToString("d. MMM", CultureInfo.CurrentCulture)}";
        CanGoNextWeek = weekEnd < today;

        // Dagens gjeldende mål som referanse i kortoverskriften
        var currentGoal = stats.Days.LastOrDefault(d => d.Date <= today) ?? stats.Days[^1];
        GoalCaption = $"{AppResources.GoalsTitle}: {currentGoal.GoalCalories} kcal";

        DayBars = BuildBars(stats);

        AverageText = stats.AverageCalories.ToString(CultureInfo.CurrentCulture);
        OnTrackText = $"{stats.DaysOnTrack} / {stats.LoggedDays}";
        LoggedDaysText = $"{stats.LoggedDays} / 7";

        DistributionRows = BuildDistribution(stats);

        IsEmpty = stats.LoggedDays == 0;
    }

    /// <summary>
    /// Skalerer mot største av (høyeste dag, høyeste mål) så målet alltid
    /// er i bildet. Gull = på mål, rød = sprakk, dempet stubb = ingen logg.
    /// </summary>
    private static List<DayBar> BuildBars(WeekStats stats)
    {
        var scaleMax = Math.Max(
            stats.Days.Max(d => d.Calories),
            stats.Days.Max(d => d.GoalCalories));
        if (scaleMax <= 0)
            scaleMax = 1;

        return stats.Days
            .Select(day =>
            {
                var barHeight = day.HasEntries ? Math.Max(BarZoneHeight * day.Calories / scaleMax, 6) : 4;

                // Kos-andelen av søylen — sunt nederst, kos øverst
                var treatHeight = day.HasEntries && day.Calories > 0
                    ? barHeight * day.TreatCalories / day.Calories
                    : 0;

                return new DayBar(
                    CaloriesText: day.HasEntries ? day.Calories.ToString(CultureInfo.CurrentCulture) : "–",
                    DayLetter: day.Date.ToString("ddd", CultureInfo.CurrentCulture)[..1].ToUpperInvariant(),
                    HealthyBarHeight: barHeight - treatHeight,
                    TreatBarHeight: treatHeight,
                    IsOnTrack: day.HasEntries && day.OnTrack,
                    IsOverLimit: day.HasEntries && !day.OnTrack);
            })
            .ToList();
    }

    private static List<DistributionRow> BuildDistribution(WeekStats stats)
    {
        var total = stats.TotalCalories;
        if (total <= 0)
            return [];

        return stats.MealTypeCalories
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp =>
            {
                var fraction = (double)kvp.Value / total;
                return new DistributionRow(
                    Label: kvp.Key.ToDisplayName(),
                    Fraction: fraction,
                    DetailText: $"{kvp.Value} kcal · {fraction:P0}",
                    IsTreat: kvp.Key == MealType.Treat);
            })
            .ToList();
    }

    // ===== Navbar =====

    [RelayCommand]
    private Task GoHomeAsync() => _navigation.GoHomeAsync();

    [RelayCommand]
    private Task OpenLibraryAsync() => _navigation.GoToLibraryAsync();

    [RelayCommand]
    private Task OpenLogAsync() => _navigation.GoToAddLogEntryAsync();

    [RelayCommand]
    private Task OpenSettingsAsync() => _navigation.GoToSettingsAsync();
}
