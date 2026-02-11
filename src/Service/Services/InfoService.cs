using System.Diagnostics;
using System.Reflection;
using MedocIntegration.Common.Models;

namespace MedocIntegration.Service.Services;

public interface IInfoService
{
    ServiceInfo GetServiceInfo();
}

public class InfoService : IInfoService
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<InfoService> _logger;

    public InfoService(IHostEnvironment environment, ILogger<InfoService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public ServiceInfo GetServiceInfo()
    {
        _logger.LogDebug("Getting service information");

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "unknown";

        var info = new ServiceInfo
        {
            Service = assembly.GetName().Name ?? "Service",
            Version = version,
            Environment = _environment.EnvironmentName,
            HostName = Environment.MachineName,
            StartedAt = Process.GetCurrentProcess().StartTime
        };

        _logger.LogInformation("Service info retrieved: {ServiceName} v{Version} on {HostName}",
            info.Service, info.Version, info.HostName);

        return info;
    }
}
