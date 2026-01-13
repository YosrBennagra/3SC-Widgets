using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using _3SC.Domain.ValueObjects;
using _3SC.Data;

namespace _3SC.Widgets.Clock
{
    /// <summary>
    /// Standalone settings window for external Clock widget
    /// </summary>
    public partial class ClockSettingsWindow : Window
    {
        private readonly DispatcherTimer _previewTimer;
        private ClockWidgetSettings? _currentSettings;

        public ClockWidgetSettings? UpdatedSettings { get; private set; }

        public ObservableCollection<TimeZoneDisplay> AvailableTimeZones { get; set; }
        public TimeZoneDisplay? SelectedTimeZone { get; set; }
        public bool Use24HourFormat { get; set; } = true;
        public bool ShowSeconds { get; set; } = true;
        public bool ShowTimeZoneLabel { get; set; } = true;

        public ClockSettingsWindow(ClockWidgetSettings? currentSettings)
        {
            InitializeComponent();
            
            _currentSettings = currentSettings ?? ClockWidgetSettings.Default();
            
            // Load available timezones
            AvailableTimeZones = new ObservableCollection<TimeZoneDisplay>(
                CommonTimeZones.GetAllTimeZones());

            // Load current settings
            Use24HourFormat = _currentSettings.Use24HourFormat;
            ShowSeconds = _currentSettings.ShowSeconds;
            ShowTimeZoneLabel = _currentSettings.ShowTimeZoneLabel;
            SelectedTimeZone = AvailableTimeZones.FirstOrDefault(tz => tz.Id == _currentSettings.TimeZoneId)
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
            catch
            {
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
                
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _previewTimer?.Stop();
            base.OnClosing(e);
        }
    }
}
