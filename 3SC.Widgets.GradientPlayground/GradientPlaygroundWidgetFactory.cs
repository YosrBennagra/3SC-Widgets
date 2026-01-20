using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.GradientPlayground;

[Widget("gradient-playground", "Gradient Playground")]
public class GradientPlaygroundWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new GradientPlaygroundWidgetImpl();
    }
}

[Widget("gradient-playground", "Gradient Playground")]
public class GradientPlaygroundWidgetImpl : IWidget
{
    private GradientPlaygroundWindow? _window;

    public string WidgetKey => "gradient-playground";
    public string DisplayName => "Gradient Playground";
    public string Version => "1.0.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new GradientPlaygroundWindow();
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
