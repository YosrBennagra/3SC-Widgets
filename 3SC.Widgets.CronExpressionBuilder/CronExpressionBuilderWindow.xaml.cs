using System;
using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.CronExpressionBuilder;

public partial class CronExpressionBuilderWindow : Window
{
    private CronExpressionBuilderViewModel _viewModel;
    private bool _isResizing = false;
    private Point _resizeStartPoint;
    private Size _resizeStartSize;

    public CronExpressionBuilderWindow()
    {
        InitializeComponent();
        _viewModel = new CronExpressionBuilderViewModel();
        DataContext = _viewModel;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResizeGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isResizing = true;
        _resizeStartPoint = PointToScreen(e.GetPosition(this));
        _resizeStartSize = new Size(ActualWidth, ActualHeight);
        Mouse.Capture((UIElement)sender);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isResizing)
        {
            var currentPoint = PointToScreen(e.GetPosition(this));
            var delta = currentPoint - _resizeStartPoint;

            var newWidth = Math.Max(MinWidth, _resizeStartSize.Width + delta.X);
            var newHeight = Math.Max(MinHeight, _resizeStartSize.Height + delta.Y);

            if (MaxWidth > 0) newWidth = Math.Min(MaxWidth, newWidth);
            if (MaxHeight > 0) newHeight = Math.Min(MaxHeight, newHeight);

            Width = newWidth;
            Height = newHeight;
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_isResizing)
        {
            _isResizing = false;
            Mouse.Capture(null);
        }
    }
}
