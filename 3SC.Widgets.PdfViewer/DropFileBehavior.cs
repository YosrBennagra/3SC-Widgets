using System;
using System.Windows;

namespace _3SC.Widgets.PdfViewer;

public static class DropFileBehavior
{
    public static readonly DependencyProperty DropHandlerProperty =
        DependencyProperty.RegisterAttached(
            "DropHandler",
            typeof(System.Action<string[]>),
            typeof(DropFileBehavior),
            new PropertyMetadata(null, OnDropHandlerChanged));

    public static void SetDropHandler(UIElement element, System.Action<string[]> value)
    {
        element.SetValue(DropHandlerProperty, value);
    }

    public static System.Action<string[]>? GetDropHandler(UIElement element)
    {
        return (System.Action<string[]>?)element.GetValue(DropHandlerProperty);
    }

    private static void OnDropHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.Drop -= Element_Drop;
            element.DragEnter -= Element_DragEnter;
            element.DragOver -= Element_DragOver;

            if (e.NewValue is not null)
            {
                element.AllowDrop = true;
                element.Drop += Element_Drop;
                element.DragEnter += Element_DragEnter;
                element.DragOver += Element_DragOver;
            }
        }
    }

    private static void Element_Drop(object sender, DragEventArgs e)
    {
        if (sender is UIElement element && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var handler = GetDropHandler(element);
            handler?.Invoke(files);
        }
    }

    private static void Element_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private static void Element_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }
}
