using Calorie.Core.Common;
using Calorie.Core.Features.DailyLog;
using Calorie.Features.DailyLog.Components;
using Calorie.Features.Library.Pages;
using Calorie.Features.Settings.Pages;
using Calorie.Features.Stats.Pages;

namespace Calorie.Features.DailyLog.Pages;

/// <summary>
/// Hovedsiden — dagsloggen. Leser fra DayLogStore (in-memory i Fase 4A,
/// SQLite i 4B) og bygges på nytt hver gang siden vises, slik at nye
/// loggføringer dukker opp umiddelbart.
/// </summary>
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDayAsync();
    }

    /// <summary>
    /// Bygger dagsvisningen: sammendragskort øverst, deretter én seksjon per
    /// måltidstype som har budsjett eller innhold, i naturlig dagsrekkefølge.
    /// </summary>
    private async Task LoadDayAsync()
    {
        var day = await DayLogStore.GetTodayAsync();

        DayContainer.Children.Clear();

        var summary = new DaySummaryCard();
        summary.Update(day);
        DayContainer.Children.Add(summary);

        var visibleSections = day.Sections
            .Where(s => s.Items.Count > 0 || s.MaxCalories.HasValue)
            .OrderBy(s => s.MealType.DisplayOrder());

        foreach (var section in visibleSections)
        {
            var card = new MealSectionCard();
            card.SetSection(section);

            // Seksjonens legg-til forhåndsvelger seksjonens måltidstype
            var mealType = section.MealType;
            card.AddClicked += async (_, _) => await OpenLogFlowAsync(mealType);

            DayContainer.Children.Add(card);
        }
    }

    /// <summary>
    /// Pluss-knappen i navbaren — måltidstype foreslås ut fra klokkeslettet.
    /// </summary>
    private async void OnAddClicked(object? sender, EventArgs e) => await OpenLogFlowAsync(null);

    private async Task OpenLogFlowAsync(MealType? mealType) =>
        await Navigation.PushAsync(new AddLogEntryPage(mealType));

    /// <summary>
    /// Åpner biblioteket (måltider og ingredienser).
    /// </summary>
    private async void OnMealsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new LibraryPage());

    /// <summary>
    /// Åpner statistikken.
    /// </summary>
    private async void OnStatsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new StatsPage());

    /// <summary>
    /// Opens the settings page.
    /// </summary>
    private async void OnSettingsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new SettingsPage());
}
