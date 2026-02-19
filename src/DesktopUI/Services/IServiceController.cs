using MedocIntegration.DesktopUI.Models;

namespace MedocIntegration.DesktopUI.Services;

public interface IServiceController
{
    Task<ServiceStatus> GetStatusAsync();
    Task StartAsync();
    Task StopAsync();
    Task RestartAsync();
    bool ServiceExists();
}
