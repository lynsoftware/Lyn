using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Calorie.Infrastructure.Services;

/// <summary>
/// Temabevisste toasts. MAUI har ingen innebygde — CommunityToolkit.Maui sine
/// Toast-er er native og kan ikke farges, så suksess vises som en grønn
/// Snackbar (Success/OnSuccess-tokens) uten handlingsknapp.
/// </summary>
public static class AppToast
{
    public static async Task SuccessAsync(string message)
    {
        var options = new SnackbarOptions
        {
            BackgroundColor = ThemeService.GetColor("Success"),
            TextColor = ThemeService.GetColor("OnSuccess"),
            CornerRadius = new CornerRadius(8)
        };

        await Snackbar.Make(message, duration: TimeSpan.FromSeconds(2), visualOptions: options).Show();
    }

    public static async Task ErrorAsync(string message)
    {
        var options = new SnackbarOptions
        {
            BackgroundColor = ThemeService.GetColor("OverLimit"),
            // Hvit fungerer på OverLimit-rødfargen i begge temaer (unntaksregelen)
            TextColor = Colors.White,
            CornerRadius = new CornerRadius(8)
        };

        // Feil får stå litt lenger enn suksess — brukeren skal rekke forklaringen
        await Snackbar.Make(message, duration: TimeSpan.FromSeconds(3), visualOptions: options).Show();
    }
}
