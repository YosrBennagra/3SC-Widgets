using System;
using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.VideoViewer;

public class VideoViewerWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new VideoViewerWidget();
    }
}

public class VideoViewerWidget : IWidget
{
    private VideoViewerWindow? _window;
    private VideoWidgetViewModel? _viewModel;

    public string WidgetKey => "video-viewer";
    public string DisplayName => "Video Viewer";
    public string Version => "1.0.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => false;

    public Window? CreateWindow()
    {
        _window = new VideoViewerWindow();
        _viewModel = (_window.DataContext as VideoWidgetViewModel)!;
        return _window;
    }

    public System.Windows.Controls.UserControl GetView()
    {
        return null!;
    }

    public void OnInitialize()
    {
        _viewModel?.OnInitialize();
    }

    public void OnDispose()
    {
        _viewModel?.OnDispose();
        _window?.Close();
        _window = null;
        _viewModel = null;
    }

    public void ShowSettings()
    {
        // No settings for this widget
    }
}
