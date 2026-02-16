namespace Lyn.Shared.Enum;

public enum ReleaseType
{
    // Direkte nedlasting fra nettside
    WindowsInstaller,    // .exe
    AndroidApk,          // .apk
    
    // Play Store-distribusjon
    WindowsStore,        // .msix
    AndroidPlayStore,    // .aab
    
    // Apple
    iOS,                 // .ipa
    MacOS,               // .pkg for Mac App Store
    MacOSDmg,            // .dmg for website download
    
    
    // Linux
    Linux
    
}