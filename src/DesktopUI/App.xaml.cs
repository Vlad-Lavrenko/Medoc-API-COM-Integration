using MedocIntegration.Common.Configuration;
using MedocIntegration.DesktopUI.Services;
using MedocIntegration.DesktopUI.ViewModels;
using MedocIntegration.DesktopUI.Views;
using MedocIntegration.DesktopUI.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MedocIntegration.DesktopUI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Точка входу застосунку — ініціалізація DI та запуск головного вікна
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Показуємо головне вікно через DI
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Реєстрація всіх залежностей у DI контейнері
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // ── Services ──────────────────────────────────────
        // Singleton — один екземпляр на весь час роботи застосунку
        services.AddSingleton<IServiceController, WindowsServiceController>();
        services.AddSingleton<ILogReader, LogReaderService>();
        services.AddSingleton<ISettingsManager, SettingsManager>();

        // ── ViewModels ────────────────────────────────────
        // Singleton для ViewModels щоб зберігати стан між перемиканням вкладок
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // ── Views ─────────────────────────────────────────
        services.AddSingleton<MainWindow>();
        services.AddSingleton<DashboardPage>();
        services.AddSingleton<SettingsPage>();
    }

    /// <summary>
    /// Коректне завершення — звільнення ресурсів DI контейнера
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
