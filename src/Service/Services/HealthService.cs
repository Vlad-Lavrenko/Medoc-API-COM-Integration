using MedocIntegration.Common.Models;
using System.Diagnostics;

namespace MedocIntegration.Service.Services;

public interface IHealthService
{
    HealthStatus GetHealthStatus();
}

public class HealthService : IHealthService
{
    private readonly ILogger<HealthService> _logger;

    public HealthService(ILogger<HealthService> logger)
    {
        _logger = logger;
    }

    public HealthStatus GetHealthStatus()
    {
        _logger.LogDebug("Health check requested");

        var status = new HealthStatus
        {
            Status = "Healthy",
            CheckedAt = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                { "uptime", Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds }
            }
        };

        _logger.LogInformation("Health check completed: {Status}", status.Status);

        return status;
    }
}
