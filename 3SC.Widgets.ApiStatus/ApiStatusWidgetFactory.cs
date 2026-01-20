using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.ApiStatus;

[Widget("apistatus", "API Status Monitor")]
public class ApiStatusWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ApiStatusWidget();
    }
}

[Widget("apistatus", "API Status Monitor")]
public class ApiStatusWidget : IWidget
{
    private ApiStatusWindow? _window;

    public string WidgetKey => "apistatus";
    public string DisplayName => "API Status Monitor";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new ApiStatusWindow();
        return _window;
    }

    public System.Windows.Controls.UserControl GetView()
    {
        throw new NotSupportedException("This widget provides its own window.");
    }

    public void OnInitialize()
    {
        // Window handles its own initialization
    }

    public void OnDispose()
    {
        _window?.Close();
        _window = null;
    }

    public void ShowSettings()
    {
        _window?.ShowSettingsDialog();
    }
}
