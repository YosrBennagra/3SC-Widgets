using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.QuickLinks;

public class QuickLinksWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new QuickLinksWidgetAdapter();
    }
}

internal class QuickLinksWidgetAdapter : IWidget
{
    private QuickLinksWidget? _window;

    public string WidgetKey => "quicklinks";
    public string DisplayName => "Quick Links";
    public string Version => "1.0.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public System.Windows.Window? CreateWindow()
    {
        _window = new _3SC.Widgets.QuickLinksWidget();
        return _window;
    }

    public System.Windows.Controls.UserControl GetView()
    {
        throw new System.NotSupportedException("This widget provides its own window.");
    }

    public void OnInitialize()
    {
        // Window handles its own initialization
    }

    public void OnDispose()
    {
        _window = null;
    }

    public void ShowSettings()
    {
        // No settings for QuickLinks
    }
}
