using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Breathe;

[Widget("breathe", "Breathe")]
public class BreatheWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new BreatheWidgetImpl();
    }
}

[Widget("breathe", "Breathe")]
public class BreatheWidgetImpl : IWidget
{
    private BreatheWindow? _window;

    public string WidgetKey => "breathe";
    public string DisplayName => "Breathe";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new BreatheWindow();
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

    public void OnSettingsChanged(IDictionary<string, object> settings)
    {
    }
}
