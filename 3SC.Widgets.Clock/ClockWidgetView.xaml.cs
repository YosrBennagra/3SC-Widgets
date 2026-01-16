using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using _3SC.Domain.ValueObjects;
using Serilog;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace _3SC.Widgets.Clock;

/// <summary>
/// Clock widget view with MVVM ViewModel binding.
/// Displays time, date, and timezone information with smooth animations.
/// </summary>
public partial class ClockWidgetView : WpfUserControl, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ClockWidgetView>();
    private readonly DispatcherTimer _timer;
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
    private bool _use24HourFormat = true;
    private bool _showSeconds = true;
    private bool _showTimeZoneLabel = true;
    private ClockWidgetSettings? _currentSettings;
    private bool _isDisposed;

    // Font size scaling constants (match internal widget)
    private const double BaseFontSize = 42.0;
    private const double BaseWidth = 300.0;
    private const double MinFontSize = 18.0;
    private const double MaxFontSize = 80.0;

    public ClockWidgetView()
    {
        InitializeComponent();

        _currentSettings = ClockWidgetSettings.Default();
        ApplySettings(_currentSettings);

        // Start timer with 500ms interval for smooth seconds display
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Loaded += ClockWidgetView_Loaded;
        Unloaded += ClockWidgetView_Unloaded;

        Log.Debug("ClockWidgetView created");
    }

    private void ClockWidgetView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateTime();
        Log.Debug("ClockWidgetView loaded");
    }

    private void ClockWidgetView_Unloaded(object sender, RoutedEventArgs e)
    {
        // Stop timer when control is unloaded to save resources
        _timer.Stop();
        Log.Debug("ClockWidgetView unloaded, timer stopped");
    }

    /// <summary>
    /// Apply settings to the clock display.
    /// </summary>
    public void ApplySettings(ClockWidgetSettings settings)
    {
        if (settings == null)
        {
            Log.Warning("Attempted to apply null settings");
            return;
        }

        try
        {
            _currentSettings = settings;
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
            _use24HourFormat = settings.Use24HourFormat;
            _showSeconds = settings.ShowSeconds;
            _showTimeZoneLabel = settings.ShowTimeZoneLabel;

            UpdateTime();

            // Update timezone label visibility and text
            if (_showTimeZoneLabel)
            {
                TimeZoneLabel.Text = _timeZone.StandardName;
                TimeZonePill.Visibility = Visibility.Visible;
            }
            else
            {
                TimeZonePill.Visibility = Visibility.Collapsed;
            }

            // Always show date
            DateTextBlock.Visibility = Visibility.Visible;

            // Hide any error
            ErrorOverlay.Visibility = Visibility.Collapsed;

            Log.Information("Settings applied: TimeZone={TimeZone}, 24Hour={Use24Hour}, Seconds={ShowSeconds}",
                settings.TimeZoneId, settings.Use24HourFormat, settings.ShowSeconds);
        }
        catch (TimeZoneNotFoundException ex)
        {
            Log.Error(ex, "Invalid timezone: {TimeZoneId}", settings.TimeZoneId);
            ShowError($"Invalid timezone: {settings.TimeZoneId}");
            _timeZone = TimeZoneInfo.Local;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to apply settings");
            ShowError("Failed to apply settings");
        }
    }

    /// <summary>
    /// Get current settings state.
    /// </summary>
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
        try
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, _timeZone);

            // Build time format based on settings
            string timeFormat = _use24HourFormat
                ? (_showSeconds ? "HH:mm:ss" : "HH:mm")
                : (_showSeconds ? "hh:mm:ss tt" : "hh:mm tt");

            TimeTextBlock.Text = now.ToString(timeFormat, CultureInfo.InvariantCulture);
            DateTextBlock.Text = now.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update time");
            TimeTextBlock.Text = "--:--:--";
            DateTextBlock.Text = "Error";
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorOverlay.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Update the font size based on the widget width.
    /// Scales proportionally like the internal ClockWidget.
    /// </summary>
    public void UpdateFontSize(double widgetWidth)
    {
        var scale = widgetWidth / BaseWidth;
        var newSize = BaseFontSize * scale;

        // Clamp to min/max bounds
        if (newSize < MinFontSize)
            newSize = MinFontSize;
        if (newSize > MaxFontSize)
            newSize = MaxFontSize;

        TimeTextBlock.FontSize = newSize;

        // Also scale the date and timezone proportionally
        DateTextBlock.FontSize = Math.Max(10, 13 * scale);
        TimeZoneLabel.FontSize = Math.Max(9, 11 * scale);
    }

    /// <summary>
    /// Clean up resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _timer.Stop();
        _timer.Tick -= Timer_Tick;

        Log.Debug("ClockWidgetView disposed");
    }
}
