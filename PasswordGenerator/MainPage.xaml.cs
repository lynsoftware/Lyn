using PasswordGenerator.Configuration;
using PasswordGenerator.Models;
using PasswordGenerator.Pages;
using PasswordGenerator.Services;
namespace PasswordGenerator;

public partial class MainPage : ContentPage
{
    private readonly IPasswordService _passwordService;
    private string _generatedPassword = string.Empty;
    
    public MainPage(IPasswordService passwordService)
    {
        InitializeComponent();
        _passwordService = passwordService;
        
        LengthSlider.Minimum = AppConstants.PasswordMinLength;
        LengthSlider.Maximum = AppConstants.PasswordMaxLength;
        LengthSlider.Value = AppConstants.PasswordDefaultLength;
        
        LoadPreferences();
        _lastSliderValue = (int)LengthSlider.Value;
    }
    
    // Slider-related fields
    private int _lastSliderValue;
    private DateTime _lastSliderUpdate = DateTime.MinValue;
    private const int SliderUpdateDelayMs = 50;
    
    /// <summary>
    /// Toggles password visibility and persists the user's preference for the next session.
    /// Updates the eye icon to reflect the current visibility state.
    /// </summary>
    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        MasterPasswordEntry.IsPassword = !MasterPasswordEntry.IsPassword;
        TogglePasswordButton.Text =
            MasterPasswordEntry.IsPassword ? "\uf06e" : "\uf070";

        Preferences.Set(AppConstants.PreferenceKeyPasswordVisible, !MasterPasswordEntry.IsPassword);
    }
    
    /// <summary>
    /// Synchronizes the length input field with the slider value changes.
    /// Implements throttling to reduce CPU usage during rapid slider movements.
    /// </summary>
    private void OnLengthSliderChanged(object sender, ValueChangedEventArgs e)
    {   
        int newValue = (int)e.NewValue;

        if (newValue == _lastSliderValue)
            return;

        var now = DateTime.Now;
        if ((now - _lastSliderUpdate).TotalMilliseconds < SliderUpdateDelayMs)
            return;
        
        _lastSliderValue = newValue;
        _lastSliderUpdate = now;
        LengthEntry.Text = newValue.ToString();
    }
    
    /// <summary>
    /// Updates the slider in real-time as the user types in the length entry field.
    /// Clamps the value within the allowed min/max range.
    /// </summary>
    private void OnLengthEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(LengthEntry.Text, out int value))
        {
            int clampedValue = Math.Clamp(value, (int)LengthSlider.Minimum, (int)LengthSlider.Maximum);
            LengthSlider.Value = clampedValue;
        }
    }

    /// <summary>
    /// Updates both the slider and input field when the user completes editing.
    /// Ensures the displayed value is properly formatted.
    /// </summary>
    private void OnLengthEntryCompleted(object sender, EventArgs e)
    {
        if (int.TryParse(LengthEntry.Text, out int value))
        {
            int clampedValue = Math.Clamp(value, (int)LengthSlider.Minimum, (int)LengthSlider.Maximum);
            LengthSlider.Value = clampedValue;
            LengthEntry.Text = clampedValue.ToString();
        }
        else
            LengthEntry.Text = ((int)LengthSlider.Value).ToString();
    }

    /// <summary>
    /// Generates a password using the Argon2id algorithm with the provided parameters.
    /// Validates input fields, displays appropriate error messages, triggers button shake animation,
    /// and persists the generation settings for future use.
    /// </summary>
    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        try
        {
            ResetMasterPasswordErrors();
            ResetSeedErrors();

            var request = new PasswordGenerationRequest
            {
                MasterPassword = MasterPasswordEntry.Text,
                Seed = SeedEntry.Text, 
                Length = (int)LengthSlider.Value,
                IncludeSpecialChars = SpecialCharsCheckBox.IsChecked
            };
            
            if (string.IsNullOrWhiteSpace(request.MasterPassword))
            {
                MasterPasswordError.IsVisible = true;
                MasterPasswordEntry.BackgroundColor = Color.FromRgba(255, 0, 0, 30);
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Seed))
            {
                SeedError.IsVisible = true;
                SeedEntry.BackgroundColor = Color.FromRgba(255, 0, 0, 30);
                return;
            }

            var button = sender as Border;

            var cts = new CancellationTokenSource();
            var startShakingTask = ContinuousShakeAsync(button, cts.Token);

            try
            {
                var result = await _passwordService.GeneratePasswordAsync(request);

                _generatedPassword = result.Value;

                ResultLabel.Text = _generatedPassword;
                CopyButtonBorder.IsVisible = true;
                PasswordDisplayBox.IsVisible = true;

                Preferences.Set(AppConstants.PreferenceKeyPasswordLength, request.Length);
                Preferences.Set(AppConstants.PreferenceKeyIncludeSpecialChars, request.IncludeSpecialChars);
            }
            finally
            {
                cts.Cancel();
                await startShakingTask;

                if (button != null)
                    await button.TranslateToAsync(0, 0, 100);
            }
            
            
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to generate password: {ex.Message}", "OK");
        }
    }
    
    /// <summary>
    /// Creates a continuous shake animation effect that runs until cancelled.
    /// Provides visual feedback as a loading indicator while password is being generated.
    /// </summary>
    private async Task ContinuousShakeAsync(Border? button, CancellationToken cancellationToken)
    {
        if (button == null) return;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await button.TranslateToAsync(-10, 0, 50);
                if (cancellationToken.IsCancellationRequested) break;

                await button.TranslateToAsync(10, 0, 50);
                if (cancellationToken.IsCancellationRequested) break;

                await button.TranslateToAsync(-10, 0, 50);
                if (cancellationToken.IsCancellationRequested) break;

                await button.TranslateToAsync(10, 0, 50);
                if (cancellationToken.IsCancellationRequested) break;
            }
        }
        catch (TaskCanceledException)
        {
            // Exception when animation is cancelled
        }
    }
    
    /// <summary>
    /// Copies the generated password to the clipboard and provides temporary visual feedback.
    /// Disables the button temporarily and shows a checkmark to confirm the action.
    /// </summary>
    private async void OnCopyClicked(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_generatedPassword))
            {
                await Clipboard.SetTextAsync(_generatedPassword);

                string originalText = CopyButton.Text;
                CopyButton.Text = "✓ Copied!";
                CopyButton.IsEnabled = false;
                
                await Task.Delay(1500);
                
                CopyButton.Text = originalText;
                CopyButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to copy password: {ex.Message}", "OK");
        }
    }
    
    /// <summary>
    /// Clears the master password error state when the user starts typing.
    /// </summary>
    private void OnMasterPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.NewTextValue))
            ResetMasterPasswordErrors();
    }
    
    /// <summary>
    /// Clears the seed error state when the user starts typing.
    /// </summary>
    private void OnSeedTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.NewTextValue))
            ResetSeedErrors();
    }
    
    /// <summary>
    /// Resets the master password error message and background color to their default states.
    /// </summary>
    private void ResetMasterPasswordErrors()
    {
        MasterPasswordError.IsVisible = false;
        MasterPasswordEntry.BackgroundColor = Colors.Transparent;
    }
    
    /// <summary>
    /// Resets the seed error message and background color to their default states.
    /// </summary>
    private void ResetSeedErrors()
    {
        SeedError.IsVisible = false;
        SeedEntry.BackgroundColor = Colors.Transparent;
    }
    
    /// <summary>
    /// Loads user preferences from local storage and applies them to the UI controls.
    /// Restores password visibility state, length settings, and special character inclusion preference.
    /// </summary>
    private void LoadPreferences()
    {
        bool passwordVisible = Preferences.Get(AppConstants.PreferenceKeyPasswordVisible, false);
        MasterPasswordEntry.IsPassword = !passwordVisible;
        TogglePasswordButton.Text = MasterPasswordEntry.IsPassword ? "\uf06e" : "\uf070";

        int savedLength = Preferences.Get(AppConstants.PreferenceKeyPasswordLength, 
            AppConstants.PasswordDefaultLength);
        LengthSlider.Value = savedLength;
        LengthEntry.Text = savedLength.ToString();

        bool includeSpecialChars = Preferences.Get(AppConstants.PreferenceKeyIncludeSpecialChars, 
            AppConstants.PasswordDefaultIncludeSpecialChars);
        SpecialCharsCheckBox.IsChecked = includeSpecialChars;
    }
    
    /// <summary>
    /// Navigates to the settings page.
    /// </summary>
    private async void OnMenuClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SettingsPage());
}