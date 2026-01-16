using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using _3SC.Widgets.Clock.Helpers;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace _3SC.Widgets.Clock;

public readonly record struct WidgetWindowInit(
    Guid WidgetInstanceId,
    double Left,
    double Top,
    double Width,
    double Height,
    bool IsLocked,
    bool ConstrainToScreen = true);

public sealed record class WidgetWindowParts(
    MenuItem? LockWidgetMenuItem = null,
    MenuItem? ResizeToggleMenuItem = null,
    System.Windows.Shapes.Rectangle? ResizeOutlineElement = null,
    Thumb? ResizeTopThumb = null,
    Thumb? ResizeBottomThumb = null,
    Thumb? ResizeLeftThumb = null,
    Thumb? ResizeRightThumb = null,
    string? WidgetKey = null);

public abstract class WidgetWindowBase : Window
{
    private readonly DragState _dragState = new();
    private bool _resizeHandlesVisible;
    private MenuItem? _lockWidgetMenuItem;
    private MenuItem? _resizeToggleMenuItem;
    private System.Windows.Shapes.Rectangle? _resizeOutlineElement;
    private Thumb? _resizeTopThumb;
    private Thumb? _resizeBottomThumb;
    private Thumb? _resizeLeftThumb;
    private Thumb? _resizeRightThumb;
    private string? _widgetKey;

    protected bool IsLocked { get; private set; }
    protected Guid WidgetInstanceId { get; private set; }

    protected WidgetWindowBase()
    {
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    protected void InitializeWidgetWindow(WidgetWindowInit init, WidgetWindowParts? parts = null)
    {
        WidgetInstanceId = init.WidgetInstanceId;
        IsLocked = init.IsLocked;
        _lockWidgetMenuItem = parts?.LockWidgetMenuItem;
        _resizeToggleMenuItem = parts?.ResizeToggleMenuItem;
        _resizeOutlineElement = parts?.ResizeOutlineElement;
        _resizeTopThumb = parts?.ResizeTopThumb;
        _resizeBottomThumb = parts?.ResizeBottomThumb;
        _resizeLeftThumb = parts?.ResizeLeftThumb;
        _resizeRightThumb = parts?.ResizeRightThumb;
        _widgetKey = parts?.WidgetKey;

        if (_lockWidgetMenuItem is { } lockMenuItem)
            lockMenuItem.IsChecked = init.IsLocked;

        if (init.ConstrainToScreen)
        {
            var constrained = ScreenBoundsHelper.ConstrainToScreenBounds(
                (int)init.Left, (int)init.Top, (int)init.Width, (int)init.Height);
            Left = constrained.X;
            Top = constrained.Y;
        }
        else
        {
            Left = init.Left;
            Top = init.Top;
        }
        Width = init.Width;
        Height = init.Height;
    }

    protected virtual MenuItem? LockWidgetMenuItemControl => _lockWidgetMenuItem;
    protected virtual MenuItem? ResizeToggleMenuItemControl => _resizeToggleMenuItem;
    protected virtual System.Windows.Shapes.Rectangle? ResizeOutlineElement => _resizeOutlineElement;
    protected virtual Thumb? ResizeTopThumb => _resizeTopThumb;
    protected virtual Thumb? ResizeBottomThumb => _resizeBottomThumb;
    protected virtual Thumb? ResizeLeftThumb => _resizeLeftThumb;
    protected virtual Thumb? ResizeRightThumb => _resizeRightThumb;

    public virtual string WidgetKey => _widgetKey ?? string.Empty;
    protected virtual double MinWidgetWidth => 200;
    protected virtual double MinWidgetHeight => 80;
    protected virtual bool SaveOnResize => false;
    protected virtual bool IsDragBlocked(DependencyObject? source) => false;

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        WidgetBehaviorHelper.ConfigureAsDesktopWidget(this);
        await OnWidgetLoadedAsync();
    }

    private async void OnClosing(object? sender, CancelEventArgs e) => await OnWidgetClosingAsync();

    protected virtual Task OnWidgetLoadedAsync() => Task.CompletedTask;
    protected virtual Task OnWidgetClosingAsync() => SavePositionAsync();

    protected void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsDragBlocked(e.OriginalSource as DependencyObject)) return;
        if (sender is FrameworkElement element)
            WidgetBehaviorHelper.StartSmoothDrag(this, element, _dragState, IsLocked, e);
    }

    protected void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        => WidgetBehaviorHelper.HandleSmoothDragMove(_dragState, e);

    protected void Border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
            WidgetBehaviorHelper.EndSmoothDrag(element, _dragState, () => _ = SavePositionAsync(), e);
    }

    protected void LockWidget_Click(object sender, RoutedEventArgs e)
    {
        if (LockWidgetMenuItemControl is not { } lockMenuItem) return;
        IsLocked = WidgetBehaviorHelper.HandleLockToggle(IsLocked, lockMenuItem, ResizeToggleMenuItemControl,
            ref _resizeHandlesVisible, v => SetResizeHandlesVisibility(this, v), () => _ = SavePositionAsync());
    }

    protected void ResizeToggle_Click(object sender, RoutedEventArgs e)
    {
        if (ResizeToggleMenuItemControl is not { } resizeMenuItem) return;
        _resizeHandlesVisible = WidgetBehaviorHelper.HandleResizeToggle(_resizeHandlesVisible, resizeMenuItem,
            v => SetResizeHandlesVisibility(this, v));
    }

    protected void RemoveWidget_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(WidgetKey))
            WidgetBehaviorHelper.HandleRemoveWidget(WidgetInstanceId, WidgetKey, this);
    }

    protected virtual Task SavePositionAsync()
    {
        if (WidgetInstanceId == Guid.Empty) return Task.CompletedTask;
        return WidgetBehaviorHelper.SaveWidgetPositionAsync(WidgetInstanceId, this, IsLocked);
    }

    protected virtual void ResizeTop_DragDelta(object sender, DragDeltaEventArgs e)
    {
        WidgetBehaviorHelper.HandleResizeTop(this, IsLocked, e.VerticalChange, MinWidgetHeight);
        if (SaveOnResize) _ = SavePositionAsync();
    }

    protected virtual void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e)
    {
        WidgetBehaviorHelper.HandleResizeBottom(this, IsLocked, e.VerticalChange, MinWidgetHeight);
        if (SaveOnResize) _ = SavePositionAsync();
    }

    protected virtual void ResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
    {
        WidgetBehaviorHelper.HandleResizeLeft(this, IsLocked, e.HorizontalChange, MinWidgetWidth);
        if (SaveOnResize) _ = SavePositionAsync();
    }

    protected virtual void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
    {
        WidgetBehaviorHelper.HandleResizeRight(this, IsLocked, e.HorizontalChange, MinWidgetWidth);
        if (SaveOnResize) _ = SavePositionAsync();
    }

    protected static void SetResizeHandlesVisibility(WidgetWindowBase instance, bool visible)
    {
        if (instance.ResizeOutlineElement is not { } resizeOutline ||
            instance.ResizeTopThumb is not { } resizeTop ||
            instance.ResizeBottomThumb is not { } resizeBottom ||
            instance.ResizeLeftThumb is not { } resizeLeft ||
            instance.ResizeRightThumb is not { } resizeRight) return;

        WidgetBehaviorHelper.SetResizeHandlesVisibility(visible, resizeOutline, resizeTop, resizeBottom, resizeLeft, resizeRight);
    }
}
