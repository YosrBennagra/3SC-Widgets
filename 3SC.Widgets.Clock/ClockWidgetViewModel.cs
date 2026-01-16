using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using _3SC.Domain.ValueObjects;
using Serilog;

namespace _3SC.Widgets.Clock;

/// <summary>
/// ViewModel for the Clock widget following MVVM pattern with CommunityToolkit.MVVM.
/// Handles time display, settings management, and lifecycle.
/// </summary>
public partial class ClockWidgetViewModel : ObservableObject, IDisposable
{
    #region Fields

    private static readonly ILogger Log = Serilog.Log.ForContext<ClockWidgetViewModel>();
    private readonly DispatcherTimer _timer;
    private readonly string _settingsPath;
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
    private bool _isDisposed;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Current time display string.
    /// </summary>
    [ObservableProperty]
    private string _timeDisplay = "00:00:00";

    /// <summary>
    /// Current date display string.
    /// </summary>
    [ObservableProperty]
    private string _dateDisplay = "Monday, January 1, 2026";

    /// <summary>
    /// Time zone label display.
    /// </summary>
    [ObservableProperty]
    private string _timeZoneLabel = "Local Time";

    /// <summary>
    /// Whether to show the time zone label.
    /// </summary>
    [ObservableProperty]
    private bool _showTimeZoneLabel = true;

    /// <summary>
    /// Whether to use 24-hour format.
    /// </summary>
    [ObservableProperty]
    private bool _use24HourFormat = true;

    /// <summary>
    /// Whether to show seconds in time display.
    /// </summary>
    [ObservableProperty]
    private bool _showSeconds = true;

    /// <summary>
    /// Error message to display, if any.
    /// </summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Whether an error is currently displayed.
    /// </summary>
    [ObservableProperty]
    private bool _hasError;

    #endregion

    #region Constructor

    public ClockWidgetViewModel()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", "clock", "settings.json");

        // Setup timer for clock updates (500ms for smooth seconds transitions)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += OnTimerTick;

        Log.Debug("ClockWidgetViewModel created");
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initialize the ViewModel. Call after construction.
    /// </summary>
    public void Initialize()
    {
        try
        {
            LoadSettings();
            _timer.Start();
            UpdateTime();
            Log.Information("Clock widget initialized with timezone {TimeZone}", _timeZone.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize clock widget");
            SetError("Failed to initialize clock");
        }
    }

    /// <summary>
    /// Clean up resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _timer.Stop();
        _timer.Tick -= OnTimerTick;

        Log.Debug("ClockWidgetViewModel disposed");
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to clear any displayed error.
    /// </summary>
    [RelayCommand]
    private void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    #endregion

    #region Settings

    /// <summary>
    /// Apply new settings to the clock.
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
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
            Use24HourFormat = settings.Use24HourFormat;
            ShowSeconds = settings.ShowSeconds;
            ShowTimeZoneLabel = settings.ShowTimeZoneLabel;

            TimeZoneLabel = _timeZone.StandardName;
            UpdateTime();

            SaveSettings(settings);
            Log.Information("Settings applied: TimeZone={TimeZone}, 24Hour={Use24Hour}, Seconds={ShowSeconds}",
                settings.TimeZoneId, settings.Use24HourFormat, settings.ShowSeconds);
        }
        catch (TimeZoneNotFoundException ex)
        {
            Log.Error(ex, "Invalid timezone ID: {TimeZoneId}", settings.TimeZoneId);
            SetError($"Invalid timezone: {settings.TimeZoneId}");
            _timeZone = TimeZoneInfo.Local;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to apply settings");
            SetError("Failed to apply settings");
        }
    }

    /// <summary>
    /// Get current settings state.
    /// </summary>
    public ClockWidgetSettings GetCurrentSettings()
    {
        return new ClockWidgetSettings(
            _timeZone.Id,
            Use24HourFormat,
            ShowSeconds,
            ShowTimeZoneLabel);
    }

    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                Log.Debug("No settings file found at {Path}, using defaults", _settingsPath);
                ApplySettings(ClockWidgetSettings.Default());
                return;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<ClockWidgetSettings>(json);

            if (settings != null)
            {
                ApplySettings(settings);
                Log.Debug("Settings loaded from {Path}", _settingsPath);
            }
            else
            {
                ApplySettings(ClockWidgetSettings.Default());
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load settings from {Path}", _settingsPath);
            ApplySettings(ClockWidgetSettings.Default());
        }
    }

    private void SaveSettings(ClockWidgetSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_settingsPath, json);
            Log.Debug("Settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save settings to {Path}", _settingsPath);
        }
    }

    #endregion

    #region Private Methods

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        try
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, _timeZone);

            // Build time format
            string timeFormat = Use24HourFormat
                ? (ShowSeconds ? "HH:mm:ss" : "HH:mm")
                : (ShowSeconds ? "hh:mm:ss tt" : "hh:mm tt");

            TimeDisplay = now.ToString(timeFormat, CultureInfo.InvariantCulture);
            DateDisplay = now.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update time display");
            TimeDisplay = "--:--:--";
            DateDisplay = "Error";
        }
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    #endregion
}
