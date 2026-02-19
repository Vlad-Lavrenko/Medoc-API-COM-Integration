using System.ServiceProcess;
using MedocIntegration.DesktopUI.Models;

namespace MedocIntegration.DesktopUI.Services;

public class WindowsServiceController : IServiceController
{
    private const string ServiceName = "MedocIntegrationService";
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    public bool ServiceExists()
    {
        try
        {
            using var service = new ServiceController(ServiceName);
            _ = service.Status; // Перевірка що служба існує
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ServiceStatus> GetStatusAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!ServiceExists())
                {
                    return new ServiceStatus
                    {
                        Status = ServiceControllerStatus.Stopped,
                        DisplayStatus = "Служба не встановлена",
                        CanStart = false,
                        CanStop = false
                    };
                }

                using var service = new ServiceController(ServiceName);
                service.Refresh();

                return new ServiceStatus
                {
                    Status = service.Status,
                    DisplayStatus = GetDisplayStatus(service.Status),
                    CanStart = service.Status == ServiceControllerStatus.Stopped,
                    CanStop = service.Status == ServiceControllerStatus.Running
                };
            }
            catch (Exception ex)
            {
                return new ServiceStatus
                {
                    Status = ServiceControllerStatus.Stopped,
                    DisplayStatus = $"Помилка: {ex.Message}",
                    CanStart = false,
                    CanStop = false
                };
            }
        });
    }

    public async Task StartAsync()
    {
        await Task.Run(() =>
        {
            using var service = new ServiceController(ServiceName);

            if (service.Status == ServiceControllerStatus.Running)
                return;

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, _timeout);
        });
    }

    public async Task StopAsync()
    {
        await Task.Run(() =>
        {
            using var service = new ServiceController(ServiceName);

            if (service.Status == ServiceControllerStatus.Stopped)
                return;

            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, _timeout);
        });
    }

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(1500);
        await StartAsync();
    }

    private static string GetDisplayStatus(ServiceControllerStatus status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => "Запущена",
            ServiceControllerStatus.Stopped => "Зупинена",
            ServiceControllerStatus.Paused => "Призупинена",
            ServiceControllerStatus.StartPending => "Запускається...",
            ServiceControllerStatus.StopPending => "Зупиняється...",
            ServiceControllerStatus.PausePending => "Призупиняється...",
            ServiceControllerStatus.ContinuePending => "Відновлюється...",
            _ => "Невідомо"
        };
    }
}
