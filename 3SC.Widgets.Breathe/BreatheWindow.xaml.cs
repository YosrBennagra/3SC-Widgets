using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.Breathe;

public partial class BreatheWindow : WidgetWindowBase
{
    private readonly BreatheViewModel _viewModel;

    public BreatheWindow() : base()
    {
        InitializeComponent();

        _viewModel = new BreatheViewModel();
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

    private void Circle_Click(object sender, MouseButtonEventArgs e)
    {
        _viewModel.ToggleBreathingCommand.Execute(null);
    }
}
