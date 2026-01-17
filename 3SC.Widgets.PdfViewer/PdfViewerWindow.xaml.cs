using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Serilog;

namespace _3SC.Widgets.PdfViewer;

public partial class PdfViewerWindow : WidgetWindowBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PdfViewerWindow>();
    private readonly PdfWidgetViewModel _viewModel;

    public PdfViewerWindow()
        : this(Guid.Empty, 0, 0, 600, 400, false)
    {
    }

    public PdfViewerWindow(Guid widgetInstanceId, double left, double top, double width, double height, bool isLocked)
    {
        Log.Debug("PdfViewerWindow constructor called with InstanceId={InstanceId}", widgetInstanceId);

        InitializeComponent();

        _viewModel = new PdfWidgetViewModel();
        DataContext = _viewModel;

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
                WidgetKey: "pdf"));

        Loaded += OnLoaded;
        Closing += (s, e) => _viewModel.OnDispose();

        Log.Information("PdfViewerWindow initialized successfully");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Log.Debug("PdfViewerWindow loaded");
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
                ScrollViewer)
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
