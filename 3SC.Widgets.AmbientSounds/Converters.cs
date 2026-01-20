using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace _3SC.Widgets.AmbientSounds;

/// <summary>
/// Converts boolean to play/pause icon.
/// </summary>
public class BoolToPlayPauseConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPlaying)
        {
            return isPlaying ? "⏸️" : "▶️";
        }
        return "▶️";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}

/// <summary>
/// Converts a percentage to a width.
/// </summary>
public class PercentToWidthConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            double maxWidth = 180;
            if (parameter is string strParam && double.TryParse(strParam, out double parsed))
            {
                maxWidth = parsed;
            }
            return percent * maxWidth;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
