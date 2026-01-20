using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.AmbientSounds;

[Widget("ambient-sounds", "Ambient Sounds")]
public class AmbientSoundsWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new AmbientSoundsWidgetImpl();
    }
}

[Widget("ambient-sounds", "Ambient Sounds")]
public class AmbientSoundsWidgetImpl : IWidget
{
    private AmbientSoundsWindow? _window;

    public string WidgetKey => "ambient-sounds";
    public string DisplayName => "Ambient Sounds";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new AmbientSoundsWindow();
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
