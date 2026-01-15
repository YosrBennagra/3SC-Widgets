using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using _3SC.ViewModels;

namespace _3SC.Widgets;

public partial class QuickLinksWidget : WidgetWindowBase
{
    public QuickLinksWidget()
    {
        InitializeComponent();

        var viewModel = new QuickLinksWidgetViewModel();
        DataContext = viewModel;
    }

    protected override bool IsDragBlocked(DependencyObject? source)
    {
        return source is not null && IsInteractiveControl(source);
    }

    private static bool IsInteractiveControl(DependencyObject element)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current is System.Windows.Controls.Button or Slider or Thumb or System.Windows.Controls.ListBox or ListBoxItem or System.Windows.Controls.Primitives.ScrollBar or ScrollViewer or System.Windows.Controls.TextBox)
            {
                return true;
            }

            current = current is Visual
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    // Window interaction handlers referenced by XAML
    private void Border_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && !IsDragBlocked(e.OriginalSource as DependencyObject))
        {
            try { DragMove(); } catch { }
        }
    }

    private void Border_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // No-op placeholder for XAML wiring
    }

    private void Border_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // No-op placeholder for XAML wiring
    }

    private void LockWidget_Click(object sender, RoutedEventArgs e)
    {
        bool isLocked = false;
        if (sender is MenuItem mi)
        {
            mi.IsChecked = !mi.IsChecked;
            isLocked = mi.IsChecked;
        }
        else if (LockWidgetMenuItem != null)
        {
            isLocked = LockWidgetMenuItem.IsChecked;
        }

        var vis = isLocked ? Visibility.Collapsed : Visibility.Visible;
        if (ResizeTop != null) ResizeTop.Visibility = vis;
        if (ResizeBottom != null) ResizeBottom.Visibility = vis;
        if (ResizeLeft != null) ResizeLeft.Visibility = vis;
        if (ResizeRight != null) ResizeRight.Visibility = vis;
    }

    private void ResizeToggle_Click(object sender, RoutedEventArgs e)
    {
        bool show = false;
        if (sender is MenuItem mi) show = mi.IsChecked;
        else if (ResizeToggleMenuItem != null) show = ResizeToggleMenuItem.IsChecked;

        var vis = show ? Visibility.Visible : Visibility.Collapsed;
        if (ResizeTop != null) ResizeTop.Visibility = vis;
        if (ResizeBottom != null) ResizeBottom.Visibility = vis;
        if (ResizeLeft != null) ResizeLeft.Visibility = vis;
        if (ResizeRight != null) ResizeRight.Visibility = vis;
    }

    private void RemoveWidget_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResizeTop_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newHeight = Height - e.VerticalChange;
        double newTop = Top + e.VerticalChange;
        if (newHeight >= MinHeight)
        {
            Height = newHeight;
            Top = newTop;
        }
    }

    private void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newHeight = Height + e.VerticalChange;
        if (newHeight >= MinHeight) Height = newHeight;
    }

    private void ResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newWidth = Width - e.HorizontalChange;
        double newLeft = Left + e.HorizontalChange;
        if (newWidth >= MinWidth)
        {
            Width = newWidth;
            Left = newLeft;
        }
    }

    private void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newWidth = Width + e.HorizontalChange;
        if (newWidth >= MinWidth) Width = newWidth;
    }
}
