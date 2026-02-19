using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedocIntegration.Common.Configuration;
using MedocIntegration.Common.Models;
using System.Net;
using System.Windows;

namespace MedocIntegration.DesktopUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsManager _settingsManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullUrl))]
    private string _apiAddress = "http://localhost";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullUrl))]
    private int _apiPort = 5000;

    [ObservableProperty]
    private string _loggingLevel = "Information";

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Повна URL адреса — генерується автоматично з Address + Port
    /// </summary>
    public string FullUrl => $"{ApiAddress}:{ApiPort}";

    /// <summary>
    /// Список доступних рівнів логування для ComboBox
    /// </summary>
    public List<string> LoggingLevels { get; } =
        new() { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" };

    public SettingsViewModel(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;

        // Автоматично завантажуємо налаштування при відкритті
        _ = LoadSettingsAsync();
    }

    /// <summary>
    /// Завантажує налаштування з файлу у поля форми
    /// </summary>
    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            IsLoading = true;

            var settings = await _settingsManager.LoadSettingsAsync<AppSettings>();

            if (settings != null)
            {
                ApiAddress = settings.Api.Address;
                ApiPort = settings.Api.Port;
                LoggingLevel = settings.Logging.MinimumLevel;

                MessageBox.Show("Налаштування успішно завантажено", "Успіх",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Файл налаштувань не знайдено.\nВикористовуються значення за замовчуванням.",
                    "Інформація",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження налаштувань:\n{ex.Message}", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Валідує та зберігає налаштування у файл
    /// </summary>
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        // Валідація перед збереженням
        if (!ValidateSettings())
            return;

        try
        {
            IsLoading = true;

            var settings = new AppSettings
            {
                Api = new ApiSettings
                {
                    Address = ApiAddress.Trim(),
                    Port = ApiPort
                },
                Logging = new LoggingSettings
                {
                    MinimumLevel = LoggingLevel
                }
            };

            await _settingsManager.SaveSettingsAsync(settings);

            MessageBox.Show(
                "Налаштування успішно збережено!\n\n" +
                "Для застосування змін необхідно перезапустити службу.",
                "Успіх",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка збереження налаштувань:\n{ex.Message}", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Перевіряє коректність введених даних
    /// </summary>
    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(ApiAddress))
        {
            MessageBox.Show("Адреса не може бути порожньою", "Помилка валідації",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!ApiAddress.StartsWith("http://") && !ApiAddress.StartsWith("https://"))
        {
            MessageBox.Show("Адреса повинна починатись з http:// або https://",
                "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (ApiPort < 1 || ApiPort > 65535)
        {
            MessageBox.Show("Порт має бути в діапазоні 1-65535", "Помилка валідації",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
