using MedocIntegration.Common.Configuration;
using MedocIntegration.DesktopUI.Services;
using MedocIntegration.DesktopUI.ViewModels;
using MedocIntegration.DesktopUI.Views;
using MedocIntegration.DesktopUI.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Windows;

namespace MedocIntegration.DesktopUI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Точка входу застосунку — ініціалізація Serilog, DI та запуск головного вікна
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ініціалізація Serilog для DesktopUI
        ConfigureSerilog();

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Налаштування Serilog логера для DesktopUI
    /// </summary>
    private static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()  // Виводить у Debug Output вікно Visual Studio
            .CreateLogger();
    }

    /// <summary>
    /// Реєстрація всіх залежностей у DI контейнері
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // ── Logging ───────────────────────────────────────
        // Реєструємо Serilog як провайдер для ILogger<T>
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });

        // ── Services ──────────────────────────────────────
        services.AddSingleton<IServiceController, WindowsServiceController>();
        services.AddSingleton<ILogReader, LogReaderService>();
        services.AddSingleton<ISettingsManager, SettingsManager>();

        // ── ViewModels ────────────────────────────────────
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // ── Views ─────────────────────────────────────────
        services.AddSingleton<MainWindow>();
        services.AddSingleton<DashboardPage>();
        services.AddSingleton<SettingsPage>();
    }

    /// <summary>
    /// Коректне завершення — звільнення ресурсів DI та Serilog
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush(); // Завершуємо Serilog коректно
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
