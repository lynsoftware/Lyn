using System.Windows.Input;
using Calorie.Features.Navbar.Enums;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Navbar.Components;

public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab), typeof(NavTab), typeof(BottomNavBar), NavTab.Home,
        propertyChanged: (bindable, _, _) => ((BottomNavBar)bindable).UpdateTabColors());

    // Command + event side om side — samme mønster som PageHeader/DaySummaryCard
    public static readonly BindableProperty HomeCommandProperty =
        BindableProperty.Create(nameof(HomeCommand), typeof(ICommand), typeof(BottomNavBar));

    public static readonly BindableProperty MealsCommandProperty =
        BindableProperty.Create(nameof(MealsCommand), typeof(ICommand), typeof(BottomNavBar));

    public static readonly BindableProperty AddCommandProperty =
        BindableProperty.Create(nameof(AddCommand), typeof(ICommand), typeof(BottomNavBar));

    public static readonly BindableProperty StatsCommandProperty =
        BindableProperty.Create(nameof(StatsCommand), typeof(ICommand), typeof(BottomNavBar));

    public static readonly BindableProperty SettingsCommandProperty =
        BindableProperty.Create(nameof(SettingsCommand), typeof(ICommand), typeof(BottomNavBar));

    /// <summary>
    /// Hvilken fane som er aktiv — settes av siden som eier navbaren.
    /// </summary>
    public NavTab ActiveTab
    {
        get => (NavTab)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public ICommand? HomeCommand
    {
        get => (ICommand?)GetValue(HomeCommandProperty);
        set => SetValue(HomeCommandProperty, value);
    }

    public ICommand? MealsCommand
    {
        get => (ICommand?)GetValue(MealsCommandProperty);
        set => SetValue(MealsCommandProperty, value);
    }

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public ICommand? StatsCommand
    {
        get => (ICommand?)GetValue(StatsCommandProperty);
        set => SetValue(StatsCommandProperty, value);
    }

    public ICommand? SettingsCommand
    {
        get => (ICommand?)GetValue(SettingsCommandProperty);
        set => SetValue(SettingsCommandProperty, value);
    }

    public event EventHandler? HomeClicked;
    public event EventHandler? MealsClicked;
    public event EventHandler? AddClicked;
    public event EventHandler? StatsClicked;
    public event EventHandler? SettingsClicked;

    public BottomNavBar()
    {
        InitializeComponent();
        UpdateTabColors();
    }

    /// <summary>
    /// Aktiv fane får primærfargen, resten dempes. Temafarger slås opp ved
    /// hvert kall slik at temabytte slår gjennom ved neste oppdatering.
    /// </summary>
    private void UpdateTabColors()
    {
        var active = ThemeService.GetColor("Primary");
        var inactive = ThemeService.GetColor("NavInactive");

        HomeButton.TextColor = ActiveTab == NavTab.Home ? active : inactive;
        MealsButton.TextColor = ActiveTab == NavTab.Meals ? active : inactive;
        StatsButton.TextColor = ActiveTab == NavTab.Stats ? active : inactive;
        SettingsButton.TextColor = ActiveTab == NavTab.Settings ? active : inactive;
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        HomeClicked?.Invoke(this, e);
        ExecuteCommand(HomeCommand);
    }

    private void OnMealsClicked(object sender, EventArgs e)
    {
        MealsClicked?.Invoke(this, e);
        ExecuteCommand(MealsCommand);
    }

    private void OnAddClicked(object sender, EventArgs e)
    {
        AddClicked?.Invoke(this, e);
        ExecuteCommand(AddCommand);
    }

    private void OnStatsClicked(object sender, EventArgs e)
    {
        StatsClicked?.Invoke(this, e);
        ExecuteCommand(StatsCommand);
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        SettingsClicked?.Invoke(this, e);
        ExecuteCommand(SettingsCommand);
    }

    private static void ExecuteCommand(ICommand? command)
    {
        if (command?.CanExecute(null) == true)
            command.Execute(null);
    }
}
