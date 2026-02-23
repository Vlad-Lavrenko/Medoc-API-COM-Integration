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

; =============================================================
[Setup]
; =============================================================
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

; =============================================================
[Languages]
; =============================================================
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

; =============================================================
[Tasks]
; =============================================================
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional options:"; Flags: checkedonce

; =============================================================
[Files]
; =============================================================
Source: "publish\Service\*";   DestDir: "{app}\Service";   Flags: ignoreversion recursesubdirs createallsubdirs
Source: "publish\DesktopUI\*"; DestDir: "{app}\DesktopUI"; Flags: ignoreversion recursesubdirs createallsubdirs

; =============================================================
[Icons]
; =============================================================
Name: "{group}\Medoc Integration Manager"; Filename: "{app}\{#ExeUI}"; Comment: "Manage Medoc Integration Service"
Name: "{group}\Uninstall {#AppName}";      Filename: "{uninstallexe}"
Name: "{autodesktop}\Medoc Integration Manager"; Filename: "{app}\{#ExeUI}"; Comment: "Manage Medoc Integration Service"; Tasks: desktopicon

; =============================================================
[Run]
; =============================================================

; Stop old service if exists (upgrade scenario)
Filename: "sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden waituntilterminated; StatusMsg: "Stopping old service..."; Check: ServiceExists

; Delete old service if exists (upgrade scenario)
Filename: "sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden waituntilterminated; StatusMsg: "Removing old service..."; Check: ServiceExists

; Short pause so SCM releases the binary
Filename: "cmd.exe"; Parameters: "/c timeout /t 2 /nobreak"; Flags: runhidden waituntilterminated; Check: ServiceExists

; Register service
Filename: "{sys}\sc.exe"; Parameters: "create {#ServiceName} binPath= """{app}\{#ExeService}""" start= auto DisplayName= ""{#ServiceDisplay}"""; Flags: runhidden waituntilterminated; StatusMsg: "Registering service..."

; Set service description
Filename: "{sys}\sc.exe"; Parameters: "description {#ServiceName} ""{#ServiceDescr}"""; Flags: runhidden waituntilterminated

; Configure auto-recovery (3 restarts)
Filename: "{sys}\sc.exe"; Parameters: "failure {#ServiceName} reset= 86400 actions= restart/5000/restart/10000/restart/30000"; Flags: runhidden waituntilterminated

; Start service
Filename: "{sys}\sc.exe"; Parameters: "start {#ServiceName}"; Flags: runhidden waituntilterminated; StatusMsg: "Starting service..."

; Launch DesktopUI after install
Filename: "{app}\{#ExeUI}"; Description: "Launch Medoc Integration Manager"; Flags: nowait postinstall skipifsilent

; =============================================================
[UninstallRun]
; =============================================================
Filename: "{sys}\sc.exe"; Parameters: "stop {#ServiceName}";   Flags: runhidden waituntilterminated
Filename: "cmd.exe";        Parameters: "/c timeout /t 3 /nobreak"; Flags: runhidden waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden waituntilterminated

; =============================================================
[Code]
; =============================================================

function IsDotNet10Installed(): Boolean;
var
  Key: String;
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;

  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  if RegGetValueNames(HKLM, Key, Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
      if Copy(Names[I], 1, 3) = '10.' then
      begin
        Result := True;
        Exit;
      end;
  end;

  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App';
  if RegGetValueNames(HKLM, Key, Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
      if Copy(Names[I], 1, 3) = '10.' then
      begin
        Result := True;
        Exit;
      end;
  end;
end;

function ServiceExists(): Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\sc.exe'), 'query {#ServiceName}', '',
       SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsDotNet10Installed() then
  begin
    if MsgBox(
      '.NET 10 Runtime was not found on this computer.' + #13#10 + #13#10 +
      'The application requires:' + #13#10 +
      '  - .NET 10 Desktop Runtime (x64) for DesktopUI' + #13#10 +
      '  - .NET 10 Runtime (x64) for the service' + #13#10 + #13#10 +
      'Download: https://dotnet.microsoft.com/download/dotnet/10.0' + #13#10 + #13#10 +
      'Continue installation without .NET 10?',
      mbConfirmation, MB_YESNO) = IDNO then
      Result := False;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    MsgBox(
      '{#AppName} has been successfully uninstalled.' + #13#10 + #13#10 +
      'Settings file is preserved at:' + #13#10 +
      'C:\ProgramData\MedocIntegration\settings.json' + #13#10 + #13#10 +
      'You can delete it manually if no longer needed.',
      mbInformation, MB_OK);
end;
