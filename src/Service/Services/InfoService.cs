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
        return new ServiceInfo
        {
            Service = "Service",
            Version = "1.0.0",
            Environment = _environment.EnvironmentName
        };
    }
}

public record ServiceInfo
{
    public required string Service { get; init; }
    public required string Version { get; init; }
    public required string Environment { get; init; }
}
