using System;
using System.Windows;
using Serilog;
using Serilog.Events;

namespace TestImageViewerWidget;

public class Program
{
    [STAThread]
    public static void Main()
    {
        // Setup logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("test-image-viewer.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        try
        {
            Log.Information("Starting Image Viewer Widget Test");

            var app = new Application();
            var window = new _3SC.Widgets.ImageViewer.ImageViewerWindow();

            app.Run(window);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in Image Viewer test");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
