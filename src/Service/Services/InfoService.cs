using System.Diagnostics;
using System.Reflection;

namespace Service.Services;

public interface IInfoService
{
    ServiceInfo GetServiceInfo();
}

public class InfoService : IInfoService
{
    private readonly IHostEnvironment _environment;

    public InfoService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ServiceInfo GetServiceInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "unknown";

        return new ServiceInfo
        {
            Service = assembly.GetName().Name ?? "Service",
            Version = version,
            Environment = _environment.EnvironmentName,
            HostName = Environment.MachineName,
            StartedAt = Process.GetCurrentProcess().StartTime
        };
    }
}

public record ServiceInfo
{
    public required string Service { get; init; }
    public required string Version { get; init; }
    public required string Environment { get; init; }
    public required string HostName { get; init; }
    public required DateTime StartedAt { get; init; }
}
