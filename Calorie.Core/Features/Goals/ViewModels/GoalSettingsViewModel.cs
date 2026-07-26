using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Extensions;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Goals.Models;
using Calorie.Core.Features.Goals.Services;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.Goals.ViewModels;

/// <summary>
/// ViewModel for mål-oppsettet: dagsmål, Limit/Target-modus, valgfrie
/// makromål og budsjett per måltidstype. Skriver til IGoalStore —
/// dagsloggen leser gjeldende mål på nytt ved neste visning.
/// </summary>
public partial class GoalSettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IToastService _toast;
    private readonly IGoalStore _goals;

    private bool _loaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLimitMode), nameof(IsTargetMode))]
    private GoalMode _mode = GoalMode.Limit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TreatKcalPreview))]
    private string _dailyCaloriesText = string.Empty;

    // Kos-andel i prosent av dagsmålet — tomt felt = featuren av
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TreatKcalPreview))]
    private string _treatPercentText = string.Empty;

    [ObservableProperty]
    private string _proteinText = string.Empty;

    [ObservableProperty]
    private string _carbsText = string.Empty;

    [ObservableProperty]
    private string _fatText = string.Empty;

    public IReadOnlyList<BudgetOption> BudgetOptions { get; }

    public GoalSettingsViewModel(INavigationService navigation, IToastService toast, IGoalStore goalStore)
    {
        _navigation = navigation;
        _toast = toast;
        _goals = goalStore;

        // Kos holdes utenfor kcal-budsjettene — kos-grensen settes som prosent
        BudgetOptions = Enum.GetValues<MealType>()
            .Where(t => t != MealType.Treat)
            .OrderBy(t => t.DisplayOrder())
            .Select(t => new BudgetOption(t, t.ToDisplayName()))
            .ToList();
    }

    /// <summary>Live omregning: "20 %" av dagsmålet → "= 400 kcal".</summary>
    public string TreatKcalPreview
    {
        get
        {
            var percent = ParseOptionalInt(TreatPercentText);
            var dailyCalories = ParseInt(DailyCaloriesText);
            if (percent is not { } p || p > 100 || dailyCalories <= 0)
                return string.Empty;

            return string.Format(AppResources.TreatShareCalculated, dailyCalories * p / 100);
        }
    }

    public bool IsLimitMode => Mode == GoalMode.Limit;

    public bool IsTargetMode => Mode == GoalMode.Target;

    // ===== Lasting =====

    /// <summary>
    /// Laster gjeldende mål — kun første gang (retur fra andre sider skal
    /// ikke overskrive det brukeren har tastet).
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_loaded)
            return;
        _loaded = true;

        var goal = await _goals.GetCurrentAsync();

        Mode = goal.Mode;
        DailyCaloriesText = goal.DailyCalories.ToString(CultureInfo.CurrentCulture);
        ProteinText = goal.ProteinGoalGrams?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        CarbsText = goal.CarbsGoalGrams?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        FatText = goal.FatGoalGrams?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        TreatPercentText = goal.TreatPercent?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;

        foreach (var option in BudgetOptions)
        {
            option.AmountText = goal.MealBudgets.TryGetValue(option.Value, out var budget)
                ? budget.ToString(CultureInfo.CurrentCulture)
                : string.Empty;
        }
    }

    // ===== Commands =====

    [RelayCommand]
    private void SelectLimitMode() => Mode = GoalMode.Limit;

    [RelayCommand]
    private void SelectTargetMode() => Mode = GoalMode.Target;

    [RelayCommand]
    private async Task SaveAsync()
    {
        var dailyCalories = ParseInt(DailyCaloriesText);
        if (dailyCalories <= 0)
            return;

        var goal = new GoalSettings
        {
            DailyCalories = dailyCalories,
            Mode = Mode,
            ProteinGoalGrams = ParseOptionalInt(ProteinText),
            CarbsGoalGrams = ParseOptionalInt(CarbsText),
            FatGoalGrams = ParseOptionalInt(FatText),
            // Over 100 % gir ikke mening — klamp i stedet for å avvise stille
            TreatPercent = ParseOptionalInt(TreatPercentText) is { } percent
                ? Math.Min(percent, 100)
                : null
        };

        foreach (var option in BudgetOptions)
        {
            if (ParseOptionalInt(option.AmountText) is { } budget && budget > 0)
                goal.MealBudgets[option.Value] = budget;
        }

        await _goals.SaveAsync(goal);

        await _toast.ShowSuccessAsync(AppResources.Saved);
        await _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();

    private static int ParseInt(string text) =>
        int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private static int? ParseOptionalInt(string text)
    {
        var parsed = ParseInt(text);
        return parsed > 0 ? parsed : null;
    }
}
