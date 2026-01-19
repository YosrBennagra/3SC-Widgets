using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace _3SC.Widgets.DesktopPet.Helpers;

/// <summary>
/// Converts a boolean value to a Visibility value.
/// true = Visible, false = Collapsed
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance for use in XAML via x:Static.
    /// </summary>
    public static readonly BooleanToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Check if we should invert
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            if (invert)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
