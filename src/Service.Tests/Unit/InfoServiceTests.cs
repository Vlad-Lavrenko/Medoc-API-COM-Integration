using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Moq;
using Service.Services;

namespace Service.Tests.Unit;

public class InfoServiceTests
{
    [Fact]
    public void GetServiceInfo_ReturnsCorrectServiceName()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Testing");
        var infoService = new InfoService(mockEnvironment.Object);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Should().NotBeNull();
        result.Service.Should().Be("Service");
    }

    [Fact]
    public void GetServiceInfo_ReturnsVersion()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        var infoService = new InfoService(mockEnvironment.Object);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Version.Should().NotBeNullOrEmpty();
        result.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void GetServiceInfo_ReturnsEnvironmentName()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        var infoService = new InfoService(mockEnvironment.Object);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Environment.Should().Be("Production");
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void GetServiceInfo_WorksWithDifferentEnvironments(string environmentName)
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns(environmentName);
        var infoService = new InfoService(mockEnvironment.Object);

        // Act
        var result = infoService.GetServiceInfo();

        // Assert
        result.Environment.Should().Be(environmentName);
    }
}
