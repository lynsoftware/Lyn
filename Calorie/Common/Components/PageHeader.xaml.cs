using System.Windows.Input;

namespace Calorie.Common.Components;

/// <summary>
/// Delt sideheader: valgfri logo (venstre), tittel (midt), valgfri
/// lukk-knapp (høyre) som kjører CloseCommand.
/// Plassering eies av siden; utseendet eies her.
/// </summary>
public partial class PageHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty);
    
    public static readonly BindableProperty ShowLogoProperty =
        BindableProperty.Create(nameof(ShowLogo), typeof(bool), typeof(PageHeader), false);
    
    public static readonly BindableProperty ShowCloseButtonProperty =
        BindableProperty.Create(nameof(ShowCloseButton), typeof(bool), typeof(PageHeader), false);
    
    public static readonly BindableProperty CloseCommandProperty =
        BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(PageHeader));
    
    public PageHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public bool ShowLogo
    {
        get => (bool)GetValue(ShowLogoProperty);
        set => SetValue(ShowLogoProperty, value);
    }
    
    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }
    
    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}