using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Serilog;

namespace _3SC.Widgets.ImageViewer;

public partial class ImageViewerWindow : WidgetWindowBase
{
    private readonly ImageWidgetViewModel _viewModel;
    private readonly ILogger _logger = Log.ForContext<ImageViewerWindow>();

    public ImageViewerWindow()
        : this(Guid.Empty, 0, 0, 400, 300, false)
    {
    }

    public ImageViewerWindow(Guid widgetInstanceId, double left, double top, double width, double height, bool isLocked)
    {
        _logger.Debug("ImageViewerWindow constructor called with InstanceId={InstanceId}", widgetInstanceId);

        InitializeComponent();

        InitializeWidgetWindow(
            new WidgetWindowInit(widgetInstanceId, left, top, width, height, isLocked),
            new WidgetWindowParts(
                LockWidgetMenuItem: LockWidgetMenuItem,
                ResizeToggleMenuItem: ResizeToggleMenuItem,
                ResizeOutlineElement: ResizeOutline,
                ResizeTopThumb: ResizeTop,
                ResizeBottomThumb: ResizeBottom,
                ResizeLeftThumb: ResizeLeft,
                ResizeRightThumb: ResizeRight,
                WidgetKey: "image"));

        _viewModel = new ImageWidgetViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
        Closing += (s, e) => _viewModel.Dispose();

        _logger.Information("ImageViewerWindow initialized successfully");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _logger.Debug("ImageViewerWindow loaded");
        _viewModel.OnInitialize();
    }

    protected override bool IsDragBlocked(DependencyObject? source)
    {
        // Don't allow dragging when clicking on interactive controls
        return source is not null && IsInteractiveControl(source);
    }

    private static bool IsInteractiveControl(DependencyObject element)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current is System.Windows.Controls.Button or
                Slider or
                Thumb or
                System.Windows.Controls.ListBox or
                ListBoxItem or
                System.Windows.Controls.Primitives.ScrollBar or
                System.Windows.Controls.Image)
            {
                return true;
            }

            // Try visual tree first, fall back to logical tree for non-visual elements
            current = current is Visual
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }

        return false;
    }
}
