using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.SystemPulse;

public partial class SystemPulseWindow : WidgetWindowBase
{
    private readonly SystemPulseViewModel _viewModel;

    public SystemPulseWindow() : base()
    {
        InitializeComponent();

        _viewModel = new SystemPulseViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Initialize();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.Dispose();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }
}
