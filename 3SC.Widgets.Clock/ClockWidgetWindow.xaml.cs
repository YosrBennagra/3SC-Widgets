using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using _3SC.Domain.ValueObjects;
using _3SC.Widgets.Clock.Helpers;
using Serilog;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace _3SC.Widgets.Clock;

/// <summary>
/// Widget shell for the Clock widget.
/// Handles window behavior (drag, resize, context menu, lifecycle).
/// Clock rendering and timing logic live in ClockWidgetView.
/// </summary>
public partial class ClockWidgetWindow : WidgetWindowBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ClockWidgetWindow>();
    
    private ClockWidgetSettings _currentSettings;

    // Drag tracking (Win32 for smooth movement)
    private bool _isDragging;
    private Win32Interop.Point _dragStartCursor;
    private Win32Interop.Rect _dragStartRect;
    private IntPtr _dragHwnd = IntPtr.Zero;

    public ClockWidgetWindow() : this(Guid.Empty, null)
    {
    }

    public ClockWidgetWindow(Guid widgetInstanceId, ClockWidgetSettings? settings)
    {
        InitializeComponent();

        // Initialize widget base (lock + resize system)
        InitializeWidgetWindow(
            new WidgetWindowInit(
                widgetInstanceId,
                Left,
                Top,
                Width,
                Height,
                IsLocked: false
            ),
            new WidgetWindowParts(
                LockWidgetMenuItem,
                ResizeToggleMenuItem,
                ResizeOutline,
                ResizeTop,
                ResizeBottom,
                ResizeLeft,
                ResizeRight,
                WidgetKey: "clock"
            )
        );

        _currentSettings = settings ?? ClockWidgetSettings.Default();

        Loaded += ClockWidget_Loaded;
        
        Log.Debug("ClockWidgetWindow created with InstanceId={InstanceId}", widgetInstanceId);
    }

    #region Window lifecycle

    private void ClockWidget_Loaded(object? sender, RoutedEventArgs e)
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

            // Apply settings to the clock view (named element in XAML)
            ClockView?.ApplySettings(_currentSettings);
            
            Log.Information("Clock widget loaded at position ({Left}, {Top})", Left, Top);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize clock widget window");
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        try
        {
            // Dispose the clock view to stop timer
            ClockView?.Dispose();
            Log.Information("Clock widget closing");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during clock widget cleanup");
        }
        
        base.OnClosing(e);
    }

    #endregion

    #region Drag handling

    private void RootBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsLocked)
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

    #region Context menu

    private void RootBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (RootBorder.ContextMenu is { } ctx) 
            ctx.IsOpen = true;
        e.Handled = true;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ClockSettingsWindow(_currentSettings)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.ShowDialog() == true && dialog.UpdatedSettings != null)
            {
                _currentSettings = dialog.UpdatedSettings;
                ClockView?.ApplySettings(_currentSettings);
                
                Log.Information("Clock settings updated: TimeZone={TimeZone}, 24Hour={Use24Hour}",
                    _currentSettings.TimeZoneId, _currentSettings.Use24HourFormat);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open or apply settings");
        }
    }

    private new void RemoveWidget_Click(object sender, RoutedEventArgs e)
    {
        Log.Information("Remove widget requested");
        base.RemoveWidget_Click(sender, e);
    }

    #endregion
}
