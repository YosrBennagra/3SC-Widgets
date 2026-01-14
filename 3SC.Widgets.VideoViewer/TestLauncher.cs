using System;
using System.IO;
using System.Windows;
using Serilog;

namespace _3SC.Widgets.VideoViewer;

public class TestLauncher
{
    [STAThread]
    public static void Main()
    {
        // Setup Serilog for testing
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Widgets", "video-viewer", "logs", "test.log");

        var logDir = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting Video Viewer test launcher");

            var app = new Application();
            var window = new VideoViewerWindow();
            
            var viewModel = (window.DataContext as VideoWidgetViewModel)!;
            viewModel.OnInitialize();

            app.Run(window);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Video Viewer test launcher crashed");
            MessageBox.Show($"Error: {ex.Message}", "Video Viewer Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
