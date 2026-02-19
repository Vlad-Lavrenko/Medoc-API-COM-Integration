using System.IO;
using System.Text.RegularExpressions;
using MedocIntegration.DesktopUI.Models;

namespace MedocIntegration.DesktopUI.Services;

public partial class LogReaderService : ILogReader
{
    private readonly string _logsDirectory;

    // Парсить рядок логу Serilog формату:
    // 2026-02-19 15:00:00.123 +02:00 [INF] Повідомлення
    [GeneratedRegex(
        @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) \[(\w{3})\] (.+)$",
        RegexOptions.Compiled)]
    private static partial Regex LogLineRegex();

    public LogReaderService()
    {
        // Шлях до логів відносно розташування служби
        _logsDirectory = ResolveLogsDirectory();
    }

    public async Task<List<LogEntry>> GetRecentLogsAsync(int count = 100)
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var logFile = Path.Combine(_logsDirectory, $"medocservice-{today}.log");

        if (!File.Exists(logFile))
            return new List<LogEntry>();

        return await Task.Run(() =>
        {
            // Читаємо з кінця файлу — останні count рядків
            var lines = File.ReadLines(logFile)
                .Reverse()
                .Take(count)
                .Reverse();

            return ParseLogLines(lines).ToList();
        });
    }

    public async Task<List<LogEntry>> GetLogsByDateAsync(DateTime date)
    {
        var dateString = date.ToString("yyyyMMdd");
        var logFile = Path.Combine(_logsDirectory, $"medocservice-{dateString}.log");

        if (!File.Exists(logFile))
            return new List<LogEntry>();

        return await Task.Run(() =>
        {
            var lines = File.ReadAllLines(logFile);
            return ParseLogLines(lines).ToList();
        });
    }

    public async Task<List<string>> GetAvailableLogFilesAsync()
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(_logsDirectory))
                return new List<string>();

            return Directory.GetFiles(_logsDirectory, "medocservice-*.log")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .OrderByDescending(f => f)
                .ToList();
        });
    }

    private IEnumerable<LogEntry> ParseLogLines(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var match = LogLineRegex().Match(line);
            if (match.Success)
            {
                yield return new LogEntry
                {
                    Timestamp = DateTimeOffset.Parse(match.Groups[1].Value).DateTime,
                    Level = match.Groups[2].Value,
                    Message = match.Groups[3].Value
                };
            }
            else
            {
                // Рядки що не підпадають під формат (наприклад stack trace)
                yield return new LogEntry
                {
                    Timestamp = DateTime.MinValue,
                    Level = "---",
                    Message = line
                };
            }
        }
    }

    private static string ResolveLogsDirectory()
    {
        // Шукаємо директорію logs відносно DesktopUI або Services
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Варіант 1: поруч з DesktopUI (розробка)
        var candidate1 = Path.Combine(baseDir, "logs");
        if (Directory.Exists(candidate1))
            return candidate1;

        // Варіант 2: на 4 рівні вище (src\DesktopUI\bin\Debug\net10.0-windows)
        var candidate2 = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "logs"));
        if (Directory.Exists(candidate2))
            return candidate2;

        // Варіант 3: з ProgramData (production)
        var candidate3 = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "MedocIntegration", "logs");
        if (Directory.Exists(candidate3))
            return candidate3;

        // Повертаємо candidate2 як fallback (шлях до кореня проекту)
        return candidate2;
    }
}
