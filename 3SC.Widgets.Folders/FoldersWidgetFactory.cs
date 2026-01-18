using System;
using System.Windows;
using _3SC.Widgets.Contracts;
using Serilog;

namespace _3SC.Widgets.Folders
{
    [Widget("folders", "Folders")]
    public class FoldersWidgetFactory : IWidgetFactory
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<FoldersWidgetFactory>();

        public IWidget CreateWidget()
        {
            Log.Debug("Creating Folders widget instance");
            return new FoldersWidget();
        }
    }

    [Widget("folders", "Folders")]
    public class FoldersWidget : IWidget
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<FoldersWidget>();
        private FoldersWindow? _window;

        public string WidgetKey => "folders";
        public string DisplayName => "Folders";
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
                    _window = new FoldersWindow(Guid.Empty, 100, 100, 420, 340, false);
                    _window.Owner = Application.Current?.MainWindow;
                    Log.Information("Folders window created successfully");
                }
                return _window;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create folders window");
                throw;
            }
        }

        public System.Windows.Controls.UserControl GetView()
        {
            return new System.Windows.Controls.UserControl();
        }

        public void OnInitialize()
        {
            Log.Debug("Folders widget initialized");
        }

        public void OnDispose()
        {
            try
            {
                _window?.Close();
                _window = null;
                Log.Information("Folders widget disposed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during folders widget disposal");
            }
        }

        public void ShowSettings()
        {
            Log.Information("ShowSettings called - no settings available");
        }
    }
}
