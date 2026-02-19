using MedocIntegration.DesktopUI.ViewModels;
using System.Windows.Controls;

namespace MedocIntegration.DesktopUI.Views.Pages;

public partial class DashboardPage : UserControl
{
    /// <summary>
    /// Ініціалізує сторінку Dashboard та прив'язує ViewModel
    /// </summary>
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
