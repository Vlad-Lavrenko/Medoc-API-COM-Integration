namespace Service.Services;

public interface IHealthService
{
    HealthCheckResult GetHealthStatus();
}

public class HealthService : IHealthService
{
    public HealthCheckResult GetHealthStatus()
    {
        return new HealthCheckResult
        {
            Status = "OK",
            Timestamp = DateTime.UtcNow
        };
    }
}

public record HealthCheckResult
{
    public required string Status { get; init; }
    public DateTime Timestamp { get; init; }
}
