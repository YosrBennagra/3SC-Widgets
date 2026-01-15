using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace _3SC.Widgets.AppLauncher
{
    public class WidgetWindowBase : Window
    {
        private System.Windows.Point _dragStartPosition;
        private bool _isDragging;

        public WidgetWindowBase()
        {
        }

        protected virtual Task OnWidgetLoadedAsync() => Task.CompletedTask;
        protected virtual Task OnWidgetClosingAsync() => Task.CompletedTask;
        protected virtual bool IsDragBlocked(DependencyObject? source) => false;
        protected virtual double MinWidgetWidth => 100;
        protected virtual double MinWidgetHeight => 80;

        protected override async void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            await OnWidgetLoadedAsync();
        }

        protected override async void OnClosed(System.EventArgs e)
        {
            await OnWidgetClosingAsync();
            base.OnClosed(e);
        }

        // Window dragging support
        protected void Border_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsDragBlocked(e.OriginalSource as DependencyObject))
            {
                _isDragging = true;
                _dragStartPosition = e.GetPosition(this);
                ((UIElement)sender).CaptureMouse();
            }
        }

        protected void Border_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _dragStartPosition;

                Left += offset.X;
                Top += offset.Y;
            }
        }

        protected void Border_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ((UIElement)sender).ReleaseMouseCapture();
            }
        }

        // Common widget actions
        protected void LockWidget_Click(object sender, RoutedEventArgs e)
        {
            // Lock/unlock widget position - placeholder
        }

        protected void ResizeToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle resize mode - placeholder
        }

        protected void RemoveWidget_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Resize grip handlers
        protected void ResizeTop_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = Height - e.VerticalChange;
            if (newHeight >= MinWidgetHeight)
            {
                Height = newHeight;
                Top += e.VerticalChange;
            }
        }

        protected void ResizeBottom_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = Height + e.VerticalChange;
            if (newHeight >= MinWidgetHeight)
            {
                Height = newHeight;
            }
        }

        protected void ResizeLeft_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newWidth = Width - e.HorizontalChange;
            if (newWidth >= MinWidgetWidth)
            {
                Width = newWidth;
                Left += e.HorizontalChange;
            }
        }

        protected void ResizeRight_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            if (newWidth >= MinWidgetWidth)
            {
                Width = newWidth;
            }
        }
    }
}
