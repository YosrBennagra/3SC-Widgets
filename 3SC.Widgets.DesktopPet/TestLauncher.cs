#if DEBUG
using System;
using Serilog;

// Explicit aliases to avoid conflicts
using Application = System.Windows.Application;
using ShutdownMode = System.Windows.ShutdownMode;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

namespace TestDesktopPet;

/// <summary>
/// Test launcher for debugging the Desktop Pet widget standalone.
/// Only compiled in Debug configuration.
/// </summary>
public class Program
{
    [STAThread]
    public static void Main()
    {
        // Configure Serilog for file output during testing
        var logPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Logs", "desktop-pet-test.log");

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath)!);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Starting Desktop Pet Test Launcher...");

        try
        {
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

            var factory = new _3SC.Widgets.DesktopPet.DesktopPetWidgetFactory();
            var widget = factory.CreateWidget();

            Log.Information("Widget created: {Name} v{Version}", widget.DisplayName, widget.Version);
            Log.Information("Has Settings: {HasSettings}, Has Own Window: {HasOwnWindow}",
                widget.HasSettings, widget.HasOwnWindow);

            var window = widget.CreateWindow();
            if (window != null)
            {
                // Center on screen for testing
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                widget.OnInitialize();

                Log.Information("Running Desktop Pet window...");
                app.Run(window);

                widget.OnDispose();
            }
            else
            {
                Log.Error("Failed to create widget window!");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception in Desktop Pet");
            throw;
        }
        finally
        {
            Log.Information("Desktop Pet Test Launcher shutting down");
            Log.CloseAndFlush();
        }
    }
}
#endif
