using System.ComponentModel.DataAnnotations;

namespace MedocIntegration.Common.Models;

/// <summary>
/// Налаштування служби
/// </summary>
public class AppSettings
{
    public const string SectionName = "AppSettings";

    /// <summary>
    /// Налаштування API сервера
    /// </summary>
    public required ApiSettings Api { get; init; }

    /// <summary>
    /// Налаштування логування
    /// </summary>
    public required LoggingSettings Logging { get; init; }

    /// <summary>
    /// Шлях до користувацьких налаштувань
    /// </summary>
    public string? UserSettingsPath { get; init; }

    /// <summary>
    /// Дефолтні налаштування — використовуються при першому запуску
    /// коли файл налаштувань ще не існує
    /// </summary>
    public static AppSettings Default => new()
    {
        Api = new ApiSettings
        {
            Address = "http://localhost",
            Port = 5000
        },
        Logging = new LoggingSettings
        {
            MinimumLevel = "Information"
        }
    };
}

/// <summary>
/// Налаштування API
/// </summary>
public class ApiSettings 
{
    /// <summary>
    /// Адреса для прослуховування (наприклад: http://localhost, http://0.0.0.0)
    /// </summary>
    [Required]
    public required string Address { get; init; }

    /// <summary>
    /// Порт для прослуховування
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт має бути в діапазоні 1-65535")]
    public required int Port { get; init; }

    /// <summary>
    /// Повна URL адреса (генерується автоматично)
    /// </summary>
    public string Url => $"{Address}:{Port}";
}

/// <summary>
/// Налаштування логування
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Мінімальний рівень логування: Verbose, Debug, Information, Warning, Error, Fatal
    /// </summary>
    [Required]
    public required string MinimumLevel { get; init; }

    /// <summary>
    /// Отримати Serilog рівень логування
    /// </summary>
    public Serilog.Events.LogEventLevel GetLogLevel()
    {
        return MinimumLevel.ToLowerInvariant() switch
        {
            "verbose" => Serilog.Events.LogEventLevel.Verbose,
            "debug" => Serilog.Events.LogEventLevel.Debug,
            "information" => Serilog.Events.LogEventLevel.Information,
            "warning" => Serilog.Events.LogEventLevel.Warning,
            "error" => Serilog.Events.LogEventLevel.Error,
            "fatal" => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }

    /// <summary>
    /// Перевірка валідності рівня логування
    /// </summary>
    public bool IsValidLevel()
    {
        var validLevels = new[] { "verbose", "debug", "information", "warning", "error", "fatal" };
        return validLevels.Contains(MinimumLevel.ToLowerInvariant());
    }
}
