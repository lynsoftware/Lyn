using Calorie.Core.Common;
using Calorie.Core.Features.Goals;
using System.Globalization;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Goals.Pages;

/// <summary>
/// Mål-oppsett: dagsmål, Limit/Target, makromål og budsjett per måltidstype.
/// Skriver til GoalStore (in-memory i 4A) — dagsloggen speiler endringene
/// via DayLogStore.ApplyGoals ved neste visning.
/// </summary>
public partial class GoalSettingsPage : ContentPage
{
    private readonly Dictionary<MealType, Entry> _budgetEntries = new();
    private GoalMode _mode = GoalMode.Limit;
    private bool _loaded;

    public GoalSettingsPage()
    {
        InitializeComponent();
        UpdateModeButtons();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_loaded)
            return;
        _loaded = true;

        var goal = await GoalStore.GetCurrentAsync();
        _mode = goal.Mode;

        DailyCaloriesEntry.Text = goal.DailyCalories.ToString(CultureInfo.CurrentCulture);
        ProteinGoalEntry.Text = goal.ProteinGoalGrams?.ToString(CultureInfo.CurrentCulture);
        CarbsGoalEntry.Text = goal.CarbsGoalGrams?.ToString(CultureInfo.CurrentCulture);
        FatGoalEntry.Text = goal.FatGoalGrams?.ToString(CultureInfo.CurrentCulture);

        BuildBudgetRows(goal);
        UpdateModeButtons();
    }

    // ================== MODUS ==================

    private void OnLimitModeClicked(object sender, EventArgs e)
    {
        _mode = GoalMode.Limit;
        UpdateModeButtons();
    }

    private void OnTargetModeClicked(object sender, EventArgs e)
    {
        _mode = GoalMode.Target;
        UpdateModeButtons();
    }

    private void UpdateModeButtons()
    {
        var primary = ThemeService.GetColor("Primary");
        var onPrimary = ThemeService.GetColor("OnPrimary");
        var surface = ThemeService.GetColor("Surface");
        var secondary = ThemeService.GetColor("TextSecondary");

        var limitSelected = _mode == GoalMode.Limit;

        LimitModeButton.BackgroundColor = limitSelected ? primary : surface;
        LimitModeButton.TextColor = limitSelected ? onPrimary : secondary;
        TargetModeButton.BackgroundColor = limitSelected ? surface : primary;
        TargetModeButton.TextColor = limitSelected ? secondary : onPrimary;
    }

    // ================== BUDSJETTRADER ==================

    /// <summary>
    /// Én rad per måltidstype: navn + kcal-felt. Tomt felt = ikke noe budsjett.
    /// </summary>
    private void BuildBudgetRows(GoalSettings goal)
    {
        foreach (var mealType in Enum.GetValues<MealType>().OrderBy(t => t.DisplayOrder()))
        {
            var row = new Grid
            {
                ColumnSpacing = 10,
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = 110 }
                ]
            };

            row.Add(new Label
            {
                Text = mealType.ToDisplayName(),
                FontSize = 15,
                TextColor = ThemeService.GetColor("TextPrimary"),
                VerticalOptions = LayoutOptions.Center
            }, 0);

            var entry = new Entry
            {
                Keyboard = Keyboard.Numeric,
                FontSize = 15,
                TextColor = ThemeService.GetColor("TextPrimary"),
                BackgroundColor = Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.End,
                Text = goal.MealBudgets.TryGetValue(mealType, out var budget)
                    ? budget.ToString(CultureInfo.CurrentCulture)
                    : null
            };

            var border = new Border
            {
                BackgroundColor = ThemeService.GetColor("Surface"),
                StrokeThickness = 0,
                Padding = new Thickness(12, 0),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Content = entry
            };
            row.Add(border, 1);

            _budgetEntries[mealType] = entry;
            BudgetsContainer.Children.Add(row);
        }
    }

    // ================== LAGRE ==================

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var dailyCalories = ParseInt(DailyCaloriesEntry.Text);
        if (dailyCalories <= 0)
            return;

        var goal = new GoalSettings
        {
            DailyCalories = dailyCalories,
            Mode = _mode,
            ProteinGoalGrams = ParseOptionalInt(ProteinGoalEntry.Text),
            CarbsGoalGrams = ParseOptionalInt(CarbsGoalEntry.Text),
            FatGoalGrams = ParseOptionalInt(FatGoalEntry.Text)
        };

        foreach (var (mealType, entry) in _budgetEntries)
        {
            if (ParseOptionalInt(entry.Text) is { } budget && budget > 0)
                goal.MealBudgets[mealType] = budget;
        }

        await GoalStore.SaveAsync(goal);

        await AppToast.SuccessAsync(AppResources.Saved);
        await Navigation.PopAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private static int ParseInt(string? text) =>
        int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private static int? ParseOptionalInt(string? text)
    {
        var parsed = ParseInt(text);
        return parsed > 0 ? parsed : null;
    }
}
