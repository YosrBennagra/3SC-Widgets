using _3SC.Widgets.Contracts;
using Serilog;

namespace _3SC.Widgets.QuickLinks;

public class QuickLinksWidgetFactory : IWidgetFactory
{
    private readonly ILogger _logger = Log.ForContext<QuickLinksWidgetFactory>();

    public IWidget CreateWidget()
    {
        _logger.Information("Creating QuickLinksWidget instance");
        return new QuickLinksWidgetAdapter();
    }
}

internal class QuickLinksWidgetAdapter : IWidget
{
    private readonly ILogger _logger = Log.ForContext<QuickLinksWidgetAdapter>();
    private QuickLinksWidget? _window;

    public string WidgetKey => "quicklinks";
    public string DisplayName => "Quick Links";
    public string Version => "1.1.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public System.Windows.Window? CreateWindow()
    {
        _logger.Information("Creating QuickLinksWidget window");
        _window = new _3SC.Widgets.QuickLinks.QuickLinksWidget();
        return _window;
    }

    public System.Windows.Controls.UserControl GetView()
    {
        _logger.Warning("GetView called but QuickLinks uses its own window");
        throw new System.NotSupportedException("This widget provides its own window.");
    }

    public void OnInitialize()
    {
        _logger.Debug("OnInitialize called - window handles its own initialization");
    }

    public void OnDispose()
    {
        _logger.Information("Disposing QuickLinksWidget");
        _window = null;
    }

    public void ShowSettings()
    {
        _logger.Information("ShowSettings called - no settings available");
    }
}
