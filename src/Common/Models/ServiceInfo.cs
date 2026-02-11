namespace MedocIntegration.Common.Models;

/// <summary>
/// Інформація про службу/додаток
/// Використовується як в Service API, так і в Desktop UI
/// </summary>
public record ServiceInfo
{
    public required string Service { get; init; }
    public required string Version { get; init; }
    public required string Environment { get; init; }
    public required string HostName { get; init; }
    public required DateTime StartedAt { get; init; }
}

/// <summary>
/// Статус здоров'я служби
/// </summary>
public record HealthStatus
{
    public required string Status { get; init; }
    public required DateTime CheckedAt { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}
