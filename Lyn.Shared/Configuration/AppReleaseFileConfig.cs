using Lyn.Shared.Enum;

namespace Lyn.Shared.Configuration;

public class AppReleaseFileConfig
{
    /// <summary>
    /// Maximum size for app releases (500 MB)
    /// </summary>
    public const long AppReleaseMaxSizeInBytes = 500 * 1024 * 1024;
    
    /// <summary>
    /// Tilatte Content Types for app release content types
    /// </summary>
    public static readonly HashSet<string> AppReleaseContentTypes = 
    [
        "application/vnd.android.package-archive",  // .apk
        "application/x-authorware-bin",             // .aab (Android App Bundle)
        "application/octet-stream",                 // .ipa, .msix, .pkg, .dmg
        "application/x-msdownload",                 // .exe
        "application/x-apple-diskimage",            // .dmg alternative
        "application/x-newton-compatible-pkg"       // .pkg alternative
    ];
    
    /// <summary>
    /// Extensions vi tilatter for Release Type. En Dictionary hvor Key = ReleaseType, og Value = extension/filendelse
    /// </summary>
    public static readonly Dictionary<ReleaseType, string> ReleaseTypeExtensions = new()
    {
        [ReleaseType.WindowsInstaller] = ".exe",
        [ReleaseType.WindowsStore] = ".msix",
        [ReleaseType.AndroidApk] = ".apk",
        [ReleaseType.AndroidPlayStore] = ".aab",
        [ReleaseType.iOS] = ".ipa",
        [ReleaseType.MacOS] = ".pkg",
        [ReleaseType.MacOSDmg] = ".dmg"
    };
    
    /// <summary>
    /// Henter extension utifra release type
    /// </summary>
    public static string? GetExtensionForReleaseType(ReleaseType releaseType)
        => ReleaseTypeExtensions.GetValueOrDefault(releaseType);
}