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
    MacOS,               // .pkg/.dmg
    iOS,                  // .ipa
    
    // Linux
    Linux
    
}