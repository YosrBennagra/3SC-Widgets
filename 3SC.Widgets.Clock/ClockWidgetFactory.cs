using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Clock;

/// <summary>
/// Factory for creating Clock widget instances.
/// This class is discovered by the 3SC app via reflection.
/// </summary>
public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ClockWindowWidget();
    }
}

/// <summary>
/// The Clock widget implementation that conforms to 3SC's IWidget interface.
/// Uses its own window (ClockWidgetWindow) with full functionality.
/// </summary>
public class ClockWindowWidget : IWidget
{
    private ClockWidgetWindow? _window;

    public string WidgetKey => "clock";
    public string DisplayName => "Clock";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new ClockWidgetWindow();
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
        _window = null;
    }

    public void ShowSettings()
    {
        // Settings accessed via window's context menu
    }
}
