using CommunityToolkit.Mvvm.ComponentModel;

namespace MedocIntegration.DesktopUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    /// <summary>
    /// Заголовок головного вікна
    /// </summary>
    public string Title => "Medoc Integration Manager";
}
