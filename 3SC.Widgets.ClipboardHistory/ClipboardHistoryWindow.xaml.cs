using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using _3SC.Widgets.ClipboardHistory.Helpers;
using Serilog;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace _3SC.Widgets.ClipboardHistory;

/// <summary>
/// Widget window for Clipboard History.
/// Monitors clipboard and displays history of copied items.
/// </summary>
public partial class ClipboardHistoryWindow : WidgetWindowBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ClipboardHistoryWindow>();

    private readonly ClipboardHistoryViewModel _viewModel;

    // Drag tracking (Win32 for smooth movement)
    private bool _isDragging;
    private Win32Interop.Point _dragStartCursor;
    private Win32Interop.Rect _dragStartRect;
    private IntPtr _dragHwnd = IntPtr.Zero;

    public ClipboardHistoryWindow() : this(Guid.Empty, 0, 0, 350, 450, false)
    {
    }

    public ClipboardHistoryWindow(Guid widgetInstanceId, double left, double top, double width, double height, bool isLocked)
    {
        InitializeComponent();

        // Initialize widget base (lock + resize system)
        InitializeWidgetWindow(
            new WidgetWindowInit(
                widgetInstanceId,
                left,
                top,
                width,
                height,
                IsLocked: isLocked
            ),
            new WidgetWindowParts(
                LockWidgetMenuItem,
                ResizeToggleMenuItem,
                ResizeOutline,
                ResizeTop,
                ResizeBottom,
                ResizeLeft,
                ResizeRight,
                WidgetKey: "clipboard-history"
            )
        );

        _viewModel = new ClipboardHistoryViewModel();
        DataContext = _viewModel;

        Loaded += ClipboardHistoryWindow_Loaded;

        Log.Debug("ClipboardHistoryWindow created with InstanceId={InstanceId}", widgetInstanceId);
    }

    #region Window lifecycle

    private void ClipboardHistoryWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;

            // Hide from Alt+Tab, prevent focus stealing
            Win32Interop.MakeToolWindowNoActivate(hwnd);

            // Show without activating
            Win32Interop.ShowWindowNoActivate(hwnd);

            // Send behind normal windows (desktop-widget behavior)
            Win32Interop.SendToBottom(hwnd);

            Log.Information("Clipboard History widget loaded at position ({Left}, {Top})", Left, Top);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize clipboard history widget window");
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        try
        {
            _viewModel?.Dispose();
            Log.Information("Clipboard History widget closing");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during clipboard history widget cleanup");
        }

        base.OnClosing(e);
    }

    #endregion

    #region Drag handling

    private void RootBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsLocked)
            return;

        // Block dragging if clicking on resize handles or other blocked elements
        if (IsDragBlocked(e.OriginalSource as DependencyObject))
            return;

        _isDragging = true;

        var helper = new WindowInteropHelper(this);
        _dragHwnd = helper.Handle;

        Win32Interop.GetCursorPos(out _dragStartCursor);
        Win32Interop.GetWindowRect(_dragHwnd, out _dragStartRect);

        RootBorder.CaptureMouse();
        e.Handled = true;
    }

    private void RootBorder_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        _dragHwnd = IntPtr.Zero;

        RootBorder.ReleaseMouseCapture();
        e.Handled = true;

        Log.Debug("Widget moved to ({Left}, {Top})", Left, Top);
    }

    private void RootBorder_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _dragHwnd == IntPtr.Zero || e.LeftButton != MouseButtonState.Pressed)
            return;

        Win32Interop.GetCursorPos(out var current);

        var dx = current.X - _dragStartCursor.X;
        var dy = current.Y - _dragStartCursor.Y;

        var newLeft = _dragStartRect.Left + dx;
        var newTop = _dragStartRect.Top + dy;

        var constrained = ScreenBoundsHelper.ConstrainToScreenBounds(
            newLeft,
            newTop,
            _dragStartRect.Right - _dragStartRect.Left,
            _dragStartRect.Bottom - _dragStartRect.Top
        );

        Win32Interop.SetWindowPos(
            _dragHwnd,
            IntPtr.Zero,
            constrained.X,
            constrained.Y,
            0,
            0,
            Win32Interop.SWP_NOSIZE |
            Win32Interop.SWP_NOACTIVATE |
            Win32Interop.SWP_NOZORDER
        );

        Left = constrained.X;
        Top = constrained.Y;

        e.Handled = true;
    }

    #endregion

    #region Drag blocking for resize handles

    protected override bool IsDragBlocked(DependencyObject? source)
    {
        // Don't start dragging if clicking on resize handles or scrollviewer
        while (source != null)
        {
            if (source is Thumb thumb &&
                (thumb == ResizeTop || thumb == ResizeBottom || thumb == ResizeLeft || thumb == ResizeRight))
            {
                return true;
            }

            if (source is System.Windows.Controls.ScrollViewer)
            {
                return true;
            }

            source = System.Windows.Media.VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    #endregion

    #region Context menu

    private void RootBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (RootBorder.ContextMenu is { } ctx)
            ctx.IsOpen = true;
        e.Handled = true;
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel?.ClearAll();
            Log.Information("Clipboard history cleared");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to clear clipboard history");
        }
    }

    private new void RemoveWidget_Click(object sender, RoutedEventArgs e)
    {
        Log.Information("Remove widget requested");
        base.RemoveWidget_Click(sender, e);
    }

    #endregion

    #region Item interactions

    private void Item_Click(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is FrameworkElement { DataContext: ClipboardItem item })
            {
                if (item.IsImage && item.Image != null)
                {
                    System.Windows.Clipboard.SetImage(item.Image);
                    Log.Debug("Copied image back to clipboard: {Width}x{Height}", item.Image.PixelWidth, item.Image.PixelHeight);
                }
                else if (!string.IsNullOrWhiteSpace(item.Text))
                {
                    System.Windows.Clipboard.SetText(item.Text);
                    Log.Debug("Copied text back to clipboard: {Text}", item.DisplayText);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to copy item to clipboard");
        }
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is FrameworkElement { DataContext: ClipboardItem item })
            {
                _viewModel?.RemoveItem(item);
                Log.Debug("Removed clipboard item: {Text}", item.DisplayText);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete clipboard item");
        }
    }

    #endregion
}

