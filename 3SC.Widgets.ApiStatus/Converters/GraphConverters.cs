using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace _3SC.Widgets.ApiStatus;

public class ResponseTimeGraphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not List<ResponseTimeData> history || !history.Any())
        {
            return new System.Windows.Media.PointCollection();
        }

        var points = new System.Windows.Media.PointCollection();
        const double width = 400; // Approximate canvas width
        const double height = 52; // Canvas height minus margins
        const double padding = 4;

        var successfulData = history.Where(d => d.Success && d.ResponseTime > 0).ToList();
        if (!successfulData.Any())
        {
            return points;
        }

        var maxResponseTime = successfulData.Max(d => d.ResponseTime);
        var minResponseTime = successfulData.Min(d => d.ResponseTime);
        var range = maxResponseTime - minResponseTime;
        if (range < 1) range = maxResponseTime; // Prevent division by zero

        var stepX = width / Math.Max(1, history.Count - 1);

        for (int i = 0; i < history.Count; i++)
        {
            var data = history[i];
            if (!data.Success || data.ResponseTime <= 0)
            {
                continue;
            }

            var x = i * stepX;
            var normalizedValue = (data.ResponseTime - minResponseTime) / range;
            var y = height - (normalizedValue * (height - padding * 2)) - padding;

            points.Add(new Point(x, y));
        }

        return points;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EmptyVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToOnOffConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "ON" : "OFF";
        }
        return "OFF";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
