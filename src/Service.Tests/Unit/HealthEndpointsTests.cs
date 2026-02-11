using FluentAssertions;
using MedocIntegration.Service.Services;
using MedocIntegration.Common.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MedocIntegration.Service.Tests.Unit;

public class HealthServiceTests
{
    private readonly IHealthService _healthService;
    private readonly ILogger<HealthService> _logger;

    public HealthServiceTests()
    {
        // Mock logger
        _logger = Substitute.For<ILogger<HealthService>>();
        _healthService = new HealthService(_logger);
    }

    [Fact]
    public void GetHealthStatus_ReturnsHealthyStatus()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Healthy");
    }

    [Fact]
    public void GetHealthStatus_ReturnsCurrentTimestamp()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetHealthStatus_StatusIsNotEmpty()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.Status.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetHealthStatus_ContainsDetails()
    {
        // Act
        var result = _healthService.GetHealthStatus();

        // Assert
        result.Details.Should().NotBeNull();
        result.Details.Should().ContainKey("uptime");
    }

    [Fact]
    public void GetHealthStatus_LogsDebugMessage()
    {
        // Act
        _healthService.GetHealthStatus();

        // Assert
        _logger.Received(1).LogDebug("Health check requested");
    }
}
