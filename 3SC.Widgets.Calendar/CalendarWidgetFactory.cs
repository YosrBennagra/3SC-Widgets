using System;
using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Calendar
{
    public class CalendarWidgetFactory : IWidgetFactory
    {
        public IWidget CreateWidget() => new CalendarWidget();
    }

    public class CalendarWidget : IWidget
    {
        private CalendarWindow? _window;

        public string WidgetKey => "calendar";
        public string DisplayName => "Calendar";
        public string Version => "1.0.0";
        public bool HasOwnWindow => true;
        public bool HasSettings => false;

        public Window? CreateWindow()
        {
            if (_window == null)
            {
                _window = new CalendarWindow();
                _window.Owner = Application.Current?.MainWindow;
            }
            return _window;
        }

        public System.Windows.Controls.UserControl GetView()
        {
            // This widget provides its own window; return an empty view as fallback.
            return new System.Windows.Controls.UserControl();
        }

        public void OnInitialize()
        {
            // No-op for now
        }

        public void OnDispose()
        {
            _window?.Close();
            _window = null;
        }
    }
}
