using System;
using System.Linq;
using System.Windows;
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;

namespace _3SC.Widgets.ImageViewer;

/// <summary>
/// Attached behavior for handling file drop operations on UI elements.
/// </summary>
public static class DropFileBehavior
{
    #region DropHandler Attached Property

    /// <summary>
    /// Gets the drop handler delegate.
    /// </summary>
    public static Action<string[]>? GetDropHandler(DependencyObject obj)
    {
        return (Action<string[]>?)obj.GetValue(DropHandlerProperty);
    }

    /// <summary>
    /// Sets the drop handler delegate.
    /// </summary>
    public static void SetDropHandler(DependencyObject obj, Action<string[]>? value)
    {
        obj.SetValue(DropHandlerProperty, value);
    }

    /// <summary>
    /// Attached property for the drop handler delegate.
    /// </summary>
    public static readonly DependencyProperty DropHandlerProperty =
        DependencyProperty.RegisterAttached(
            "DropHandler",
            typeof(Action<string[]>),
            typeof(DropFileBehavior),
            new PropertyMetadata(null, OnDropHandlerChanged));

    #endregion

    #region FileExtension Attached Property

    /// <summary>
    /// Gets the file extension filter (e.g., ".jpg").
    /// </summary>
    public static string GetFileExtension(DependencyObject obj)
    {
        return (string)obj.GetValue(FileExtensionProperty);
    }

    /// <summary>
    /// Sets the file extension filter.
    /// </summary>
    public static void SetFileExtension(DependencyObject obj, string value)
    {
        obj.SetValue(FileExtensionProperty, value);
    }

    /// <summary>
    /// Attached property for file extension filtering.
    /// </summary>
    public static readonly DependencyProperty FileExtensionProperty =
        DependencyProperty.RegisterAttached(
            "FileExtension",
            typeof(string),
            typeof(DropFileBehavior),
            new PropertyMetadata(string.Empty));

    #endregion

    private static void OnDropHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if (e.NewValue != null)
            {
                element.AllowDrop = true;
                element.Drop += Element_Drop;
                element.DragOver += Element_DragOver;
            }
            else if (e.OldValue != null)
            {
                element.Drop -= Element_Drop;
                element.DragOver -= Element_DragOver;
            }
        }
    }

    private static void Element_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length == 0)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        // Check file extension filter if specified
        if (sender is DependencyObject obj)
        {
            var extension = GetFileExtension(obj);
            if (!string.IsNullOrEmpty(extension))
            {
                var hasValidFile = files.Any(file =>
                    file.EndsWith(extension, StringComparison.OrdinalIgnoreCase));

                if (!hasValidFile)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
            }
        }

        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private static void Element_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length == 0)
        {
            return;
        }

        if (sender is DependencyObject obj)
        {
            var handler = GetDropHandler(obj);
            handler?.Invoke(files);
        }

        e.Handled = true;
    }
}
