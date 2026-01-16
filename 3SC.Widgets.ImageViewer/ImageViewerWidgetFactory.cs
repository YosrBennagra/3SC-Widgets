using _3SC.Widgets.Contracts;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace _3SC.Widgets.ImageViewer;

public class ImageViewerWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ImageViewerWidget();
    }
}

public class ImageViewerWidget : IWidget
{
    private ImageViewerWindow? _window;

    public string WidgetKey => "image-viewer";
    public string DisplayName => "Image Viewer";
    public string Version => "1.0.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new ImageViewerWindow();
        return _window;
    }

    public UserControl GetView()
    {
        throw new System.NotImplementedException("Image Viewer uses its own window");
    }

    public void OnInitialize()
    {
        // Initialization handled by the window
    }

    public void OnDispose()
    {
        _window?.Close();
    }

    public void ShowSettings()
    {
        // Settings not implemented yet
        System.Windows.MessageBox.Show("Settings not yet available", "Image Viewer", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}
