using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThreeSC.Widgets.Pomodoro
{
    public class TreeStageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stage && parameter is string rangeStr)
            {
                if (rangeStr.Contains("-"))
                {
                    // Range like "3-4"
                    var parts = rangeStr.Split('-');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int min) &&
                        int.TryParse(parts[1], out int max))
                    {
                        return stage >= min && stage <= max ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if (int.TryParse(rangeStr, out int exact))
                {
                    // Exact match like "0"
                    return stage == exact ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
