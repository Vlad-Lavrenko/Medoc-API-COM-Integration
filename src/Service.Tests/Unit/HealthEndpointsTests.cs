using FluentAssertions;
using Service.Services;

namespace Prototype.Service.Tests.Unit;

public class HealthServiceTests
{
    private readonly IHealthService _healthService;

    public HealthServiceTests()
    {
        _healthService = new HealthService();
    }

    [Fact]
    public void GetHealthStatus_ReturnsOkStatus()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("OK");
    }

    [Fact]
    public void GetHealthStatus_ReturnsCurrentTimestamp()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetHealthStatus_StatusIsNotEmpty()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.Status.Should().NotBeNullOrWhiteSpace();
    }
}
