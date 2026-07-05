using Calorie.Features.Navbar.Enums;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Navbar.Components;

public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab), typeof(NavTab), typeof(BottomNavBar), NavTab.Home,
        propertyChanged: (bindable, _, _) => ((BottomNavBar)bindable).UpdateTabColors());

    /// <summary>
    /// Hvilken fane som er aktiv — settes av siden som eier navbaren.
    /// </summary>
    public NavTab ActiveTab
    {
        get => (NavTab)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
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

    private void OnHomeClicked(object sender, EventArgs e) => HomeClicked?.Invoke(this, e);
    private void OnMealsClicked(object sender, EventArgs e) => MealsClicked?.Invoke(this, e);
    private void OnAddClicked(object sender, EventArgs e) => AddClicked?.Invoke(this, e);
    private void OnStatsClicked(object sender, EventArgs e) => StatsClicked?.Invoke(this, e);
    private void OnSettingsClicked(object sender, EventArgs e) => SettingsClicked?.Invoke(this, e);
}
