using System;
using System.Windows;
// using System.Windows.Controls is omitted to avoid ambiguity with WinForms UserControl
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.AppLauncher;

public class AppLauncherWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget() => new AppLauncherWidget();
}

public class AppLauncherWidget : IWidget
{
    private AppLauncherWindow? _window;

    public string WidgetKey => "app-launcher";
    public string DisplayName => "App Launcher";
    public string Version => "1.0.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => false;

    public Window? CreateWindow()
    {
        if (_window == null)
        {
            _window = new AppLauncherWindow();
            _window.Owner = System.Windows.Application.Current?.MainWindow;
        }
        return _window;
    }

    public System.Windows.Controls.UserControl GetView() => new System.Windows.Controls.UserControl();

    public void OnInitialize() { }

    public void OnDispose()
    {
        _window?.Close();
        _window = null;
    }
}
