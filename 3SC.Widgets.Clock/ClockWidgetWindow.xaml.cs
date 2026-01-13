using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using _3SC.Domain.ValueObjects;

namespace _3SC.Widgets.Clock;

/// <summary>
/// ClockWidget displays the current time with timezone support.
/// External widget version with full drag functionality.
/// </summary>
public partial class ClockWidgetWindow : Window
{
    private readonly DispatcherTimer _timer;
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
    private bool _use24HourFormat;
    private bool _showSeconds;
    private bool _showTimeZoneLabel;
    private ClockWidgetSettings? _currentSettings;

    // Drag tracking (use Win32 coordinates for smooth movement)
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

        _currentSettings = settings ?? ClockWidgetSettings.Default();
        ApplySettings(_currentSettings);

        // Start timer (match built-in widget timing)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
        
        Loaded += ClockWidget_Loaded;
    }

    private void ClockWidget_Loaded(object? sender, RoutedEventArgs e)
    {
        // Access HWND and set extended styles
        var helper = new WindowInteropHelper(this);
        var hwnd = helper.Handle;

        // Hide from Alt+Tab and prevent activation
        Win32Interop.MakeToolWindowNoActivate(hwnd);

        // Show window without activating
        Win32Interop.ShowWindowNoActivate(hwnd);

        // Place window behind normal windows
        Win32Interop.SendToBottom(hwnd);
    }

    private void ApplySettings(ClockWidgetSettings settings)
    {
        _currentSettings = settings;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        _use24HourFormat = settings.Use24HourFormat;
        _showSeconds = settings.ShowSeconds;
        _showTimeZoneLabel = settings.ShowTimeZoneLabel;

        // Set initial values
        UpdateTime();
        
        // Update timezone label
        if (_showTimeZoneLabel)
        {
            TimeZoneLabel.Text = _timeZone.StandardName;
            TimeZoneLabel.Visibility = Visibility.Visible;
        }
        else
        {
            TimeZoneLabel.Visibility = Visibility.Collapsed;
        }
    }

    private void RootBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        RootBorder.ContextMenu.IsOpen = true;
        e.Handled = true;
    }

    private void RootBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Start dragging
        _isDragging = true;
        var helper = new WindowInteropHelper(this);
        _dragHwnd = helper.Handle;
        Win32Interop.GetCursorPos(out _dragStartCursor);
        Win32Interop.GetWindowRect(_dragHwnd, out _dragStartRect);

        // Capture mouse to continue receiving events during drag
        RootBorder.CaptureMouse();

        e.Handled = true;
    }

    private void RootBorder_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        RootBorder.ReleaseMouseCapture();
        _dragHwnd = IntPtr.Zero;

        e.Handled = true;
    }

    private void RootBorder_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (_dragHwnd == IntPtr.Zero)
        {
            return;
        }

        // Compute delta in physical pixels and move the HWND directly for smooth dragging
        Win32Interop.GetCursorPos(out var current);
        var dx = current.X - _dragStartCursor.X;
        var dy = current.Y - _dragStartCursor.Y;

        var newLeft = _dragStartRect.Left + dx;
        var newTop = _dragStartRect.Top + dy;

        // Constrain to screen boundaries
        var constrainedPosition = ScreenBoundsHelper.ConstrainToScreenBounds(
            newLeft,
            newTop,
            _dragStartRect.Right - _dragStartRect.Left,
            _dragStartRect.Bottom - _dragStartRect.Top);

        Win32Interop.SetWindowPos(
            _dragHwnd,
            IntPtr.Zero,
            constrainedPosition.X,
            constrainedPosition.Y,
            0,
            0,
            Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE | Win32Interop.SWP_NOZORDER);

        // Update WPF window position to match
        Left = constrainedPosition.X;
        Top = constrainedPosition.Y;

        e.Handled = true;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new ClockSettingsWindow(_currentSettings)
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (settingsWindow.ShowDialog() == true && settingsWindow.UpdatedSettings != null)
        {
            ApplySettings(settingsWindow.UpdatedSettings);
        }
    }

    private void CloseWidget_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, _timeZone);
        
        string format = _use24HourFormat
            ? (_showSeconds ? "HH:mm:ss" : "HH:mm")
            : (_showSeconds ? "hh:mm:ss tt" : "hh:mm tt");

        TimeTextBlock.Text = now.ToString(format, CultureInfo.InvariantCulture);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
        base.OnClosing(e);
    }
}
