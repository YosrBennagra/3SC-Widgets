using System;
using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.GradientPlayground
{
    public class WidgetWindowBase : Window
    {
        private bool _isDragging;
        private Point _clickPosition;

        public WidgetWindowBase()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.CanResize;
            ShowInTaskbar = false;
            Topmost = true;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(this);
            CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = PointToScreen(e.GetPosition(this));
                Left = currentPosition.X - _clickPosition.X;
                Top = currentPosition.Y - _clickPosition.Y;
            }
        }
    }
}
