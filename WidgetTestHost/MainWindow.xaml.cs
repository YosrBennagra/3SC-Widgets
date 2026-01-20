using System.Windows;
using System.Windows.Controls;
using _3SC.Widgets.Contracts;

namespace WidgetTestHost;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, IWidgetFactory> _widgetFactories = new();
    private IWidget? _currentWidget;
    private Window? _currentWidgetWindow;

    public MainWindow()
    {
        InitializeComponent();
        LoadAvailableWidgets();
    }

    private void LoadAvailableWidgets()
    {
        // Register all available widgets
        _widgetFactories["Clock"] = new _3SC.Widgets.Clock.ClockWidgetFactory();
        _widgetFactories["QuickLinks"] = new _3SC.Widgets.QuickLinks.QuickLinksWidgetFactory();
        _widgetFactories["Calendar"] = new _3SC.Widgets.Calendar.CalendarWidgetFactory();
        _widgetFactories["Notes"] = new _3SC.Widgets.Notes.NotesWidgetFactory();
        _widgetFactories["Image Viewer"] = new _3SC.Widgets.ImageViewer.ImageViewerWidgetFactory();
        _widgetFactories["Video Viewer"] = new _3SC.Widgets.VideoViewer.VideoViewerWidgetFactory();
        _widgetFactories["PDF Viewer"] = new _3SC.Widgets.PdfViewer.PdfViewerWidgetFactory();
        _widgetFactories["Folders"] = new _3SC.Widgets.Folders.FoldersWidgetFactory();
        _widgetFactories["App Launcher"] = new _3SC.Widgets.AppLauncher.AppLauncherWidgetFactory();
        _widgetFactories["Game Vault"] = new _3SC.Widgets.GameVault.GameVaultWidgetFactory();
        _widgetFactories["Clipboard History"] = new _3SC.Widgets.ClipboardHistory.ClipboardHistoryWidgetFactory();
        _widgetFactories["Desktop Pet"] = new _3SC.Widgets.DesktopPet.DesktopPetWidgetFactory();
        _widgetFactories["System Pulse"] = new _3SC.Widgets.SystemPulse.SystemPulseWidgetFactory();
        _widgetFactories["Ambient Sounds"] = new _3SC.Widgets.AmbientSounds.AmbientSoundsWidgetFactory();
        _widgetFactories["Breathe"] = new _3SC.Widgets.Breathe.BreatheWidgetFactory();
        _widgetFactories["Pomodoro+ Timer"] = new ThreeSC.Widgets.Pomodoro.PomodoroWidgetFactory();

        WidgetSelector.ItemsSource = _widgetFactories.Keys.OrderBy(k => k).ToList();
        WidgetSelector.SelectedIndex = -1;
    }

    private void WidgetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WidgetSelector.SelectedItem is string widgetName)
        {
            LoadWidget(widgetName);
        }
    }

    private void ReloadWidget_Click(object sender, RoutedEventArgs e)
    {
        if (WidgetSelector.SelectedItem is string widgetName)
        {
            LoadWidget(widgetName);
        }
    }

    private void LoadWidget(string widgetName)
    {
        try
        {
            // Clean up previous widget
            CleanupCurrentWidget();

            // Create new widget instance
            var factory = _widgetFactories[widgetName];
            _currentWidget = factory.CreateWidget();

            StatusText.Text = $"Loading {widgetName}...";

            // Initialize the widget
            _currentWidget.OnInitialize();

            // Check if widget has its own window
            if (_currentWidget.HasOwnWindow)
            {
                // Show widget in its own window
                _currentWidgetWindow = _currentWidget.CreateWindow();
                if (_currentWidgetWindow != null)
                {
                    _currentWidgetWindow.Owner = this;
                    _currentWidgetWindow.Show();

                    // Show message in main window
                    InstructionsPanel.Visibility = Visibility.Collapsed;
                    EmbeddedWidgetContainer.Visibility = Visibility.Collapsed;
                    WindowedWidgetMessage.Visibility = Visibility.Visible;

                    StatusText.Text = $"✓ {widgetName} opened in separate window";
                }
            }
            else
            {
                // Embed widget view in the container
                var view = _currentWidget.GetView();
                WidgetContentControl.Content = view;

                InstructionsPanel.Visibility = Visibility.Collapsed;
                WindowedWidgetMessage.Visibility = Visibility.Collapsed;
                EmbeddedWidgetContainer.Visibility = Visibility.Visible;

                StatusText.Text = $"✓ {widgetName} loaded";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading widget: {ex.Message}\n\n{ex.StackTrace}",
                "Widget Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "✗ Error loading widget";
        }
    }

    private void CleanupCurrentWidget()
    {
        if (_currentWidget != null)
        {
            _currentWidget.OnDispose();
            _currentWidget = null;
        }

        if (_currentWidgetWindow != null)
        {
            _currentWidgetWindow.Close();
            _currentWidgetWindow = null;
        }

        WidgetContentControl.Content = null;

        InstructionsPanel.Visibility = Visibility.Visible;
        EmbeddedWidgetContainer.Visibility = Visibility.Collapsed;
        WindowedWidgetMessage.Visibility = Visibility.Collapsed;
    }

    protected override void OnClosed(EventArgs e)
    {
        CleanupCurrentWidget();
        base.OnClosed(e);
    }
}