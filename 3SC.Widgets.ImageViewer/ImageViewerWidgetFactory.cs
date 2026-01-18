using _3SC.Widgets.Contracts;
using System;
using System.Windows;
using Serilog;
using UserControl = System.Windows.Controls.UserControl;

namespace _3SC.Widgets.ImageViewer;

[Widget("image-viewer", "Image Viewer")]
public class ImageViewerWidgetFactory : IWidgetFactory
{
    private readonly ILogger _logger = Log.ForContext<ImageViewerWidgetFactory>();

    public IWidget CreateWidget()
    {
        _logger.Information("Creating ImageViewerWidget instance");
        return new ImageViewerWidget();
    }
}

[Widget("image-viewer", "Image Viewer")]
public class ImageViewerWidget : IWidget
{
    private readonly ILogger _logger = Log.ForContext<ImageViewerWidget>();
    private ImageViewerWindow? _window;

    public string WidgetKey => "image-viewer";
    public string DisplayName => "Image Viewer";
    public string Version => "1.1.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _logger.Information("Creating ImageViewerWindow");
        _window = new ImageViewerWindow();
        return _window;
    }

    public UserControl GetView()
    {
        _logger.Warning("GetView called but ImageViewer uses its own window");
        throw new System.NotImplementedException("Image Viewer uses its own window");
    }

    public void OnInitialize()
    {
        _logger.Debug("OnInitialize called - initialization handled by window");
    }

    public void OnDispose()
    {
        _logger.Information("Disposing ImageViewerWidget");
        _window?.Close();
    }

    public void ShowSettings()
    {
        _logger.Information("ShowSettings called - not yet implemented");
        System.Windows.MessageBox.Show("Settings not yet available", "Image Viewer", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}
