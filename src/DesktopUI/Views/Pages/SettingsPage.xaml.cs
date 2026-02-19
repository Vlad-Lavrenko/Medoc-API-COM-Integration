using MedocIntegration.DesktopUI.ViewModels;
using System.Windows.Controls;

namespace MedocIntegration.DesktopUI.Views.Pages;

public partial class SettingsPage : UserControl
{
    /// <summary>
    /// Ініціалізує сторінку налаштувань та прив'язує ViewModel
    /// </summary>
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
