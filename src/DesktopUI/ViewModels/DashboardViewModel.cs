using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedocIntegration.DesktopUI.Models;
using MedocIntegration.DesktopUI.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace MedocIntegration.DesktopUI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IServiceController _serviceController;
    private readonly ILogReader _logReader;
    private readonly System.Timers.Timer _refreshTimer;

    [ObservableProperty]
    private ServiceStatus _currentStatus = new();

    [ObservableProperty]
    private ObservableCollection<LogEntry> _logEntries = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Brush _statusColor = Brushes.Gray;

    [ObservableProperty]
    private string _statusIcon = "HelpCircle";

    public DashboardViewModel(IServiceController serviceController, ILogReader logReader)
    {
        _serviceController = serviceController;
        _logReader = logReader;

        // Початкове завантаження даних
        _ = InitializeAsync();

        // Автооновлення статусу кожні 5 секунд
        _refreshTimer = new System.Timers.Timer(5000);
        _refreshTimer.Elapsed += async (s, e) => await RefreshStatusAsync();
        _refreshTimer.Start();
    }

    /// <summary>
    /// Початкова ініціалізація: завантаження статусу та логів
    /// </summary>
    private async Task InitializeAsync()
    {
        await RefreshStatusAsync();
        await RefreshLogsAsync();
    }

    /// <summary>
    /// Запускає службу
    /// </summary>
    [RelayCommand]
    private async Task StartServiceAsync()
    {
        try
        {
            IsLoading = true;
            await _serviceController.StartAsync();
            await Task.Delay(2000);
            await RefreshStatusAsync();
            MessageBox.Show("Службу успішно запущено", "Успіх",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка запуску служби:\n{ex.Message}", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Зупиняє службу з підтвердженням
    /// </summary>
    [RelayCommand]
    private async Task StopServiceAsync()
    {
        var result = MessageBox.Show(
            "Ви впевнені що хочете зупинити службу?",
            "Підтвердження",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            await _serviceController.StopAsync();
            await Task.Delay(2000);
            await RefreshStatusAsync();
            MessageBox.Show("Службу успішно зупинено", "Успіх",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка зупинки служби:\n{ex.Message}", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Перезапускає службу з підтвердженням
    /// </summary>
    [RelayCommand]
    private async Task RestartServiceAsync()
    {
        var result = MessageBox.Show(
            "Ви впевнені що хочете перезапустити службу?",
            "Підтвердження",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            await _serviceController.RestartAsync();
            await Task.Delay(3000);
            await RefreshStatusAsync();
            MessageBox.Show("Службу успішно перезапущено", "Успіх",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка перезапуску служби:\n{ex.Message}", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Оновлює статус служби та індикатори
    /// </summary>
    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        try
        {
            CurrentStatus = await _serviceController.GetStatusAsync();
            UpdateStatusIndicators();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Помилка оновлення статусу: {ex.Message}");
        }
    }

    /// <summary>
    /// Оновлює список логів з файлу
    /// </summary>
    [RelayCommand]
    private async Task RefreshLogsAsync()
    {
        try
        {
            var logs = await _logReader.GetRecentLogsAsync(200);

            // Оновлення ObservableCollection з UI потоку
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Clear();
                foreach (var log in logs)
                    LogEntries.Add(log);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Помилка завантаження логів: {ex.Message}");
        }
    }

    /// <summary>
    /// Очищає список логів у UI (без видалення файлу)
    /// </summary>
    [RelayCommand]
    private void ClearLogs() => LogEntries.Clear();

    /// <summary>
    /// Відкриває Scalar API документацію у браузері за замовчуванням
    /// </summary>
    [RelayCommand]
    private void OpenScalar()
    {
        try
        {
            var url = "http://localhost:5000/scalar/v1";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не вдалося відкрити браузер:\n{ex.Message}", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Оновлює колір та іконку індикатора відповідно до поточного статусу
    /// </summary>
    private void UpdateStatusIndicators()
    {
        StatusColor = CurrentStatus.Status switch
        {
            System.ServiceProcess.ServiceControllerStatus.Running =>
                new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // Зелений
            System.ServiceProcess.ServiceControllerStatus.Stopped =>
                new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Червоний
            System.ServiceProcess.ServiceControllerStatus.StartPending or
            System.ServiceProcess.ServiceControllerStatus.StopPending =>
                new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Оранжевий
            _ =>
                new SolidColorBrush(Color.FromRgb(158, 158, 158))  // Сірий
        };

        StatusIcon = CurrentStatus.Status switch
        {
            System.ServiceProcess.ServiceControllerStatus.Running => "CheckCircle",
            System.ServiceProcess.ServiceControllerStatus.Stopped => "StopCircle",
            System.ServiceProcess.ServiceControllerStatus.StartPending => "ProgressClock",
            System.ServiceProcess.ServiceControllerStatus.StopPending => "ProgressClock",
            _ => "HelpCircle"
        };
    }
}
