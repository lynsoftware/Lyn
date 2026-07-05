using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calorie.Common.Components;

/// <summary>
/// Delt sideheader: valgfri logo (venstre), tittel (midt), valgfri
/// tilbake-knapp (høyre). Logo navigerer oss tilbake til forrige skjerm.
/// Klikk på Logo tar oss til hovedskjermen.
/// Plassering eies av siden; utseendet eies her.
/// </summary>
public partial class PageHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty);
    
    public static readonly BindableProperty ShowLogoProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty);
    
    public static readonly BindableProperty ShowCloseButtonProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty);
    
    public static readonly BindableProperty CloseCommandProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty);
    
    public event EventHandler? CloseClicked;
    
    public PageHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public string ShowLogo
    {
        get => (string)GetValue(ShowLogoProperty);
        set => SetValue(ShowLogoProperty, value);
    }
    
    public string ShowCloseButton
    {
        get => (string)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }
    
    public ICommand? CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        CloseClicked?.Invoke(this, EventArgs.Empty);

        if (CloseCommand?.CanExecute(null) == true)
            CloseCommand.Execute(null);
    }
}