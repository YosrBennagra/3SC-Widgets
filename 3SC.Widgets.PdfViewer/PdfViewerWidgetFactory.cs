using _3SC.Widgets.Contracts;
using System.Windows;
using System.Windows.Controls;

namespace _3SC.Widgets.PdfViewer;

public class PdfViewerWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new PdfViewerWidget();
    }
}

public class PdfViewerWidget : IWidget
{
    private PdfViewerWindow? _window;

    public string WidgetKey => "pdf-viewer";
    public string DisplayName => "PDF Viewer";
    public string Version => "1.0.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new PdfViewerWindow();
        return _window;
    }

    public UserControl GetView()
    {
        throw new System.NotImplementedException("PDF Viewer uses its own window");
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
        System.Windows.MessageBox.Show("Settings not yet available", "PDF Viewer", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}
