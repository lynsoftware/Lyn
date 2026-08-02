using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Extensions;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.DailyLog;
using Calorie.Core.Features.DailyLog.Models;
using Calorie.Core.Features.DailyLog.Services;
using Calorie.Core.Features.DailyLog.ViewModels;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.MainPage.ViewModels;

/// <summary>
/// ViewModel for dagsloggen (hovedsiden). Lastes på nytt hver visning slik
/// at nye loggføringer dukker opp umiddelbart. Kun seksjoner med innhold
/// eller budsjett vises, i naturlig dagsrekkefølge. Trykk på en loggrad
/// åpner redigeringspanelet (endre mengde/måltidstype eller slette).
/// </summary>
public partial class MainPageViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IToastService _toast;
    private readonly IDayLogStore _dayLog;
    private readonly TimeProvider _time;

    private MealType _editMealType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDayEmpty))]
    private DayLog? _day;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDayEmpty))]
    private List<MealSection> _visibleSections = [];

    // Raden som redigeres — null betyr at panelet er skjult
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditPanelVisible), nameof(EditItemName), nameof(EditKcalPreview))]
    private LoggedItem? _editingItem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditKcalPreview))]
    private string _editAmountText = string.Empty;
    
    // Dagen som vises — hovedsiden lever på tvers av navigeringer, så valget består
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextDayCommand))]
    private DateOnly _currentDate;

    public MainPageViewModel(
        INavigationService navigation,
        IToastService toast,
        IDayLogStore dayLogStore,
        TimeProvider timeProvider)
    {
        _navigation = navigation;
        _toast = toast;
        _dayLog = dayLogStore;
        _time = timeProvider;

        _currentDate = Today;

        EditMealTypeOptions = Enum.GetValues<MealType>()
            .OrderBy(t => t.DisplayOrder())
            .Select(t => new MealTypeOption(t, t.ToDisplayName()))
            .ToList();
    }

    public IReadOnlyList<MealTypeOption> EditMealTypeOptions { get; }

    public bool IsEditPanelVisible => EditingItem != null;

    // Tomtilstand — kun etter at en dag faktisk er lastet, så meldingen
    // ikke blinker før første lasting ved oppstart
    public bool IsDayEmpty => Day != null && VisibleSections.Count == 0;

    public string EditItemName => EditingItem?.Name ?? string.Empty;

    /// <summary>
    /// Live kcal for ny mengde — snapshotet er lineært i gram, så
    /// kcal skaleres med ny/gammel mengde.
    /// </summary>
    public string EditKcalPreview
    {
        get
        {
            if (EditingItem is not { AmountGrams: > 0 } item)
                return string.Empty;

            var grams = ParseAmount();
            if (grams <= 0)
                return string.Empty;

            var calories = (int)Math.Round(item.Calories * grams / item.AmountGrams);
            return $"{calories} kcal";
        }
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    public bool CanGoNextDay => CurrentDate < Today;

    [RelayCommand]
    private async Task LoadDayAsync()
    {
        Day = await _dayLog.GetDayAsync(CurrentDate);

        VisibleSections = Day.Sections
            .Where(s => s.Items.Count > 0 || s.MaxCalories.HasValue)
            .OrderBy(s => s.MealType.DisplayOrder())
            .ToList();
    }

    [RelayCommand]
    private async Task PreviousDayAsync()
    {
        EditingItem = null;
        CurrentDate = CurrentDate.AddDays(-1);
        await LoadDayAsync();
    }
    
    [RelayCommand(CanExecute = nameof(CanGoNextDay))]
    private async Task NextDayAsync()
    {
        EditingItem = null;
        CurrentDate = CurrentDate.AddDays(1);
        await LoadDayAsync();
    }
    
    // Naviger til ønsket dato via DatePickeren
    [RelayCommand]
    private Task GoToDateAsync(DateOnly date) => ChangeDayAsync(date);

    private async Task ChangeDayAsync(DateOnly date)
    {
        EditingItem = null;
        CurrentDate = date;
        await LoadDayAsync();
    }

    /// <summary>
    /// Åpner logge-flyten for dagen som VISES — blar du til i går, logges det
    /// til i går (glemt nattmat-caset). Fra en seksjons legg-til følger
    /// seksjonens måltidstype med; fra navbarens pluss-knapp (null) foreslås
    /// typen ut fra klokkeslettet.
    /// </summary>
    [RelayCommand]
    private Task OpenLogFlowAsync(MealType? mealType) =>
        _navigation.GoToAddLogEntryAsync(mealType, CurrentDate);

    // ===== Redigering av loggrad =====

    [RelayCommand]
    private void EditEntry(LoggedItem item)
    {
        _editMealType = item.MealType;
        SyncEditMealTypeOptions();

        EditAmountText = item.AmountGrams.ToString("0.#", CultureInfo.CurrentCulture);
        EditingItem = item;
    }

    [RelayCommand]
    private void SelectEditMealType(MealTypeOption option)
    {
        _editMealType = option.Value;
        SyncEditMealTypeOptions();
    }

    [RelayCommand]
    private async Task SaveEntryAsync()
    {
        if (EditingItem is not { } item)
            return;

        var grams = ParseAmount();
        if (grams <= 0)
            return;

        await _dayLog.UpdateEntryAsync(item.Id, grams, _editMealType);

        EditingItem = null;
        await _toast.ShowSuccessAsync(AppResources.Saved);
        await LoadDayAsync();
    }

    [RelayCommand]
    private async Task DeleteEntryAsync()
    {
        if (EditingItem is not { } item)
            return;

        await _dayLog.DeleteEntryAsync(item.Id);

        EditingItem = null;
        await _toast.ShowSuccessAsync(AppResources.EntryDeleted);
        await LoadDayAsync();
    }

    [RelayCommand]
    private void CloseEdit() => EditingItem = null;

    private void SyncEditMealTypeOptions()
    {
        foreach (var option in EditMealTypeOptions)
            option.IsSelected = option.Value == _editMealType;
    }

    private decimal ParseAmount()
    {
        var text = EditAmountText.Replace(',', '.');
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var grams) && grams > 0
            ? grams
            : 0;
    }

    // ===== Navbar =====

    [RelayCommand]
    private Task OpenLibraryAsync() => _navigation.GoToLibraryAsync();

    [RelayCommand]
    private Task OpenStatsAsync() => _navigation.GoToStatsAsync();

    [RelayCommand]
    private Task OpenSettingsAsync() => _navigation.GoToSettingsAsync();
}
