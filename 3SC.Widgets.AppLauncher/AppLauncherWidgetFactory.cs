using System;
using System.Windows;
// using System.Windows.Controls is omitted to avoid ambiguity with WinForms UserControl
using _3SC.Widgets.Contracts;
using Serilog;

namespace _3SC.Widgets.AppLauncher;

public class AppLauncherWidgetFactory : IWidgetFactory
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AppLauncherWidgetFactory>();

    public IWidget CreateWidget()
    {
        Log.Debug("Creating AppLauncher widget instance");
        return new AppLauncherWidget();
    }
}

public class AppLauncherWidget : IWidget
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AppLauncherWidget>();
    private AppLauncherWindow? _window;

    public string WidgetKey => "app-launcher";
    public string DisplayName => "App Launcher";
    public string Version => "1.1.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => false;

    public Window? CreateWindow()
    {
        try
        {
            if (_window == null)
            {
                // Provide default positioning/size when created outside host
                _window = new AppLauncherWindow(Guid.Empty, 100, 100, 420, 340, false);
                _window.Owner = System.Windows.Application.Current?.MainWindow;
                Log.Information("AppLauncher window created successfully");
            }
            return _window;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create AppLauncher window");
            throw;
        }
    }

    public System.Windows.Controls.UserControl GetView() => new System.Windows.Controls.UserControl();

    public void OnInitialize()
    {
        Log.Debug("AppLauncher widget initialized");
    }

    public void OnDispose()
    {
        try
        {
            _window?.Close();
            _window = null;
            Log.Information("AppLauncher widget disposed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during AppLauncher widget disposal");
        }
    }
}
