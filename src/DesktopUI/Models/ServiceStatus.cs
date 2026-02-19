using System.ServiceProcess;

namespace MedocIntegration.DesktopUI.Models;

public class ServiceStatus
{
    public ServiceControllerStatus Status { get; init; }
    public string DisplayStatus { get; init; } = string.Empty;
    public bool CanStart { get; init; }
    public bool CanStop { get; init; }
}
