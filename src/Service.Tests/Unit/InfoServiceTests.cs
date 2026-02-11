using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using MedocIntegration.Service.Services;
using MedocIntegration.Common.Models;

namespace MedocIntegration.Service.Tests.Unit;

public class InfoServiceTests
{
    private readonly IHostEnvironment _mockEnvironment;
    private readonly ILogger<InfoService> _logger;

    public InfoServiceTests()
    {
        _mockEnvironment = Substitute.For<IHostEnvironment>();
        _logger = Substitute.For<ILogger<InfoService>>();
    }

    [Fact]
    public void GetServiceInfo_ReturnsCorrectServiceName()
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns("Testing");
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Should().NotBeNull();
        result.Service.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetServiceInfo_ReturnsVersion()
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns("Development");
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetServiceInfo_ReturnsEnvironmentName()
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns("Production");
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Environment.Should().Be("Production");
    }

    [Fact]
    public void GetServiceInfo_ReturnsHostName()
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns("Development");
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.HostName.Should().NotBeNullOrEmpty();
        result.HostName.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void GetServiceInfo_ReturnsStartedAt()
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns("Development");
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.StartedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void GetServiceInfo_WorksWithDifferentEnvironments(string environmentName)
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns(environmentName);
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Environment.Should().Be(environmentName);
    }

    [Fact]
    public void GetServiceInfo_CallsLogger()
    {
        // Arrange
        _mockEnvironment.EnvironmentName.Returns("Development");
        var infoService = new InfoService(_mockEnvironment, _logger);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        // Перевіряємо що logger викликався (будь-який рівень логування)
        _logger.ReceivedWithAnyArgs(2).Log(
            default,
            default,
            default!,
            default,
            default!
        );
    }
}
