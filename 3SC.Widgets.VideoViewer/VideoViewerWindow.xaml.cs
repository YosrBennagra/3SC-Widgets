using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace _3SC.Widgets.VideoViewer;

public partial class VideoViewerWindow : Window
{
    private readonly VideoWidgetViewModel _viewModel;
    private bool _isDragging;
    private Point _clickPosition;

    public VideoViewerWindow()
    {
        InitializeComponent();

        _viewModel = new VideoWidgetViewModel();
        DataContext = _viewModel;

        // Connect ViewModel events to MediaElement
        _viewModel.PlayRequested += () => VideoPlayer?.Play();
        _viewModel.PauseRequested += () => VideoPlayer?.Pause();
        _viewModel.StopRequested += () => VideoPlayer?.Stop();
        _viewModel.SeekRequested += (position) =>
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Position = position;
            }
        };

        // Update position periodically during playback
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        timer.Tick += (s, e) =>
        {
            if (_viewModel.IsPlaying && VideoPlayer != null)
            {
                _viewModel.UpdatePosition(VideoPlayer.Position);
            }
        };
        timer.Start();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnInitialize();
    }

    private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        if (VideoPlayer?.NaturalDuration.HasTimeSpan == true)
        {
            _viewModel.SetDuration(VideoPlayer.NaturalDuration.TimeSpan);
        }
    }

    private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        _viewModel.NotifyPlaybackEnded();
    }

    private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Slider slider)
        {
            // Calculate the position based on mouse click
            var mousePosition = e.GetPosition(slider);
            var percentage = mousePosition.X / slider.ActualWidth;
            var newValue = percentage * slider.Maximum;
            slider.Value = newValue;

            // Seek to the new position
            _viewModel.Seek(newValue);

            e.Handled = true;
        }
    }

    private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Only seek if user is dragging (not during normal playback updates)
        if (sender is Slider slider && slider.IsMouseCaptureWithin)
        {
            _viewModel.Seek(e.NewValue);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(this);
            (sender as Border)?.CaptureMouse();
        }
    }

    private void Border_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPosition = e.GetPosition(this);
            var offset = currentPosition - _clickPosition;
            Left += offset.X;
            Top += offset.Y;
        }
    }

    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            (sender as Border)?.ReleaseMouseCapture();
        }
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        // Handled by DropFileBehavior
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
        // Handled by DropFileBehavior
    }

    private void ProgressSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Handled in PreviewMouseLeftButtonDown
    }
}
