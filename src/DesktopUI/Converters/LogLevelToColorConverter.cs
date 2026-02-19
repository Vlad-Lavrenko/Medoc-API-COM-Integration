using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MedocIntegration.DesktopUI.Converters;

public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string level)
            return new SolidColorBrush(Colors.Gray);

        return level.ToUpperInvariant() switch
        {
            "VRB" => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // Сірий
            "DBG" => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Синій
            "INF" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Зелений
            "WRN" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Оранжевий
            "ERR" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),  // Червоний
            "FTL" => new SolidColorBrush(Color.FromRgb(136, 14, 79)),  // Темно-червоний
            _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))  // Сірий (default)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
