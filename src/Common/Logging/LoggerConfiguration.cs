using Serilog;
using Serilog.Events;

namespace MedocIntegration.Common.Logging;

public static class LoggerConfigurationExtensions
{
    private const string LogOutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    private const string ConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Єдина конфігурація логування для всіх додатків
    /// Все пишеться у текстові файли з можливістю одночасного читання
    /// </summary>
    public static LoggerConfiguration ConfigureFileLogging(
        this LoggerConfiguration configuration,
        string applicationName,
        LogEventLevel minimumLevel = LogEventLevel.Information,
        bool isDevelopment = false)
    {
        // В Development більш детальне логування
        var level = isDevelopment ? LogEventLevel.Debug : minimumLevel;

        return configuration
            .MinimumLevel.Is(level)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(outputTemplate: ConsoleOutputTemplate)
            .WriteTo.File(
                path: $"logs/{applicationName.ToLower()}-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: LogOutputTemplate,
                shared: true,  // 🔥 Дозволяє читати файл іншим процесам (UI)
                flushToDiskInterval: TimeSpan.FromSeconds(1)  // 🔥 Швидке записування для моніторингу
            );
    }
}
