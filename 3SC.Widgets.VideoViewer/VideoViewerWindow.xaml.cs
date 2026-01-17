using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Serilog;

namespace _3SC.Widgets.VideoViewer;

public partial class VideoViewerWindow : WidgetWindowBase
{
    private readonly VideoWidgetViewModel _viewModel;
    private readonly ILogger _logger = Log.ForContext<VideoViewerWindow>();
    private readonly DispatcherTimer _timer;
    private bool _isSeeking;

    public VideoViewerWindow()
        : this(Guid.Empty, 0, 0, 500, 350, false)
    {
    }

    public VideoViewerWindow(Guid widgetInstanceId, double left, double top, double width, double height, bool isLocked)
    {
        _logger.Debug("VideoViewerWindow constructor called with InstanceId={InstanceId}", widgetInstanceId);

        InitializeComponent();

        InitializeWidgetWindow(
            new WidgetWindowInit(widgetInstanceId, left, top, width, height, isLocked),
            new WidgetWindowParts(
                LockWidgetMenuItem: LockWidgetMenuItem,
                ResizeToggleMenuItem: ResizeToggleMenuItem,
                ResizeOutlineElement: ResizeOutline,
                ResizeTopThumb: ResizeTop,
                ResizeBottomThumb: ResizeBottom,
                ResizeLeftThumb: ResizeLeft,
                ResizeRightThumb: ResizeRight,
                WidgetKey: "video"));

        _viewModel = new VideoWidgetViewModel();
        DataContext = _viewModel;

        // Timer for updating progress
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Loaded += OnLoaded;
        Closing += (s, e) =>
        {
            _timer.Stop();
            VideoPlayer.Stop();
            VideoPlayer.Close();
        };

        _logger.Information("VideoViewerWindow initialized successfully");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _logger.Debug("VideoViewerWindow loaded");
        _viewModel.OnInitialize();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (VideoPlayer.Source != null && VideoPlayer.NaturalDuration.HasTimeSpan && !_isSeeking)
        {
            var duration = VideoPlayer.NaturalDuration.TimeSpan;
            var position = VideoPlayer.Position;

            ProgressSlider.Maximum = duration.TotalSeconds;
            ProgressSlider.Value = position.TotalSeconds;

            TimeDisplay.Text = $"{FormatTime(position)} / {FormatTime(duration)}";
        }
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.Hours > 0
            ? $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}"
            : $"{time.Minutes}:{time.Seconds:D2}";
    }

    protected override bool IsDragBlocked(DependencyObject? source)
    {
        // Don't allow dragging when clicking on interactive controls
        return source is not null && IsInteractiveControl(source);
    }

    private static bool IsInteractiveControl(DependencyObject element)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current is System.Windows.Controls.Button or
                Slider or
                Thumb or
                System.Windows.Controls.ListBox or
                ListBoxItem or
                System.Windows.Controls.Primitives.ScrollBar or
                MediaElement)
            {
                return true;
            }

            current = current is Visual
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        _logger.Debug("Video media opened");
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
        {
            ProgressSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        }
        VideoPlayer.Volume = VolumeSlider.Value / 100.0;
    }

    private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        _logger.Debug("Video playback ended");
        VideoPlayer.Stop();
        PlayPauseIcon.Text = "\uE768"; // Play icon
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (VideoPlayer.Source == null)
        {
            _logger.Warning("No video loaded");
            return;
        }

        if (PlayPauseIcon.Text == "\uE768") // Play icon
        {
            VideoPlayer.Play();
            PlayPauseIcon.Text = "\uE769"; // Pause icon
            _logger.Debug("Video playback started");
        }
        else
        {
            VideoPlayer.Pause();
            PlayPauseIcon.Text = "\uE768"; // Play icon
            _logger.Debug("Video playback paused");
        }
    }

    private void ProgressSlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isSeeking = true;
    }

    private void ProgressSlider_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isSeeking = false;
        if (VideoPlayer.Source != null && VideoPlayer.NaturalDuration.HasTimeSpan)
        {
            VideoPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            _logger.Debug("Seeked to position: {Position}", ProgressSlider.Value);
        }
    }

    private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Handled by MouseUp event
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (VideoPlayer != null)
        {
            VideoPlayer.Volume = e.NewValue / 100.0;
        }
    }

    private void Fullscreen_Click(object sender, RoutedEventArgs e)
    {
        _logger.Information("Fullscreen requested (not implemented)");
        // TODO: Implement fullscreen mode
    }
}
