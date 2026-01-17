using System;
using System.Linq;
using System.Windows;
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;

namespace _3SC.Widgets.VideoViewer;

public static class DropFileBehavior
{
    public static readonly DependencyProperty DropHandlerProperty =
        DependencyProperty.RegisterAttached(
            "DropHandler",
            typeof(System.Action<string[]>),
            typeof(DropFileBehavior),
            new PropertyMetadata(null, OnDropHandlerChanged));

    public static readonly DependencyProperty FileExtensionProperty =
        DependencyProperty.RegisterAttached(
            "FileExtension",
            typeof(string),
            typeof(DropFileBehavior),
            new PropertyMetadata(null));

    public static void SetDropHandler(UIElement element, System.Action<string[]> value)
    {
        element.SetValue(DropHandlerProperty, value);
    }

    public static System.Action<string[]>? GetDropHandler(UIElement element)
    {
        return (System.Action<string[]>?)element.GetValue(DropHandlerProperty);
    }

    public static void SetFileExtension(UIElement element, string value)
    {
        element.SetValue(FileExtensionProperty, value);
    }

    public static string? GetFileExtension(UIElement element)
    {
        return (string?)element.GetValue(FileExtensionProperty);
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
            var filteredFiles = FilterFiles(element, files);
            if (filteredFiles.Length > 0)
            {
                var handler = GetDropHandler(element);
                handler?.Invoke(filteredFiles);
            }
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

    private static string[] FilterFiles(UIElement element, string[] files)
    {
        var extensionFilter = GetFileExtension(element);
        if (string.IsNullOrWhiteSpace(extensionFilter))
            return files;

        var allowedExtensions = extensionFilter
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => !string.IsNullOrEmpty(e))
            .ToHashSet();

        return files
            .Where(f => allowedExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
            .ToArray();
    }
}
