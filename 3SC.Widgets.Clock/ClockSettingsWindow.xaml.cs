using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using _3SC.Domain.ValueObjects;
using _3SC.Data;
using Serilog;

namespace _3SC.Widgets.Clock;

/// <summary>
/// Settings window for the Clock widget with live preview.
/// </summary>
public partial class ClockSettingsWindow : Window
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ClockSettingsWindow>();
    
    private readonly DispatcherTimer _previewTimer;
    private readonly ClockWidgetSettings _originalSettings;

    public ClockWidgetSettings? UpdatedSettings { get; private set; }

    public ObservableCollection<TimeZoneDisplay> AvailableTimeZones { get; set; }
    public TimeZoneDisplay? SelectedTimeZone { get; set; }
    public bool Use24HourFormat { get; set; } = true;
    public bool ShowSeconds { get; set; } = true;
    public bool ShowTimeZoneLabel { get; set; } = true;

    public ClockSettingsWindow(ClockWidgetSettings? currentSettings)
    {
        InitializeComponent();
        
        _originalSettings = currentSettings ?? ClockWidgetSettings.Default();
        
        // Load available timezones
        AvailableTimeZones = new ObservableCollection<TimeZoneDisplay>(
            CommonTimeZones.GetAllTimeZones());

        // Load current settings
        Use24HourFormat = _originalSettings.Use24HourFormat;
        ShowSeconds = _originalSettings.ShowSeconds;
        ShowTimeZoneLabel = _originalSettings.ShowTimeZoneLabel;
        SelectedTimeZone = AvailableTimeZones.FirstOrDefault(tz => tz.Id == _originalSettings.TimeZoneId)
            ?? AvailableTimeZones.FirstOrDefault(tz => tz.Id == TimeZoneInfo.Local.Id);

        DataContext = this;

        // Setup preview timer
        _previewTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _previewTimer.Tick += (_, _) => UpdatePreview();
        _previewTimer.Start();
        
        UpdatePreview();
        
        Log.Debug("ClockSettingsWindow opened with settings: TimeZone={TimeZone}", _originalSettings.TimeZoneId);
    }

    /// <summary>
    /// Allow dragging the window from title bar.
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void UpdatePreview()
    {
        if (SelectedTimeZone == null)
        {
            PreviewTimeTextBlock.Text = "--:--:--";
            PreviewZoneTextBlock.Text = "";
            return;
        }

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(SelectedTimeZone.Id);
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);

            string format = Use24HourFormat
                ? (ShowSeconds ? "HH:mm:ss" : "HH:mm")
                : (ShowSeconds ? "h:mm:ss tt" : "h:mm tt");

            PreviewTimeTextBlock.Text = now.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
            PreviewZoneTextBlock.Text = ShowTimeZoneLabel ? SelectedTimeZone.ShortName : "";
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to update preview for timezone {TimeZone}", SelectedTimeZone?.Id);
            PreviewTimeTextBlock.Text = "--:--:--";
            PreviewZoneTextBlock.Text = "Invalid";
        }
    }

    private void TimeZoneComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void Format24Hour_Checked(object sender, RoutedEventArgs e)
    {
        UpdatePreview();
    }

    private void ShowSeconds_Checked(object sender, RoutedEventArgs e)
    {
        UpdatePreview();
    }

    private void ShowTimeZone_Checked(object sender, RoutedEventArgs e)
    {
        UpdatePreview();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTimeZone != null)
        {
            UpdatedSettings = new ClockWidgetSettings(
                SelectedTimeZone.Id,
                Use24HourFormat,
                ShowSeconds,
                ShowTimeZoneLabel);
            
            Log.Information("Settings saved: TimeZone={TimeZone}, 24Hour={Use24Hour}, Seconds={ShowSeconds}, Label={ShowLabel}",
                UpdatedSettings.TimeZoneId, UpdatedSettings.Use24HourFormat, 
                UpdatedSettings.ShowSeconds, UpdatedSettings.ShowTimeZoneLabel);
            
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Log.Debug("Settings cancelled");
        DialogResult = false;
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _previewTimer?.Stop();
        base.OnClosing(e);
    }
}
