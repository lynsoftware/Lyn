; Inno Setup Script for Lyn Password Generator
; Adapted for GitHub Actions CI/CD

#define MyAppName "Lyn Password Generator"
#define MyAppVersion GetEnv('APP_VERSION')
#if MyAppVersion == ""
#define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "Fredrik Magee"
#define MyAppURL "https://www.lynsoftware.com/"
#define MyAppExeName "PasswordGenerator.exe"

[Setup]
AppId={{C236371D-6C8E-4173-B537-41BD22A758E4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=LynPasswordGenerator-Setup
SetupIconFile=Resources\AppIcon\lyn_logo.ico
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
