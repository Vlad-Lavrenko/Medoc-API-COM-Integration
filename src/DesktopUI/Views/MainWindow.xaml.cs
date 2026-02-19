using MedocIntegration.DesktopUI.ViewModels;
using System.Windows;

namespace MedocIntegration.DesktopUI.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Ініціалізує головне вікно та прив'язує ViewModel
    /// </summary>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
