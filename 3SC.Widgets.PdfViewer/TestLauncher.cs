using System;
using System.Windows;
using Serilog;
using Serilog.Events;

namespace TestPdfViewerWidget;

public class Program
{
    [STAThread]
    public static void Main()
    {
        // Setup logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("test-pdf-viewer.log", 
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        try
        {
            Log.Information("Starting PDF Viewer Widget Test");

            var app = new Application();
            var window = new _3SC.Widgets.PdfViewer.PdfViewerWindow();
            
            app.Run(window);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in PDF Viewer test");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
