using System.Windows.Controls;

namespace _3SC.Widgets.Clock;

/// <summary>
/// UserControl wrapper for the Clock widget.
/// The 3SC host expects a UserControl, not a Window.
/// </summary>
public partial class ClockWidgetControl : System.Windows.Controls.UserControl
{
    private readonly ClockWidgetWindow _clockWindow;
    private System.Windows.Threading.DispatcherTimer? _timer;

    public ClockWidgetControl(ClockWidgetWindow clockWindow)
    {
        InitializeComponent();
        _clockWindow = clockWindow;

        // Copy the visual content from the window
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Start a simple clock display
        UpdateTime();

        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, args) => UpdateTime();
        _timer.Start();
    }

    private void UpdateTime()
    {
        // Simple clock display
        var now = DateTime.Now;
        var timeText = now.ToString("h:mm:ss tt");

        // Update the TextBlock in the Grid
        ContentGrid.Children.Clear();
        ContentGrid.Children.Add(new TextBlock
        {
            Text = timeText,
            FontSize = 28,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xF1, 0xF5, 0xF9)),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        });
    }
}
