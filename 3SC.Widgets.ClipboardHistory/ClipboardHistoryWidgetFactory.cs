using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.ClipboardHistory
{
    [Widget("clipboard-history", "Clipboard History")]
    public class ClipboardHistoryWidgetFactory : IWidgetFactory
    {
        public IWidget CreateWidget() => new ClipboardHistoryWidget();
    }

    [Widget("clipboard-history", "Clipboard History")]
    public class ClipboardHistoryWidget : IWidget
    {
        private ClipboardHistoryWindow? _window;

        public string WidgetKey => "clipboard-history";
        public string DisplayName => "Clipboard History";
        public string Version => "1.0.0";
        public bool HasOwnWindow => true;
        public bool HasSettings => false;

        public System.Windows.Window? CreateWindow()
        {
            _window = new ClipboardHistoryWindow();
            _window.Owner = Application.Current?.MainWindow;
            return _window;
        }

        public System.Windows.Controls.UserControl GetView()
        {
            // This widget uses its own window; return empty control to satisfy interface
            return new System.Windows.Controls.UserControl();
        }

        public void OnInitialize()
        {
            // Initialize VM if needed
        }

        public void OnDispose()
        {
            _window?.Close();
            _window = null;
        }

        public void ShowSettings() { }
    }
}
