using System.Globalization;
using System.Windows.Data;

namespace _3SC.Widgets.SystemPulse.Helpers;

/// <summary>
/// Converts a percentage (0-100) to a decimal (0-1) for transforms.
/// </summary>
public class PercentToDecimalConverter : IValueConverter
{
    public static readonly PercentToDecimalConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            return percent / 100.0;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a percentage to a width based on container width.
/// </summary>
public class PercentToWidthConverter : IValueConverter
{
    public static readonly PercentToWidthConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            var maxWidth = 60.0; // Default max width
            if (parameter is double max) maxWidth = max;
            return (percent / 100.0) * maxWidth;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
