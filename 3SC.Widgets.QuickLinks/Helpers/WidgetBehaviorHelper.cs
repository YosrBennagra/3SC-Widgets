using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace _3SC.Widgets.QuickLinks.Helpers;

public class DragState
{
    public bool IsDragging { get; set; }
    public IntPtr Hwnd { get; set; }
    public int StartCursorX { get; set; }
    public int StartCursorY { get; set; }
    public int StartRectLeft { get; set; }
    public int StartRectTop { get; set; }
    public int StartRectRight { get; set; }
    public int StartRectBottom { get; set; }
}

public static class WidgetBehaviorHelper
{
    public static void ConfigureAsDesktopWidget(Window window)
    {
        var helper = new WindowInteropHelper(window);
        var hwnd = helper.Handle;
        if (hwnd == IntPtr.Zero) return;
        Win32Interop.MakeToolWindowNoActivate(hwnd);
        Win32Interop.ShowWindowNoActivate(hwnd);
        Win32Interop.SendToBottom(hwnd);
    }

    public static void StartSmoothDrag(Window window, UIElement captureElement, DragState dragState, bool isLocked, MouseButtonEventArgs e)
    {
        if (isLocked) return;

        dragState.IsDragging = true;
        var helper = new WindowInteropHelper(window);
        dragState.Hwnd = helper.Handle;
        Win32Interop.GetCursorPos(out var cursor);
        Win32Interop.GetWindowRect(dragState.Hwnd, out var rect);
        dragState.StartCursorX = cursor.X;
        dragState.StartCursorY = cursor.Y;
        dragState.StartRectLeft = rect.Left;
        dragState.StartRectTop = rect.Top;
        dragState.StartRectRight = rect.Right;
        dragState.StartRectBottom = rect.Bottom;

        captureElement.CaptureMouse();
        e.Handled = true;
    }

    public static void HandleSmoothDragMove(DragState dragState, System.Windows.Input.MouseEventArgs e)
    {
        if (!dragState.IsDragging || e.LeftButton != MouseButtonState.Pressed) return;
        if (dragState.Hwnd == IntPtr.Zero) return;

        Win32Interop.GetCursorPos(out var current);
        var dx = current.X - dragState.StartCursorX;
        var dy = current.Y - dragState.StartCursorY;

        var newLeft = dragState.StartRectLeft + dx;
        var newTop = dragState.StartRectTop + dy;

        var width = dragState.StartRectRight - dragState.StartRectLeft;
        var height = dragState.StartRectBottom - dragState.StartRectTop;
        var constrained = ScreenBoundsHelper.ConstrainToScreenBounds(newLeft, newTop, width, height);

        Win32Interop.SetWindowPos(
            dragState.Hwnd,
            IntPtr.Zero,
            constrained.X,
            constrained.Y,
            0,
            0,
            Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOACTIVATE | Win32Interop.SWP_NOZORDER);

        e.Handled = true;
    }

    public static void EndSmoothDrag(UIElement captureElement, DragState dragState, Action saveCallback, MouseButtonEventArgs e)
    {
        if (!dragState.IsDragging) return;
        dragState.IsDragging = false;
        captureElement.ReleaseMouseCapture();
        dragState.Hwnd = IntPtr.Zero;
        saveCallback();
        e.Handled = true;
    }

    public static bool HandleLockToggle(
        bool currentLockState,
        MenuItem lockMenuItem,
        MenuItem? resizeMenuItem,
        ref bool resizeHandlesVisible,
        Action<bool> setResizeVisibility,
        Action saveCallback)
    {
        var newLockState = !currentLockState;
        lockMenuItem.IsChecked = newLockState;

        if (newLockState)
        {
            resizeHandlesVisible = false;
            if (resizeMenuItem is { } menuItem)
            {
                menuItem.IsChecked = false;
            }

            setResizeVisibility(false);
        }

        saveCallback();
        return newLockState;
    }

    public static bool HandleResizeToggle(
        bool currentVisibility,
        MenuItem resizeMenuItem,
        Action<bool> setResizeVisibility)
    {
        var newVisibility = !currentVisibility;
        resizeMenuItem.IsChecked = newVisibility;
        setResizeVisibility(newVisibility);
        return newVisibility;
    }

    public static void SetResizeHandlesVisibility(
        bool visible,
        System.Windows.Shapes.Rectangle resizeOutline,
        Thumb resizeTop,
        Thumb resizeBottom,
        Thumb resizeLeft,
        Thumb resizeRight)
    {
        var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        resizeOutline.Visibility = visibility;
        resizeTop.Visibility = visibility;
        resizeBottom.Visibility = visibility;
        resizeLeft.Visibility = visibility;
        resizeRight.Visibility = visibility;
    }

    public static void HandleResizeTop(Window window, bool isLocked, double verticalChange, double minHeight = 100)
    {
        if (isLocked) return;
        var newHeight = window.Height - verticalChange;
        if (newHeight >= minHeight)
        {
            window.Top += verticalChange;
            window.Height = newHeight;
        }
    }

    public static void HandleResizeBottom(Window window, bool isLocked, double verticalChange, double minHeight = 100)
    {
        if (isLocked) return;
        var newHeight = window.Height + verticalChange;
        if (newHeight >= minHeight)
        {
            window.Height = newHeight;
        }
    }

    public static void HandleResizeLeft(Window window, bool isLocked, double horizontalChange, double minWidth = 200)
    {
        if (isLocked) return;
        var newWidth = window.Width - horizontalChange;
        if (newWidth >= minWidth)
        {
            window.Left += horizontalChange;
            window.Width = newWidth;
        }
    }

    public static void HandleResizeRight(Window window, bool isLocked, double horizontalChange, double minWidth = 200)
    {
        if (isLocked) return;
        var newWidth = window.Width + horizontalChange;
        if (newWidth >= minWidth)
        {
            window.Width = newWidth;
        }
    }

    public static void HandleRemoveWidget(Guid widgetInstanceId, string widgetKey, Window window)
    {
        try
        {
            window.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error closing widget: {ex.Message}");
        }
    }

    public static async System.Threading.Tasks.Task SaveWidgetPositionAsync(Guid widgetInstanceId, Window window, bool isLocked)
    {
        await System.Threading.Tasks.Task.CompletedTask;
    }
}
