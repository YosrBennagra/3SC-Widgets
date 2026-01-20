using System;
using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.ApiStatus;

public partial class ApiStatusWindow : Window
{
    private ApiStatusViewModel _viewModel;
    private bool _isResizing = false;
    private Point _resizeStartPoint;
    private Size _resizeStartSize;

    public ApiStatusWindow()
    {
        InitializeComponent();
        _viewModel = new ApiStatusViewModel();
        DataContext = _viewModel;
        Closing += (s, e) => _viewModel.Dispose();
    }

    public void ShowSettingsDialog()
    {
        var settingsDialog = new SettingsDialog(_viewModel);
        settingsDialog.Owner = this;
        settingsDialog.ShowDialog();
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

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsDialog();
    }

    private void Endpoint_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ApiEndpoint endpoint)
        {
            _viewModel.SelectedEndpoint = endpoint;
        }
    }

    private void Url_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _viewModel.SaveSettings();
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
