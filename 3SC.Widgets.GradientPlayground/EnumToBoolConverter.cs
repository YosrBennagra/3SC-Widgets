using System;
using System.Globalization;
using System.Windows.Data;
using _3SC.Widgets.GradientPlayground.Models;

namespace _3SC.Widgets.GradientPlayground;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            return Enum.Parse(typeof(GradientType), parameter.ToString()!);
        }
        return Binding.DoNothing;
    }
}
