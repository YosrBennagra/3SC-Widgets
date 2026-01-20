using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.SystemPulse;

/// <summary>
/// Factory for creating System Pulse widget instances.
/// </summary>
[Widget("system-pulse", "System Pulse")]
public class SystemPulseWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new SystemPulseWidgetImpl();
    }
}

/// <summary>
/// System Pulse widget - Real-time system monitor with beautiful animations.
/// </summary>
[Widget("system-pulse", "System Pulse")]
public class SystemPulseWidgetImpl : IWidget
{
    private SystemPulseWindow? _window;

    public string WidgetKey => "system-pulse";
    public string DisplayName => "System Pulse";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new SystemPulseWindow();
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
