using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _3SC.Widgets.Notes;

public partial class NotesWindow : Window
{
    private readonly NotesWidgetViewModel _viewModel;
    private bool _isDragging;
    private Point _clickPosition;

    public NotesWindow()
    {
        InitializeComponent();
        
        _viewModel = new NotesWidgetViewModel(loadFromDisk: false);
        DataContext = _viewModel;
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnInitialize();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(this);
            (sender as Border)?.CaptureMouse();
        }
    }

    private void Border_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPosition = e.GetPosition(this);
            var offset = currentPosition - _clickPosition;
            Left += offset.X;
            Top += offset.Y;
        }
    }

    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            (sender as Border)?.ReleaseMouseCapture();
        }
    }
}
