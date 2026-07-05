using Calorie.Core.Common;
using Calorie.Features.DailyLog.Pages;
using Calorie.Features.Library.Pages;
using Calorie.Features.Settings.Pages;
using System.Globalization;
using Calorie.Core.Features.Stats;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Stats.Pages;

/// <summary>
/// Statistikk: ukessøyler, nøkkeltall og fordeling per måltidstype —
/// beregnet lokalt fra SQLite. Hver dag farges mot målet som gjaldt
/// den dagen (datert målhistorikk).
/// </summary>
public partial class StatsPage : ContentPage
{
    private const double BarZoneHeight = 130;

    private DateOnly _weekReference = DateOnly.FromDateTime(DateTime.Now);

    public StatsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWeekAsync();
    }

    // ================== UKE-NAVIGASJON ==================

    private async void OnPreviousWeekClicked(object sender, EventArgs e)
    {
        _weekReference = _weekReference.AddDays(-7);
        await LoadWeekAsync();
    }

    private async void OnNextWeekClicked(object sender, EventArgs e)
    {
        var next = _weekReference.AddDays(7);
        if (next > DateOnly.FromDateTime(DateTime.Now).AddDays(6))
            return;

        _weekReference = next;
        await LoadWeekAsync();
    }

    // ================== LASTING ==================

    private async Task LoadWeekAsync()
    {
        var stats = await StatsStore.GetWeekAsync(_weekReference);
        var today = DateOnly.FromDateTime(DateTime.Now);

        var weekEnd = stats.WeekStart.AddDays(6);
        WeekLabel.Text = $"{stats.WeekStart.ToString("d. MMM", CultureInfo.CurrentCulture)} – {weekEnd.ToString("d. MMM", CultureInfo.CurrentCulture)}";
        NextWeekButton.IsEnabled = weekEnd < today;

        // Dagens gjeldende mål som referanse i kortoverskriften
        var currentGoal = stats.Days.LastOrDefault(d => d.Date <= today) ?? stats.Days[^1];
        GoalCaptionLabel.Text = $"{AppResources.GoalsTitle}: {currentGoal.GoalCalories} kcal";

        BuildBars(stats);

        AverageValueLabel.Text = stats.AverageCalories.ToString(CultureInfo.CurrentCulture);
        OnTrackValueLabel.Text = $"{stats.DaysOnTrack} / {stats.LoggedDays}";
        LoggedDaysValueLabel.Text = $"{stats.LoggedDays} / 7";

        BuildDistribution(stats);

        EmptyLabel.IsVisible = stats.LoggedDays == 0;
        DistributionCard.IsVisible = stats.LoggedDays > 0;
    }

    // ================== SØYLER ==================

    private void BuildBars(WeekStats stats)
    {
        BarsGrid.Children.Clear();
        BarsGrid.ColumnDefinitions.Clear();

        // Skalér mot største av (høyeste dag, høyeste mål) så målet alltid er i bildet
        var scaleMax = Math.Max(
            stats.Days.Max(d => d.Calories),
            stats.Days.Max(d => d.GoalCalories));
        if (scaleMax <= 0)
            scaleMax = 1;

        for (var i = 0; i < stats.Days.Count; i++)
        {
            BarsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            BarsGrid.Add(CreateBarColumn(stats.Days[i], scaleMax), i);
        }
    }

    /// <summary>
    /// Én dagskolonne: søyle (bunnjustert), kcal-tall og ukedagsbokstav.
    /// Gull = på mål, rød = sprakk, dempet stubb = ingen logg.
    /// </summary>
    private View CreateBarColumn(DayStat day, int scaleMax)
    {
        var column = new Grid
        {
            RowSpacing = 3,
            RowDefinitions =
            [
                new RowDefinition { Height = BarZoneHeight },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            ]
        };

        var barColor = !day.HasEntries
            ? ThemeService.GetColor("Separator")
            : day.OnTrack
                ? ThemeService.GetColor("Primary")
                : ThemeService.GetColor("OverLimit");

        var barHeight = day.HasEntries
            ? Math.Max(BarZoneHeight * day.Calories / scaleMax, 6)
            : 4;

        column.Add(new BoxView
        {
            Color = barColor,
            CornerRadius = 3,
            HeightRequest = barHeight,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill
        }, 0, 0);

        column.Add(new Label
        {
            Text = day.HasEntries ? day.Calories.ToString(CultureInfo.CurrentCulture) : "–",
            FontSize = 10,
            TextColor = ThemeService.GetColor("TextMuted"),
            HorizontalOptions = LayoutOptions.Center
        }, 0, 1);

        column.Add(new Label
        {
            // Første bokstav i ukedagen, på gjeldende språk
            Text = day.Date.ToString("ddd", CultureInfo.CurrentCulture)[..1].ToUpperInvariant(),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = ThemeService.GetColor("TextSecondary"),
            HorizontalOptions = LayoutOptions.Center
        }, 0, 2);

        return column;
    }

    // ================== FORDELING ==================

    private void BuildDistribution(WeekStats stats)
    {
        DistributionContainer.Children.Clear();

        var total = stats.TotalCalories;
        if (total <= 0)
            return;

        var rows = stats.MealTypeCalories
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => (kvp.Key, kvp.Value));

        foreach (var (mealType, calories) in rows)
        {
            var fraction = (double)calories / total;

            var row = new Grid
            {
                ColumnSpacing = 10,
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = 110 },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                ]
            };

            row.Add(new Label
            {
                Text = mealType.ToDisplayName(),
                FontSize = 13,
                TextColor = ThemeService.GetColor("TextPrimary"),
                VerticalOptions = LayoutOptions.Center
            }, 0);

            row.Add(new ProgressBar
            {
                Progress = fraction,
                ProgressColor = ThemeService.GetColor("Primary"),
                BackgroundColor = ThemeService.GetColor("Separator"),
                VerticalOptions = LayoutOptions.Center
            }, 1);

            row.Add(new Label
            {
                Text = $"{calories} kcal · {fraction:P0}",
                FontSize = 12,
                TextColor = ThemeService.GetColor("TextSecondary"),
                VerticalOptions = LayoutOptions.Center
            }, 2);

            DistributionContainer.Children.Add(row);
        }
    }

    // ================== NAVBAR ==================

    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    private async void OnMealsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new LibraryPage());

    private async void OnLogClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AddLogEntryPage());

    private async void OnSettingsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new SettingsPage());
}
