# Drag & Resize Behavior

> **Category:** UI | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers implementing drag-to-move and resize functionality for widgets.

## Prerequisites

- [xaml-styling.md](xaml-styling.md)

---

## Basic Drag Implementation

### Code-Behind Approach

```csharp
using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.MyWidget;

public partial class MyWidgetWindow : Window
{
    private bool _isDragging;
    private Point _clickPosition;
    
    public MyWidgetWindow()
    {
        InitializeComponent();
    }
    
    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _clickPosition = e.GetPosition(this);
        
        if (sender is UIElement element)
        {
            element.CaptureMouse();
        }
        
        e.Handled = true;
    }
    
    private void DragArea_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
            return;
        
        var currentPosition = e.GetPosition(this);
        var offset = currentPosition - _clickPosition;
        
        Left += offset.X;
        Top += offset.Y;
    }
    
    private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        
        if (sender is UIElement element)
        {
            element.ReleaseMouseCapture();
        }
        
        // Notify position changed (for persistence)
        OnPositionChanged();
    }
    
    private void OnPositionChanged()
    {
        // Save position or notify host
        PositionChanged?.Invoke(this, new PositionEventArgs(Left, Top));
    }
    
    public event EventHandler<PositionEventArgs>? PositionChanged;
}

public class PositionEventArgs : EventArgs
{
    public double X { get; }
    public double Y { get; }
    
    public PositionEventArgs(double x, double y)
    {
        X = x;
        Y = y;
    }
}
```

### XAML Setup

```xml
<Window x:Class="_3SC.Widgets.MyWidget.MyWidgetWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent">
    
    <Border Background="{StaticResource WidgetSurface}"
            CornerRadius="8"
            BorderBrush="{StaticResource Border}"
            BorderThickness="1">
        
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>
            
            <!-- Drag Area (Title Bar) -->
            <Border x:Name="TitleBar"
                    Grid.Row="0"
                    Background="Transparent"
                    Cursor="SizeAll"
                    MouseLeftButtonDown="DragArea_MouseLeftButtonDown"
                    MouseMove="DragArea_MouseMove"
                    MouseLeftButtonUp="DragArea_MouseLeftButtonUp">
                <TextBlock Text="My Widget" 
                           Margin="12,8"
                           Foreground="{StaticResource TextPrimary}"/>
            </Border>
            
            <!-- Content (Not draggable) -->
            <Grid Grid.Row="1" Margin="12">
                <!-- Widget content here -->
            </Grid>
        </Grid>
    </Border>
</Window>
```

---

## Attached Behavior Approach

### DragBehavior Class

```csharp
using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.MyWidget.Behaviors;

/// <summary>
/// Attached behavior for making elements draggable.
/// </summary>
public static class DragBehavior
{
    #region Attached Property
    
    public static readonly DependencyProperty IsDraggableProperty =
        DependencyProperty.RegisterAttached(
            "IsDraggable",
            typeof(bool),
            typeof(DragBehavior),
            new PropertyMetadata(false, OnIsDraggableChanged));
    
    public static bool GetIsDraggable(DependencyObject obj) =>
        (bool)obj.GetValue(IsDraggableProperty);
    
    public static void SetIsDraggable(DependencyObject obj, bool value) =>
        obj.SetValue(IsDraggableProperty, value);
    
    #endregion
    
    #region State
    
    private static bool _isDragging;
    private static Point _clickPosition;
    
    #endregion
    
    #region Event Handlers
    
    private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;
        
        if ((bool)e.NewValue)
        {
            element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            element.MouseMove += Element_MouseMove;
            element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
        }
        else
        {
            element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            element.MouseMove -= Element_MouseMove;
            element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
        }
    }
    
    private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element)
            return;
        
        var window = Window.GetWindow(element);
        if (window == null)
            return;
        
        _isDragging = true;
        _clickPosition = e.GetPosition(window);
        element.CaptureMouse();
        e.Handled = true;
    }
    
    private static void Element_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
            return;
        
        if (sender is not UIElement element)
            return;
        
        var window = Window.GetWindow(element);
        if (window == null)
            return;
        
        var currentPosition = e.GetPosition(window);
        var offset = currentPosition - _clickPosition;
        
        window.Left += offset.X;
        window.Top += offset.Y;
    }
    
    private static void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement element)
        {
            element.ReleaseMouseCapture();
        }
        
        _isDragging = false;
    }
    
    #endregion
}
```

### Using the Behavior

```xml
<Window xmlns:behaviors="clr-namespace:_3SC.Widgets.MyWidget.Behaviors">
    
    <Border behaviors:DragBehavior.IsDraggable="True"
            Cursor="SizeAll">
        <!-- Content here -->
    </Border>
    
</Window>
```

---

## Smooth Win32 Drag (Recommended)

For smoother dragging, use Win32 interop:

### Win32Interop Class

```csharp
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace _3SC.Widgets.MyWidget;

/// <summary>
/// Win32 interop for smooth window operations.
/// </summary>
public static class Win32Interop
{
    #region Win32 Imports
    
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int HT_CAPTION = 0x0002;
    
    #endregion
    
    /// <summary>
    /// Initiates a window drag using Win32, which is smoother than WPF dragging.
    /// </summary>
    public static void DragMove(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        ReleaseCapture();
        SendMessage(handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
    }
}
```

### Using Win32 Drag

```csharp
public partial class MyWidgetWindow : Window
{
    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Win32Interop.DragMove(this);
        }
    }
}
```

```xml
<Border Background="Transparent"
        MouseLeftButtonDown="DragArea_MouseLeftButtonDown"
        Cursor="SizeAll">
    <TextBlock Text="Drag Me"/>
</Border>
```

---

## Resize Implementation

### Resize Grips

```csharp
public partial class MyWidgetWindow : Window
{
    private bool _isResizing;
    private Point _startPoint;
    private Size _startSize;
    private ResizeDirection _resizeDirection;
    
    private enum ResizeDirection
    {
        None, TopLeft, Top, TopRight,
        Left, Right, BottomLeft, Bottom, BottomRight
    }
    
    private void ResizeGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;
        
        _isResizing = true;
        _startPoint = PointToScreen(e.GetPosition(this));
        _startSize = new Size(Width, Height);
        _resizeDirection = GetResizeDirection(element.Tag?.ToString());
        
        element.CaptureMouse();
        e.Handled = true;
    }
    
    private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isResizing || e.LeftButton != MouseButtonState.Pressed)
            return;
        
        var currentPoint = PointToScreen(e.GetPosition(this));
        var delta = currentPoint - _startPoint;
        
        ApplyResize(delta);
    }
    
    private void ResizeGrip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isResizing = false;
        
        if (sender is UIElement element)
        {
            element.ReleaseMouseCapture();
        }
        
        // Notify size changed
        OnSizeChanged();
    }
    
    private void ApplyResize(Vector delta)
    {
        var minWidth = 150.0;
        var minHeight = 100.0;
        var maxWidth = 800.0;
        var maxHeight = 600.0;
        
        switch (_resizeDirection)
        {
            case ResizeDirection.Right:
                Width = Math.Clamp(_startSize.Width + delta.X, minWidth, maxWidth);
                break;
                
            case ResizeDirection.Bottom:
                Height = Math.Clamp(_startSize.Height + delta.Y, minHeight, maxHeight);
                break;
                
            case ResizeDirection.BottomRight:
                Width = Math.Clamp(_startSize.Width + delta.X, minWidth, maxWidth);
                Height = Math.Clamp(_startSize.Height + delta.Y, minHeight, maxHeight);
                break;
                
            // Add other directions as needed...
        }
    }
    
    private ResizeDirection GetResizeDirection(string? tag) => tag switch
    {
        "TopLeft" => ResizeDirection.TopLeft,
        "Top" => ResizeDirection.Top,
        "TopRight" => ResizeDirection.TopRight,
        "Left" => ResizeDirection.Left,
        "Right" => ResizeDirection.Right,
        "BottomLeft" => ResizeDirection.BottomLeft,
        "Bottom" => ResizeDirection.Bottom,
        "BottomRight" => ResizeDirection.BottomRight,
        _ => ResizeDirection.None
    };
    
    private void OnSizeChanged()
    {
        SizeChanged?.Invoke(this, new SizeChangedEventArgs(Width, Height));
    }
}
```

### Resize Grip XAML

```xml
<Grid>
    <!-- Main content -->
    <Border x:Name="Content" Margin="4">
        <!-- Widget content -->
    </Border>
    
    <!-- Resize grips (only visible when widget is unlocked) -->
    <Canvas Visibility="{Binding IsLocked, Converter={StaticResource InvertedBoolToVisibility}}">
        
        <!-- Right edge -->
        <Rectangle Width="4" Height="{Binding ActualHeight, RelativeSource={RelativeSource AncestorType=Canvas}}"
                   Canvas.Right="0" Canvas.Top="0"
                   Fill="Transparent" Cursor="SizeWE"
                   Tag="Right"
                   MouseLeftButtonDown="ResizeGrip_MouseLeftButtonDown"
                   MouseMove="ResizeGrip_MouseMove"
                   MouseLeftButtonUp="ResizeGrip_MouseLeftButtonUp"/>
        
        <!-- Bottom edge -->
        <Rectangle Width="{Binding ActualWidth, RelativeSource={RelativeSource AncestorType=Canvas}}" Height="4"
                   Canvas.Left="0" Canvas.Bottom="0"
                   Fill="Transparent" Cursor="SizeNS"
                   Tag="Bottom"
                   MouseLeftButtonDown="ResizeGrip_MouseLeftButtonDown"
                   MouseMove="ResizeGrip_MouseMove"
                   MouseLeftButtonUp="ResizeGrip_MouseLeftButtonUp"/>
        
        <!-- Bottom-right corner -->
        <Rectangle Width="8" Height="8"
                   Canvas.Right="0" Canvas.Bottom="0"
                   Fill="Transparent" Cursor="SizeNWSE"
                   Tag="BottomRight"
                   MouseLeftButtonDown="ResizeGrip_MouseLeftButtonDown"
                   MouseMove="ResizeGrip_MouseMove"
                   MouseLeftButtonUp="ResizeGrip_MouseLeftButtonUp"/>
        
    </Canvas>
</Grid>
```

---

## Snap to Grid

```csharp
public static class SnapHelper
{
    private const int GridSize = 10;
    
    public static double SnapToGrid(double value)
    {
        return Math.Round(value / GridSize) * GridSize;
    }
    
    public static Point SnapToGrid(Point point)
    {
        return new Point(
            SnapToGrid(point.X),
            SnapToGrid(point.Y));
    }
}

// Usage in drag
window.Left = SnapHelper.SnapToGrid(window.Left + offset.X);
window.Top = SnapHelper.SnapToGrid(window.Top + offset.Y);
```

---

## Screen Boundary Constraints

```csharp
public static class ScreenBounds
{
    public static void ConstrainToScreen(Window window)
    {
        var screen = System.Windows.Forms.Screen.FromHandle(
            new WindowInteropHelper(window).Handle);
        var workingArea = screen.WorkingArea;
        
        // Keep at least 50px visible on screen
        var minVisible = 50.0;
        
        window.Left = Math.Max(workingArea.Left - window.Width + minVisible,
                      Math.Min(workingArea.Right - minVisible, window.Left));
        
        window.Top = Math.Max(workingArea.Top,
                     Math.Min(workingArea.Bottom - minVisible, window.Top));
    }
}
```

---

## Complete Draggable Window Example

```csharp
public partial class MyWidgetWindow : Window
{
    private bool _isDragging;
    private Point _startPoint;
    private Point _startPosition;
    
    public MyWidgetWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        
        // Only drag from title bar area (check visual hit test)
        var hitElement = e.OriginalSource as DependencyObject;
        if (!IsInDragArea(hitElement))
            return;
        
        _isDragging = true;
        _startPoint = PointToScreen(e.GetPosition(this));
        _startPosition = new Point(Left, Top);
        
        CaptureMouse();
        e.Handled = true;
    }
    
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        
        if (!_isDragging)
            return;
        
        var currentPoint = PointToScreen(e.GetPosition(this));
        var offset = currentPoint - _startPoint;
        
        Left = _startPosition.X + offset.X;
        Top = _startPosition.Y + offset.Y;
        
        // Constrain to screen
        ScreenBounds.ConstrainToScreen(this);
    }
    
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        
        if (!_isDragging)
            return;
        
        _isDragging = false;
        ReleaseMouseCapture();
        
        // Save position
        OnPositionChanged();
    }
    
    private bool IsInDragArea(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is FrameworkElement fe && fe.Name == "DragArea")
                return true;
            
            element = VisualTreeHelper.GetParent(element);
        }
        return false;
    }
    
    private void OnPositionChanged()
    {
        // Save to settings or notify host
    }
}
```

---

## Best Practices

1. **Use Win32 drag** for smoothest experience
2. **Constrain to screen** to prevent widgets from being lost
3. **Save position** on mouse up, not during move
4. **Indicate drag area** with cursor change
5. **Debounce saves** to avoid excessive I/O

---

## Related Skills

- [xaml-styling.md](xaml-styling.md) - XAML patterns
- [settings-management.md](../data/settings-management.md) - Saving position

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added Win32 drag, resize grips |
| 1.0.0 | 2025-06-01 | Initial version |
