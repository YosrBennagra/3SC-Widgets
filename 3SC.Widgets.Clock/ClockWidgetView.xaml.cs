using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace _3SC.Widgets.Clock;

/// <summary>
/// UserControl view for the Clock widget.
/// This is the visual element hosted by CommunityWidgetWindow.
/// </summary>
public partial class ClockWidgetView : System.Windows.Controls.UserControl
{
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
    private bool _use24HourFormat = false;
    private bool _showSeconds = true;
    private bool _showTimeZoneLabel = true;

    public ClockWidgetView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Updates the time display with settings.
    /// Called by the widget's timer.
    /// </summary>
    public void UpdateTime()
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, _timeZone);

        string format = _use24HourFormat
            ? (_showSeconds ? "HH:mm:ss" : "HH:mm")
            : (_showSeconds ? "hh:mm:ss tt" : "hh:mm tt");

        TimeTextBlock.Text = now.ToString(format, CultureInfo.InvariantCulture);

        // Update timezone label visibility and text
        if (_showTimeZoneLabel)
        {
            TimeZoneLabel.Text = _timeZone.StandardName;
            TimeZoneLabel.Visibility = Visibility.Visible;
        }
        else
        {
            TimeZoneLabel.Visibility = Visibility.Collapsed;
        }
    }

    public void ApplySettings(TimeZoneInfo timeZone, bool use24HourFormat, bool showSeconds, bool showTimeZoneLabel)
    {
        _timeZone = timeZone;
        _use24HourFormat = use24HourFormat;
        _showSeconds = showSeconds;
        _showTimeZoneLabel = showTimeZoneLabel;
        UpdateTime();
    }
}
