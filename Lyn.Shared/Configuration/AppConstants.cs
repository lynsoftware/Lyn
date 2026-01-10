namespace Lyn.Shared.Configuration;

/// <summary>
/// Constants that are reused in several components
/// </summary>
public static class AppConstants
{
    // Passord-relaterte innstillinger
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 256;
    public const int PasswordDefaultLength = 16;
    public const bool PasswordDefaultIncludeSpecialChars = true;

    public const string AlphanumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public const string SpecialChars = "!@#$%&*-_+=";

    // Preference keys
    public const string PreferenceKeyPasswordLength = "PasswordLength";
    public const string PreferenceKeyIncludeSpecialChars = "IncludeSpecialChars";
    public const string PreferenceKeyPasswordVisible = "PasswordVisible";
    public const string PreferenceKeyAppLanguage = "AppLanguage";
    public const string PreferenceKeyAppTheme = "AppTheme";
}
