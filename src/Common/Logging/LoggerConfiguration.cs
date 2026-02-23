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
    /// Returns absolute log directory path under ProgramData.
    /// Windows Service runs with working directory = System32,
    /// so relative paths would fail with access denied.
    /// </summary>
    private static string GetLogDirectory(string applicationName)
    {
        // ProgramData is accessible by Windows Service (SYSTEM account)
        var programData = Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData);

        var logDir = Path.Combine(programData, "MedocIntegration", "logs");

        // Ensure directory exists before Serilog tries to write
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);

        return Path.Combine(logDir, $"{applicationName.ToLower()}-.log");
    }

    /// <summary>
    /// Unified logging configuration for all applications.
    /// isDevelopment=true adds Console sink (interactive mode only).
    /// </summary>
    public static LoggerConfiguration ConfigureFileLogging(
        this LoggerConfiguration configuration,
        string applicationName,
        LogEventLevel minimumLevel = LogEventLevel.Information,
        bool isDevelopment = false)
    {
        var level = isDevelopment ? LogEventLevel.Debug : minimumLevel;
        var logPath = GetLogDirectory(applicationName);

        var config = configuration
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
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: LogOutputTemplate,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1)
            );

        // Console only in interactive/development mode
        // Windows Service has no console - adding it causes silent failure
        if (isDevelopment || Environment.UserInteractive)
        {
            config = config.WriteTo.Console(outputTemplate: ConsoleOutputTemplate);
        }

        return config;
    }
}
