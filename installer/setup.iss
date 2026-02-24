; =============================================================
; setup.iss - Inno Setup Script
; Medoc API COM Integration
; =============================================================

#define AppName        "Medoc API COM Integration"
#define AppVersion     "1.0.0"
#define AppPublisher   "Vlad Lavrenko"
#define AppURL         "https://github.com/Vlad-Lavrenko/Medoc-API-COM-Integration"
#define ServiceName    "MedocIntegrationService"
#define ServiceDisplay "Medoc Integration Service"
#define ServiceDescr   "REST API service for M.E.Doc COM integration"
#define ExeService     "Service\Service.exe"
#define ExeUI          "DesktopUI\DesktopUI.exe"

; Download URLs for .NET 10 runtimes (aka.ms always points to latest patch)
#define UrlAspNet  "https://aka.ms/dotnet/10.0/aspnetcore-runtime-win-x64.exe"
#define UrlDesktop "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe"

[Setup]
AppId={{F3A7C2E1-B894-4D56-9A3F-7E8C1D2B5F90}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
DefaultDirName={autopf}\MedocIntegration
DefaultGroupName={#AppName}
OutputDir=output
OutputBaseFilename=MedocIntegration_Setup_{#AppVersion}
WizardStyle=modern
PrivilegesRequired=admin
AllowNoIcons=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#ExeUI}
Compression=lzma2/ultra64
SolidCompression=yes
DiskSpanning=no
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} Installer
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}

[Languages]
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional options:"; Flags: checkedonce

[Files]
Source: "publish\Service\*";   DestDir: "{app}\Service";   Flags: ignoreversion recursesubdirs createallsubdirs
Source: "publish\DesktopUI\*"; DestDir: "{app}\DesktopUI"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Medoc Integration Manager";       Filename: "{app}\{#ExeUI}"; Comment: "Manage Medoc Integration Service"
Name: "{group}\Uninstall {#AppName}";             Filename: "{uninstallexe}"
Name: "{autodesktop}\Medoc Integration Manager"; Filename: "{app}\{#ExeUI}"; Comment: "Manage Medoc Integration Service"; Tasks: desktopicon

[Run]
Filename: "{app}\{#ExeUI}"; Description: "Launch Medoc Integration Manager"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\sc.exe"; Parameters: "stop {#ServiceName}";     Flags: runhidden waituntilterminated
Filename: "cmd.exe";      Parameters: "/c timeout /t 3 /nobreak"; Flags: runhidden waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "delete {#ServiceName}";   Flags: runhidden waituntilterminated

[Code]

{ ---- Helpers ---- }

function GetSC: String;
begin
  Result := ExpandConstant('{sys}\sc.exe');
end;

function RunSC(Params: String): Boolean;
var
  RC: Integer;
begin
  Exec(GetSC, Params, '', SW_HIDE, ewWaitUntilTerminated, RC);
  Result := (RC = 0);
end;

function ServiceExists: Boolean;
var
  RC: Integer;
begin
  Exec(GetSC, 'query {#ServiceName}', '', SW_HIDE, ewWaitUntilTerminated, RC);
  Result := (RC = 0);
end;

{ ---- .NET runtime checks ---- }

function IsRuntime10Installed(SharedFxName: String): Boolean;
var
  Key: String;
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;
  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\' + SharedFxName;
  if RegGetValueNames(HKLM, Key, Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      if Copy(Names[I], 1, 3) = '10.' then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;
end;

function IsAspNetCore10Installed: Boolean;
begin
  Result := IsRuntime10Installed('Microsoft.AspNetCore.App');
end;

function IsWindowsDesktop10Installed: Boolean;
begin
  Result := IsRuntime10Installed('Microsoft.WindowsDesktop.App');
end;

{ ---- Download and install ---- }

function DownloadFile(Url: String; DestPath: String): Boolean;
var
  RC: Integer;
  PSCmd: String;
begin
  Result := False;
  PSCmd := '-NoProfile -NonInteractive -Command "' +
    '[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12; ' +
    'Invoke-WebRequest -Uri ''' + Url + ''' -OutFile ''' + DestPath + '''"';
  Exec('powershell.exe', PSCmd, '', SW_HIDE, ewWaitUntilTerminated, RC);
  Result := (RC = 0) and FileExists(DestPath);
end;

function InstallRuntime(InstallerPath: String): Boolean;
var
  RC: Integer;
begin
  { /install /quiet /norestart - silent install without reboot }
  Exec(InstallerPath, '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, RC);
  { 0 = success, 3010 = success but reboot required }
  Result := (RC = 0) or (RC = 3010);
end;

function DownloadAndInstall(ComponentName: String; Url: String; FileName: String): Boolean;
var
  TempFile: String;
begin
  Result := False;
  TempFile := ExpandConstant('{tmp}\') + FileName;

  MsgBox(
    'Downloading ' + ComponentName + '...' + #13#10 +
    'Please wait, this may take a few minutes.' + #13#10 + #13#10 +
    'The installer will continue automatically after download.',
    mbInformation, MB_OK);

  if not DownloadFile(Url, TempFile) then
  begin
    MsgBox(
      'Failed to download ' + ComponentName + '.' + #13#10 + #13#10 +
      'Please install it manually from:' + #13#10 +
      'https://dotnet.microsoft.com/download/dotnet/10.0' + #13#10 + #13#10 +
      'The installation will continue, but the service may not start.',
      mbError, MB_OK);
    Exit;
  end;

  if not InstallRuntime(TempFile) then
  begin
    MsgBox(
      'Failed to install ' + ComponentName + '.' + #13#10 + #13#10 +
      'Please install it manually from:' + #13#10 +
      'https://dotnet.microsoft.com/download/dotnet/10.0',
      mbError, MB_OK);
    Exit;
  end;

  Result := True;
end;

{ ---- Main setup init ---- }

function InitializeSetup: Boolean;
var
  NeedAspNet: Boolean;
  NeedDesktop: Boolean;
  MissingList: String;
  Choice: Integer;
begin
  Result := True;

  NeedAspNet  := not IsAspNetCore10Installed;
  NeedDesktop := not IsWindowsDesktop10Installed;

  if (not NeedAspNet) and (not NeedDesktop) then
    Exit;

  MissingList := '';
  if NeedAspNet then
    MissingList := MissingList + '  - ASP.NET Core Runtime 10.0 (x64)' + #13#10;
  if NeedDesktop then
    MissingList := MissingList + '  - .NET Desktop Runtime 10.0 (x64)' + #13#10;

  Choice := MsgBox(
    'The following required .NET 10 components are missing:' + #13#10 + #13#10 +
    MissingList + #13#10 +
    'YES    - Download and install automatically (internet required)' + #13#10 +
    'NO     - Continue without installing (service may not start)' + #13#10 +
    'CANCEL - Abort installation',
    mbConfirmation, MB_YESNOCANCEL);

  if Choice = IDCANCEL then
  begin
    Result := False;
    Exit;
  end;

  if Choice = IDNO then
    Exit;

  { YES - download and install missing components }
  if NeedAspNet then
    DownloadAndInstall(
      'ASP.NET Core Runtime 10.0 (x64)',
      '{#UrlAspNet}',
      'aspnetcore-runtime-10.0-win-x64.exe');

  if NeedDesktop then
    DownloadAndInstall(
      '.NET Desktop Runtime 10.0 (x64)',
      '{#UrlDesktop}',
      'windowsdesktop-runtime-10.0-win-x64.exe');
end;

{ ---- Service registration after files are copied ---- }

procedure CurStepChanged(CurStep: TSetupStep);
var
  BinPath: String;
  Params: String;
  RC: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    BinPath := ExpandConstant('{app}\{#ExeService}');

    if ServiceExists then
    begin
      RunSC('stop {#ServiceName}');
      Exec('cmd.exe', '/c timeout /t 2 /nobreak', '', SW_HIDE, ewWaitUntilTerminated, RC);
      RunSC('delete {#ServiceName}');
      Exec('cmd.exe', '/c timeout /t 2 /nobreak', '', SW_HIDE, ewWaitUntilTerminated, RC);
    end;

    Params := 'create {#ServiceName} binPath= "' + BinPath + '" start= auto DisplayName= "{#ServiceDisplay}"';
    RunSC(Params);
    RunSC('description {#ServiceName} "{#ServiceDescr}"');
    RunSC('failure {#ServiceName} reset= 86400 actions= restart/5000/restart/10000/restart/30000');
    RunSC('start {#ServiceName}');
  end;
end;

{ ---- Uninstall notification ---- }

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    MsgBox(
      '{#AppName} has been successfully uninstalled.' + #13#10 + #13#10 +
      'Settings file is preserved at:' + #13#10 +
      'C:\ProgramData\MedocIntegration\settings.json' + #13#10 + #13#10 +
      'You can delete it manually if no longer needed.',
      mbInformation, MB_OK);
  end;
end;
