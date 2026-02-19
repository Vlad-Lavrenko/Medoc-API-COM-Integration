using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MedocIntegration.Common.Configuration;

/// <summary>
/// Менеджер для роботи з налаштуваннями служби
/// </summary>
public interface ISettingsManager
{
    string GetSettingsFilePath();
    bool SettingsFileExists();

    /// <summary>
    /// Перевіряє наявність файлу налаштувань.
    /// Якщо файл відсутній — створює його з переданими дефолтними значеннями.
    /// Повертає актуальні налаштування з файлу.
    /// </summary>
    Task<T> EnsureCreatedAsync<T>(T defaults) where T : class;

    Task<T?> LoadSettingsAsync<T>() where T : class;
    Task SaveSettingsAsync<T>(T settings) where T : class;
}

public class SettingsManager : ISettingsManager
{
    private readonly ILogger<SettingsManager> _logger;
    private readonly string _settingsDirectory;
    private readonly string _settingsFileName;

    public SettingsManager(ILogger<SettingsManager> logger, string? customDirectory = null)
    {
        _logger = logger;

        // Використовуємо ProgramData для служб Windows (доступно під системними правами)
        _settingsDirectory = customDirectory ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MedocIntegration"
            );

        _settingsFileName = "settings.json";

        // Створюємо директорію якщо не існує
        EnsureDirectoryExists();
    }

    public string GetSettingsFilePath()
    {
        return Path.Combine(_settingsDirectory, _settingsFileName);
    }

    public bool SettingsFileExists()
    {
        return File.Exists(GetSettingsFilePath());
    }

    /// <summary>
    /// Гарантує існування файлу налаштувань.
    /// Якщо файл відсутній — записує дефолти та повертає їх.
    /// Якщо файл існує — читає та повертає поточні значення.
    /// </summary>
    public async Task<T> EnsureCreatedAsync<T>(T defaults) where T : class
    {
        if (!SettingsFileExists())
        {
            _logger.LogInformation(
                "Файл налаштувань не знайдено. Створюємо з дефолтними значеннями: {Path}",
                GetSettingsFilePath());

            await SaveSettingsAsync(defaults);
            return defaults;
        }

        _logger.LogInformation(
            "Файл налаштувань знайдено: {Path}",
            GetSettingsFilePath());

        // Файл є — читаємо актуальні налаштування
        var existing = await LoadSettingsAsync<T>();

        if (existing is null)
        {
            // Файл пошкоджений — перезаписуємо дефолтами
            _logger.LogWarning(
                "Файл налаштувань пошкоджено. Відновлюємо з дефолтними значеннями: {Path}",
                GetSettingsFilePath());

            await SaveSettingsAsync(defaults);
            return defaults;
        }

        return existing;
    }

    public async Task<T?> LoadSettingsAsync<T>() where T : class
    {
        var filePath = GetSettingsFilePath();

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Файл налаштувань не знайдено: {FilePath}", filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var settings = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            _logger.LogInformation("Налаштування успішно завантажено з {FilePath}", filePath);
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка читання налаштувань з {FilePath}", filePath);
            throw;
        }
    }

    public async Task SaveSettingsAsync<T>(T settings) where T : class
    {
        var filePath = GetSettingsFilePath();

        try
        {
            EnsureDirectoryExists();

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(filePath, json);
            _logger.LogInformation("Налаштування успішно збережено у {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка запису налаштувань у {FilePath}", filePath);
            throw;
        }
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_settingsDirectory))
        {
            Directory.CreateDirectory(_settingsDirectory);
            _logger.LogInformation("Створено директорію для налаштувань: {Directory}", _settingsDirectory);
        }
    }
}
