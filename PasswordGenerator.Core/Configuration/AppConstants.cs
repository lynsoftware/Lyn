namespace PasswordGenerator.Core.Configuration;

/// <summary>
/// Constants that are reused in several components
/// </summary>
public static class AppConstants
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 256;
    public const int PasswordDefaultLength = 16;
    public const bool PasswordDefaultIncludeSpecialChars = true;

    public const string AlphanumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public const string SpecialChars = "!@#$%&*-_+=";
}
