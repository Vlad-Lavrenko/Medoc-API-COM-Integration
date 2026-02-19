using MedocIntegration.DesktopUI.Models;

namespace MedocIntegration.DesktopUI.Services;

public interface ILogReader
{
    Task<List<LogEntry>> GetRecentLogsAsync(int count = 100);
    Task<List<LogEntry>> GetLogsByDateAsync(DateTime date);
    Task<List<string>> GetAvailableLogFilesAsync();
}
