using MedocIntegration.DesktopUI.ViewModels;
using MedocIntegration.DesktopUI.Views.Pages;
using System.Windows;

namespace MedocIntegration.DesktopUI.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Ініціалізує головне вікно, отримує сторінки через DI
    /// та встановлює їх як вміст відповідних вкладок
    /// </summary>
    public MainWindow(MainViewModel viewModel, DashboardPage dashboardPage, SettingsPage settingsPage)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Встановлюємо сторінки як вміст вкладок програмно
        // щоб DI коректно передав залежності у конструктори сторінок
        DashboardTab.Content = dashboardPage;
        SettingsTab.Content = settingsPage;
    }
}
