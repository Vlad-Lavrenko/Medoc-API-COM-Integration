# Medoc API COM Integration

REST API сервіс для інтеграції з системою M.E.Doc через COM-інтерфейс. Складається з Windows Service (REST API), Desktop UI для керування та спільної бібліотеки.

## 🎯 Призначення

Проект створений для автоматизації роботи з M.E.Doc через REST API:
- Створення та обробка документів
- Електронне підписання
- Відправка до контролюючих органів (ДПС України)
- Моніторинг стану служби
- Керування налаштуваннями через Desktop UI

---

## 🏗️ Архітектура проекту

```
Medoc-API-COM-Integration/
├── src/
│   ├── Common/                         # Спільна бібліотека
│   │   ├── Configuration/              # Система налаштувань
│   │   │   ├── SettingsManager.cs      # Менеджер читання/запису налаштувань
│   │   │   └── SettingsValidator.cs    # Валідація налаштувань
│   │   ├── Constants/                  # Константи додатку
│   │   ├── Logging/                    # Конфігурації Serilog
│   │   └── Models/                     # Спільні моделі даних
│   │       ├── ApiResponse.cs          # Стандартизована обгортка відповідей API
│   │       ├── AppSettings.cs          # Модель налаштувань служби
│   │       └── ServiceInfo.cs          # Моделі інформації про службу
│   ├── Service/                        # REST API Windows Service
│   │   ├── Endpoints/                  # Minimal API endpoints
│   │   ├── Extensions/                 # Extension методи
│   │   ├── Middleware/                 # ASP.NET Core middleware
│   │   ├── Services/                   # Бізнес-логіка
│   │   ├── appsettings.json            # Налаштування за замовчуванням
│   │   └── Program.cs                  # Точка входу
│   ├── DesktopUI/                      # WPF Desktop застосунок
│   │   ├── Converters/                 # XAML Value Converters
│   │   ├── Models/                     # Моделі UI (ServiceStatus, LogEntry)
│   │   ├── Services/                   # Сервіси UI
│   │   ├── ViewModels/                 # MVVM ViewModels
│   │   ├── Views/                      # XAML вікна та контроли
│   │   ├── App.xaml                    # Application entry point
│   │   ├── app.manifest                # UAC elevation manifest
│   │   └── DesktopUI.csproj
│   └── Service.Tests/                  # Unit-тести
└── installer/
    ├── setup.iss                       # Inno Setup скрипт
    └── build.ps1                       # PowerShell збірка інсталятора
```

---

## 🚀 Технологічний стек

### Платформа
- **.NET 10.0** — остання версія .NET платформи
- **C# 13** — сучасні можливості мови

### REST API Service
- **ASP.NET Core Minimal API** — легковажний підхід до створення API
- **OpenAPI 3.1** — стандарт опису REST API
- **Scalar 2.12** — сучасний інтерактивний UI для документації API
- **Microsoft.Extensions.Hosting.WindowsServices** — запуск як Windows Service

### Desktop UI
- **WPF (.NET 10)** — Windows Presentation Foundation
- **MaterialDesignThemes 5.3** — Material Design 3 компоненти
- **CommunityToolkit.Mvvm 8.4** — MVVM helpers (ObservableObject, RelayCommand)
- **Microsoft.Extensions.DependencyInjection 10.0** — DI контейнер

### Логування
- **Serilog 4.2** — structured logging
- **Serilog.Sinks.File** — запис у файли з ротацією (щодня, 30 днів)
- **Serilog.Sinks.Console** — вивід у консоль (тільки в інтерактивному режимі)
- **Enrichers** — machine name, thread ID, process ID, environment

### Тестування
- **xUnit 2.9** — фреймворк для unit-тестів
- **NSubstitute 5.3** — mocking library
- **FluentAssertions 7.0** — виразні assertions

---

## 📋 Реалізований функціонал

### REST API Endpoints

#### `GET /health`
Перевірка стану служби

```json
{
  "result": "ok",
  "errorMsg": "",
  "data": {
    "status": "Healthy",
    "checkedAt": "2026-02-19T21:00:00Z",
    "details": { "uptime": 123.45 }
  }
}
```

#### `GET /info`
Інформація про службу (версія, середовище, хост, час запуску)

```json
{
  "result": "ok",
  "errorMsg": "",
  "data": {
    "service": "Service",
    "version": "1.0.0.0",
    "environment": "Production",
    "hostName": "DESKTOP-PC",
    "startedAt": "2026-02-19T21:00:00Z"
  }
}
```

#### Стандартний формат відповіді
```json
{ "result": "ok" | "error", "errorMsg": "", "data": { } }
```

### Desktop UI
- 🟢/🔴 Індикатор стану служби (автооновлення кожні 5 сек)
- Кнопки: **Запустити / Зупинити / Перезапустити / Оновити**
- Відкриття Scalar API документації у браузері
- Таблиця логів з кольоровим маркуванням (VRB/DBG/INF/WRN/ERR/FTL)
- Сторінка налаштувань (читання/запис settings.json)

---

## ⚙️ Система налаштувань

### Файли конфігурації

| Файл | Призначення |
|---|---|
| `src/Service/appsettings.json` | Дефолтні значення (вбудовані в збірку) |
| `C:\ProgramData\MedocIntegration\settings.json` | Користувацькі налаштування (створюються автоматично при першому запуску) |

### Доступні налаштування

```json
{
  "api": {
    "address": "http://localhost",
    "port": 5000
  },
  "logging": {
    "minimumLevel": "Information"
  }
}
```

**Address:**
- `http://localhost` — тільки локальний доступ
- `http://0.0.0.0` — доступ з усіх мережевих інтерфейсів

**MinimumLevel:** `Verbose` | `Debug` | `Information` | `Warning` | `Error` | `Fatal`

---

## 📊 Логування

### Шляхи до файлів логів

```
C:\ProgramData\MedocIntegration\logs\medocservice-YYYYMMDD.log   ← Windows Service
C:\ProgramData\MedocIntegration\logs\medocui-YYYYMMDD.log        ← Desktop UI
```

> **Важливо:** Логи пишуться в `ProgramData` (абсолютний шлях).  
> Windows Service запускається з робочою директорією `C:\Windows\System32`,  
> тому відносні шляхи не працюють.

**Console sink** — активний тільки в інтерактивному режимі (`Environment.UserInteractive = true`).  
Коли служба запущена через SCM — консолі немає, sink автоматично вимикається.

**Формат рядка:**
```
2026-02-19 21:00:15.123 +02:00 [INF] [SourceContext] Message
```

**Параметри:**
- Ротація: щодня
- Зберігання: 30 днів
- Shared access: дозволений (Desktop UI читає одночасно)
- Flush: кожну секунду

---

## 🖥️ Windows Service

### Параметри служби

| Параметр | Значення |
|---|---|
| Ім'я | `MedocIntegrationService` |
| Display Name | `Medoc Integration Service` |
| Description | `REST API service for M.E.Doc COM integration` |
| Start Type | Automatic |
| Бінарник | `{InstallDir}\Service\Service.exe` |
| Recovery | Restart after 5s / 10s / 30s |

### Важливе в `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ПЕРШИМ — до UseSerilog, UseUrls і будь-чого іншого
// Встановлює ContentRoot = AppContext.BaseDirectory
builder.Host.UseWindowsService();
```

`UseWindowsService()` **обов'язково має бути першим** після `CreateBuilder`.  
Якщо викликати після `UseSerilog` або `UseUrls` — служба не реєструється правильно і отримуємо **Error 1053**.

---

## 📦 Інсталятор

### Вимоги до системи

| Компонент | Версія | Призначення |
|---|---|---|
| Windows | 10/11 або Server 2019+ | Операційна система |
| `Microsoft.AspNetCore.App` | 10.0.x x64 | **REST API Service** |
| `Microsoft.WindowsDesktop.App` | 10.0.x x64 | Desktop UI (WPF) |

> ⚠️ **Потрібен саме ASP.NET Core Runtime**, не просто .NET Runtime.  
> Service — це ASP.NET Core Web Application і без `Microsoft.AspNetCore.App`  
> служба не запуститься (Error 1053, без жодних логів).

Перевірка встановлених runtime:
```powershell
dotnet --list-runtimes
# Мають бути рядки:
# Microsoft.AspNetCore.App    10.0.x [...]
# Microsoft.WindowsDesktop.App 10.0.x [...]
```

### Збірка інсталятора

```powershell
git pull
cd installer
.\build.ps1 -InnoPath "E:\Program Files (x86)\Inno Setup 6\ISCC.exe"
```

Результат: `installer\output\MedocIntegration_Setup_1.0.0.exe`

### Поведінка інсталятора

1. **Перевірка .NET компонентів** на старті (registry `HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\...`)
2. Якщо щось відсутнє — діалог:
   - **YES** → завантажує і встановлює автоматично (потрібен інтернет)
   - **NO** → продовжує без встановлення (служба може не стартувати)
   - **CANCEL** → скасовує інсталяцію
3. Завантаження через PowerShell `Invoke-WebRequest` з `aka.ms/dotnet/10.0/...`
4. Встановлення у режимі `/passive` — видно вікно прогресу
5. Копіювання файлів Service і DesktopUI
6. Реєстрація і запуск Windows Service через `sc.exe`

**URLs завантаження:**
```
https://aka.ms/dotnet/10.0/aspnetcore-runtime-win-x64.exe
https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe
```

### Деінсталяція

Служба зупиняється і видаляється автоматично.  
Файл `C:\ProgramData\MedocIntegration\settings.json` **зберігається** (потрібно видалити вручну).

---

## 🔧 Розробка

### Запуск у режимі розробки

```bash
git clone https://github.com/Vlad-Lavrenko/Medoc-API-COM-Integration.git
cd Medoc-API-COM-Integration
```

Відкрийте `Medoc API COM Integration.slnx` у Visual Studio.  
Встановіть Multiple Startup Projects: `Service` → Start, `DesktopUI` → Start.  
Запустіть `F5`.

Scalar документація: `http://localhost:5000/scalar/v1`

### Запуск Unit-тестів

```bash
dotnet test src/Service.Tests/Service.Tests.csproj
```

### Налагодження служби без SCM

Коли `Service.exe` запускається напряму (не через SCM), `Environment.UserInteractive = true` — з'являється консоль і Console sink активується автоматично:

```powershell
cd "C:\Program Files\MedocIntegration\Service"
.\Service.exe
# Виводить всі логи в консоль, видно точну помилку
```

### Ручне керування службою

```powershell
sc start  MedocIntegrationService
sc stop   MedocIntegrationService
sc delete MedocIntegrationService
```

---

## 🎯 Roadmap

- [x] REST API з OpenAPI/Scalar документацією
- [x] Structured logging через Serilog
- [x] Система налаштувань (JSON + ProgramData)
- [x] Desktop UI (WPF + Material Design 3)
- [x] Керування Windows Service через UI
- [x] Перегляд логів у реальному часі
- [x] Інсталятор Inno Setup з авто-встановленням .NET
- [ ] COM інтеграція з M.E.Doc
- [ ] Endpoints для роботи з документами
- [ ] Електронне підписання через ЕЦП
- [ ] Авторизація та аутентифікація API

---

## 📝 Ліцензія

Приватний проект.

## 👨‍💻 Автор

**Vlad Lavrenko** — [@Vlad-Lavrenko](https://github.com/Vlad-Lavrenko)

## 🔗 Корисні посилання

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Serilog Documentation](https://serilog.net/)
- [Scalar API Documentation](https://scalar.com/)
- [Material Design In XAML](https://materialdesigninxaml.net/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [M.E.Doc Official Site](https://medoc.ua/)
- [Inno Setup Documentation](https://jrsoftware.org/ishelp/)
- [.NET 10 Download](https://dotnet.microsoft.com/download/dotnet/10.0)
