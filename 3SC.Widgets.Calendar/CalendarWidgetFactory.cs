using System;
using System.Windows;
using _3SC.Widgets.Contracts;
using Serilog;

namespace _3SC.Widgets.Calendar
{
    public class CalendarWidgetFactory : IWidgetFactory
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<CalendarWidgetFactory>();

        public IWidget CreateWidget()
        {
            Log.Debug("Creating Calendar widget instance");
            return new CalendarWidget();
        }
    }

    public class CalendarWidget : IWidget
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<CalendarWidget>();
        private CalendarWindow? _window;

        public string WidgetKey => "calendar";
        public string DisplayName => "Calendar";
        public string Version => "1.1.0";
        public bool HasOwnWindow => true;
        public bool HasSettings => false;

        public Window? CreateWindow()
        {
            try
            {
                if (_window == null)
                {
                    _window = new CalendarWindow();
                    _window.Owner = Application.Current?.MainWindow;
                    Log.Information("Calendar window created successfully");
                }
                return _window;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create calendar window");
                throw;
            }
        }

        public System.Windows.Controls.UserControl GetView()
        {
            // This widget provides its own window; return an empty view as fallback.
            return new System.Windows.Controls.UserControl();
        }

        public void OnInitialize()
        {
            Log.Debug("Calendar widget initialized");
        }

        public void OnDispose()
        {
            try
            {
                _window?.Close();
                _window = null;
                Log.Information("Calendar widget disposed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during calendar widget disposal");
            }
        }
    }
}
