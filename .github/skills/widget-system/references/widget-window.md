# Widget Window Implementation

## Overview
When a widget has `HasOwnWindow = true`, it provides a complete WPF Window with full control over all functionality.

## Window Features

### 1. Transparent Background with Rounded Border

```xaml
<Window x:Class="_3SC.Widgets.Clock.ClockWidgetWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Clock Widget"
        Width="300"
        Height="80"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        ShowInTaskbar="False"
        Topmost="False"
        ResizeMode="NoResize">

    <Window.Resources>
        <SolidColorBrush x:Key="WidgetSurface" Color="#D0080810"/>
        <SolidColorBrush x:Key="WidgetText" Color="#FFF1F5F9"/>
    </Window.Resources>

    <Grid Margin="6">
        <Border CornerRadius="12"
                Background="{StaticResource WidgetSurface}">
            <!-- Widget content -->
        </Border>
    </Grid>
</Window>
```

### 2. Drag Support

```csharp
using System.Windows.Interop;

public partial class ClockWidgetWindow : Window
{
    private bool _isDragging;
    private Win32Interop.Point _dragStartCursor;
    private Win32Interop.Rect _dragStartRect;
    private IntPtr _dragHwnd;

    private void RootBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        var helper = new WindowInteropHelper(this);
        _dragHwnd = helper.Handle;
        Win32Interop.GetCursorPos(out _dragStartCursor);
        Win32Interop.GetWindowRect(_dragHwnd, out _dragStartRect);
        
        RootBorder.CaptureMouse();
        e.Handled = true;
    }

    private void RootBorder_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
            return;

        Win32Interop.GetCursorPos(out var current);
        var dx = current.X - _dragStartCursor.X;
        var dy = current.Y - _dragStartCursor.Y;

        var newLeft = _dragStartRect.Left + dx;
        var newTop = _dragStartRect.Top + dy;

        Win32Interop.SetWindowPos(_dragHwnd, IntPtr.Zero, 
            newLeft, newTop, 0, 0,
            Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOZORDER);
    }

    private void RootBorder_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        RootBorder.ReleaseMouseCapture();
    }
}
```

### 3. Context Menu

```xaml
<Border.ContextMenu>
    <ContextMenu Background="#FF101018" 
                 BorderBrush="#FF2A2A3A"
                 Foreground="#FFF1F5F9">
        <MenuItem Header="Settings..."
                  Click="Settings_Click"
                  Foreground="#FFF1F5F9">
            <MenuItem.Icon>
                <TextBlock Text="&#xE713;"
                           FontFamily="Segoe MDL2 Assets"
                           FontSize="14"
                           Foreground="#FF2DD4BF"/>
            </MenuItem.Icon>
        </MenuItem>
        <Separator Background="#FF2A2A3A"/>
        <MenuItem Header="Close Widget"
                  Click="CloseWidget_Click"
                  Foreground="#FFF1F5F9">
            <MenuItem.Icon>
                <TextBlock Text="&#xE711;"
                           FontFamily="Segoe MDL2 Assets"
                           FontSize="14"
                           Foreground="#FFFB7185"/>
            </MenuItem.Icon>
        </MenuItem>
    </ContextMenu>
</Border.ContextMenu>
```

```csharp
private void RootBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
{
    RootBorder.ContextMenu.IsOpen = true;
    e.Handled = true;
}

private void Settings_Click(object sender, RoutedEventArgs e)
{
    var settingsWindow = new ClockSettingsWindow(_currentSettings)
    {
        Owner = this,
        WindowStartupLocation = WindowStartupLocation.CenterOwner
    };

    if (settingsWindow.ShowDialog() == true)
    {
        ApplySettings(settingsWindow.UpdatedSettings);
    }
}

private void CloseWidget_Click(object sender, RoutedEventArgs e)
{
    this.Close();
}
```

### 4. Timers and Updates

```csharp
public partial class ClockWidgetWindow : Window
{
    private readonly DispatcherTimer _timer;

    public ClockWidgetWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        UpdateTime();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        var now = DateTime.Now;
        TimeTextBlock.Text = now.ToString("hh:mm:ss tt");
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _timer?.Stop();
        base.OnClosing(e);
    }
}
```

### 5. Window Positioning

```csharp
private void ClockWidget_Loaded(object? sender, RoutedEventArgs e)
{
    var helper = new WindowInteropHelper(this);
    var hwnd = helper.Handle;

    // Hide from Alt+Tab
    Win32Interop.MakeToolWindowNoActivate(hwnd);

    // Show window without activating
    Win32Interop.ShowWindowNoActivate(hwnd);

    // Place behind normal windows
    Win32Interop.SendToBottom(hwnd);
}
```

## Win32 Interop Helper

```csharp
public static class Win32Interop
{
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOZORDER = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
```

## Complete Window Template

See the Clock widget implementation in `3SC.Widgets.Clock/ClockWidgetWindow.xaml` for a complete, production-ready example with all features integrated.
