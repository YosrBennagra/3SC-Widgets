using System.Windows;
using System.Windows.Controls;
using _3SC.Widgets.Contracts;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace _3SC.Widgets.Clock;

/// <summary>
/// Factory for creating Clock widget instances.
/// This class is discovered by the 3SC app via reflection.
/// </summary>
public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ClockWidget();
    }
}

/// <summary>
/// The Clock widget implementation that conforms to 3SC's IWidget interface.
/// Uses its own window (ClockWidgetWindow) with full functionality.
/// </summary>
public class ClockWidget : IWidget
{
    private ClockWidgetWindow? _window;

    public string WidgetKey => "clock";
    public string DisplayName => "Clock";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        // Create the original ClockWidgetWindow with all its functionality
        _window = new ClockWidgetWindow();
        return _window;
    }

    public WpfUserControl GetView()
    {
        // Not used since HasOwnWindow is true
        throw new NotSupportedException("This widget provides its own window. Use CreateWindow() instead.");
    }

    public void OnInitialize()
    {
        // The window handles its own initialization
    }

    public void OnDispose()
    {
        // The window handles its own disposal when closed
        _window = null;
    }

    public void ShowSettings()
    {
        // Settings are accessed via the window's context menu
    }
}
