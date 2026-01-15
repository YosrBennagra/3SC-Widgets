using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using _3SC.Domain.ValueObjects;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace _3SC.Widgets.Clock;

public partial class ClockWidgetView : WpfUserControl, IDisposable
{
    private readonly DispatcherTimer _timer;
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
    private bool _use24HourFormat = true;
    private bool _showSeconds = true;
    private bool _showTimeZoneLabel = true;
    private ClockWidgetSettings? _currentSettings;

    public ClockWidgetView()
    {
        InitializeComponent();

        _currentSettings = ClockWidgetSettings.Default();
        ApplySettings(_currentSettings);

        // Start timer
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Loaded += ClockWidgetView_Loaded;
    }

    private void ClockWidgetView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateTime();
    }

    public void ApplySettings(ClockWidgetSettings settings)
    {
        _currentSettings = settings;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        _use24HourFormat = settings.Use24HourFormat;
        _showSeconds = settings.ShowSeconds;
        _showTimeZoneLabel = settings.ShowTimeZoneLabel;

        UpdateTime();

        // Update timezone label
        if (_showTimeZoneLabel)
        {
            TimeZoneLabel.Text = _timeZone.StandardName;
            TimeZoneLabel.Visibility = Visibility.Visible;
        }
        else
        {
            TimeZoneLabel.Visibility = Visibility.Collapsed;
        }

        // Show date by default
        DateTextBlock.Visibility = Visibility.Visible;
    }

    public ClockWidgetSettings GetCurrentSettings()
    {
        return _currentSettings ?? ClockWidgetSettings.Default();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, _timeZone);

        // Update time
        string timeFormat = _use24HourFormat
            ? (_showSeconds ? "HH:mm:ss" : "HH:mm")
            : (_showSeconds ? "hh:mm:ss tt" : "hh:mm tt");

        TimeTextBlock.Text = now.ToString(timeFormat, CultureInfo.InvariantCulture);

        // Update date (always shown)
        DateTextBlock.Text = now.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
    }

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }
}
