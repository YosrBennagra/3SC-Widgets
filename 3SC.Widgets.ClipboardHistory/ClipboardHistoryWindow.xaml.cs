using System.Windows;
using System.Windows.Input;
using _3SC.Widgets.ClipboardHistory;

namespace _3SC.Widgets.ClipboardHistory
{
    public partial class ClipboardHistoryWindow : Window
    {
        private bool _isDragging;
        private Point _dragStart;

        public ClipboardHistoryWindow()
        {
            InitializeComponent();
            DataContext ??= new ClipboardHistoryViewModel();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // No-op for now
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(this);
            CaptureMouse();
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition(null);
            this.Left += pos.X - _dragStart.X;
            this.Top += pos.Y - _dragStart.Y;
        }

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClipboardHistoryViewModel vm && sender is FrameworkElement fe && fe.DataContext is ClipboardItem item)
            {
                vm.RemoveItem(item);
            }
        }
    }
}
