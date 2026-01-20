using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.LogoSizeTester;

[Widget("logo-size-tester", "Logo Size Tester")]
public class LogoSizeTesterWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new LogoSizeTesterWidgetImpl();
    }
}

[Widget("logo-size-tester", "Logo Size Tester")]
public class LogoSizeTesterWidgetImpl : IWidget
{
    private LogoSizeTesterWindow? _window;

    public string WidgetKey => "logo-size-tester";
    public string DisplayName => "Logo Size Tester";
    public string Version => "1.0.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new LogoSizeTesterWindow();
        return _window;
    }

    public System.Windows.Controls.UserControl GetView()
    {
        throw new NotSupportedException("This widget provides its own window.");
    }

    public void OnInitialize()
    {
    }

    public void OnDispose()
    {
        _window?.Close();
        _window = null;
    }

    public void ShowSettings()
    {
    }

    public void OnSettingsChanged(string settingsJson)
    {
    }
}
