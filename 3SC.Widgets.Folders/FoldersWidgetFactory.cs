using System;
using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Folders
{
    public class FoldersWidgetFactory : IWidgetFactory
    {
        public IWidget CreateWidget() => new FoldersWidget();
    }

    public class FoldersWidget : IWidget
    {
        private FoldersWindow? _window;

        public string WidgetKey => "folders";
        public string DisplayName => "Folders";
        public string Version => "1.0.0";
        public bool HasOwnWindow => true;
        public bool HasSettings => false;

        public Window? CreateWindow()
        {
            if (_window == null)
            {
                // Provide default positioning/size when created outside host
                _window = new FoldersWindow(Guid.Empty, 100, 100, 300, 200, false);
                _window.Owner = Application.Current?.MainWindow;
            }
            return _window;
        }

        public System.Windows.Controls.UserControl GetView()
        {
            return new System.Windows.Controls.UserControl();
        }

        public void OnInitialize() { }

        public void OnDispose()
        {
            _window?.Close();
            _window = null;
        }
    }
}
