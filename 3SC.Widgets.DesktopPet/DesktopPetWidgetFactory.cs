using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.DesktopPet;

/// <summary>
/// Factory for creating Desktop Pet widget instances.
/// This class is discovered by the 3SC app via reflection.
/// </summary>
[Widget("desktop-pet", "Desktop Pet")]
public class DesktopPetWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new DesktopPetWidget();
    }
}

/// <summary>
/// The Desktop Pet widget implementation that conforms to 3SC's IWidget interface.
/// Creates an adorable interactive pet that lives on your desktop!
/// </summary>
[Widget("desktop-pet", "Desktop Pet")]
public class DesktopPetWidget : IWidget
{
    private DesktopPetWindow? _window;

    public string WidgetKey => "desktop-pet";
    public string DisplayName => "Desktop Pet";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new DesktopPetWindow();
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
        _window?.Dispose();
        _window = null;
    }

    public void ShowSettings()
    {
        // Settings accessed via window's context menu
    }
}
