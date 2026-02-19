; =============================================================
; setup.iss — Inno Setup Script
; Medoc API COM Integration
; =============================================================

#define AppName        "Medoc API COM Integration"
#define AppVersion     "1.0.0"
#define AppPublisher   "Vlad Lavrenko"
#define AppURL         "https://github.com/Vlad-Lavrenko/Medoc-API-COM-Integration"
#define ServiceName    "MedocIntegrationService"
#define ServiceDisplay "Medoc Integration Service"
#define ServiceDescr   "REST API сервіс для інтеграції з M.E.Doc через COM-інтерфейс"
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

; Директорії
DefaultDirName={autopf}\MedocIntegration
DefaultGroupName={#AppName}
OutputDir=output
OutputBaseFilename=MedocIntegration_Setup_{#AppVersion}

; Вигляд та права
WizardStyle=modern
PrivilegesRequired=admin
AllowNoIcons=yes

; 64-бітна архітектура
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Іконка у "Додати/видалити програми"
UninstallDisplayIcon={app}\{#ExeUI}

; Стиснення
Compression=lzma2/ultra64
SolidCompression=yes
DiskSpanning=no

; Інформація про версію
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
Name: "desktopicon"; \
    Description: "Створити ярлик на робочому столі"; \
    GroupDescription: "Додаткові параметри:"; \
    Flags: checked

; =============================================================
[Files]
; =============================================================

; Service файли
Source: "publish\Service\*"; \
    DestDir: "{app}\Service"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

; DesktopUI файли
Source: "publish\DesktopUI\*"; \
    DestDir: "{app}\DesktopUI"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

; =============================================================
[Icons]
; =============================================================

; Меню Пуск
Name: "{group}\Medoc Integration Manager"; \
    Filename: "{app}\{#ExeUI}"; \
    Comment: "Керування Medoc Integration Service"

Name: "{group}\Видалити {#AppName}"; \
    Filename: "{uninstallexe}"

; Ярлик на робочому столі
Name: "{autodesktop}\Medoc Integration Manager"; \
    Filename: "{app}\{#ExeUI}"; \
    Comment: "Керування Medoc Integration Service"; \
    Tasks: desktopicon

; =============================================================
[Run]
; =============================================================

; Зупинка старої служби (при оновленні)
Filename: "sc.exe"; \
    Parameters: "stop {#ServiceName}"; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "Зупинка старої служби..."; \
    Check: ServiceExists

; Видалення старої служби (при оновленні)
Filename: "sc.exe"; \
    Parameters: "delete {#ServiceName}"; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "Видалення старої служби..."; \
    Check: ServiceExists

; Пауза після видалення
Filename: "cmd.exe"; \
    Parameters: "/c timeout /t 2 /nobreak"; \
    Flags: runhidden waituntilterminated; \
    Check: ServiceExists

; Реєстрація служби
Filename: "sc.exe"; \
    Parameters: "create {#ServiceName} binPath= """"{{app}}\{#ExeService}"""" start= auto DisplayName= """"{{#ServiceDisplay}}"""""; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "Реєстрація служби..."

; Опис служби
Filename: "sc.exe"; \
    Parameters: "description {#ServiceName} """"{#ServiceDescr}"""""; \
    Flags: runhidden waituntilterminated

; Налаштування автовідновлення після збою (3 спроби)
Filename: "sc.exe"; \
    Parameters: "failure {#ServiceName} reset= 86400 actions= restart/5000/restart/10000/restart/30000"; \
    Flags: runhidden waituntilterminated

; Запуск служби
Filename: "sc.exe"; \
    Parameters: "start {#ServiceName}"; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "Запуск служби..."

; Запуск DesktopUI після установки
Filename: "{app}\{#ExeUI}"; \
    Description: "Запустити Medoc Integration Manager"; \
    Flags: nowait postinstall skipifsilent

; =============================================================
[UninstallRun]
; =============================================================

; Зупинка служби
Filename: "sc.exe"; \
    Parameters: "stop {#ServiceName}"; \
    Flags: runhidden waituntilterminated

; Очікування зупинки
Filename: "cmd.exe"; \
    Parameters: "/c timeout /t 3 /nobreak"; \
    Flags: runhidden waituntilterminated

; Видалення служби
Filename: "sc.exe"; \
    Parameters: "delete {#ServiceName}"; \
    Flags: runhidden waituntilterminated

; =============================================================
[Code]
; =============================================================

/// Перевіряє наявність .NET 10 Runtime
function IsDotNet10Installed(): Boolean;
var
  Key: String;
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;

  // Перевірка Microsoft.WindowsDesktop.App (WPF)
  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
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

  // Резервна перевірка Microsoft.NETCore.App
  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App';
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

/// Перевіряє чи існує Windows Service з такою назвою
function ServiceExists(): Boolean;
var
  ResultCode: Integer;
begin
  Exec('sc.exe', 'query {#ServiceName}', '', SW_HIDE,
       ewWaitUntilTerminated, ResultCode);
  Result := (ResultCode = 0);
end;

/// Ініціалізація — перевірка .NET 10 перед установкою
function InitializeSetup(): Boolean;
begin
  Result := True;

  if not IsDotNet10Installed() then
  begin
    if MsgBox(
      '.NET 10 Runtime не знайдено на цьому компютері.' + #13#10 + #13#10 +
      'Для роботи програми необхідно встановити:' + #13#10 +
      '  • .NET 10 Desktop Runtime (x64) — для DesktopUI' + #13#10 +
      '  • .NET 10 Runtime (x64)         — для служби' + #13#10 + #13#10 +
      'Завантажити: https://dotnet.microsoft.com/download/dotnet/10.0' + #13#10 + #13#10 +
      'Продовжити встановку без .NET 10?',
      mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;

/// Повідомлення після видалення
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    MsgBox(
      '{#AppName} успішно видалено.' + #13#10 + #13#10 +
      'Файл налаштувань збережено у:' + #13#10 +
      'C:\ProgramData\MedocIntegration\settings.json' + #13#10 + #13#10 +
      'Ви можете видалити його вручну за необхідності.',
      mbInformation, MB_OK);
  end;
end;
