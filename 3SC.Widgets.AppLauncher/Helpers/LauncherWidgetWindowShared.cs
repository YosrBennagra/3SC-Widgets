using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using _3SC.Widgets.AppLauncher.Abstractions;
using _3SC.Widgets.AppLauncher.Helpers;
using Serilog;

namespace _3SC.Widgets.AppLauncher.Helpers;

public static class LauncherWidgetWindowShared
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(LauncherWidgetWindowShared));

    public static void AddFromPath<TItem>(
        string path,
        ObservableCollection<TItem> collection,
        Action refresh,
        Action save)
        where TItem : class, ILauncherItem, new()
    {
        var item = CreateItemFromPath<TItem>(path);
        if (item == null)
        {
            Log.Warning("Failed to create item from path: {Path}", path);
            return;
        }

        if (collection.Any(existingItem => string.Equals(existingItem.Path, item.Path, StringComparison.OrdinalIgnoreCase)))
        {
            Log.Debug("App already exists, skipping duplicate: {Path}", item.Path);
            return;
        }

        collection.Add(item);
        Log.Information("Added app: {Name} at {Path}", item.Name, item.Path);
        refresh();
        save();
    }

    public static void UpdateItemsDisplay<TItem>(
        bool showFavoritesOnly,
        IEnumerable<TItem> sourceItems,
        Func<TItem, bool> isFavorite,
        UIElement emptyState,
        UIElement scrollViewer,
        ItemsControl itemsControl,
        Action updateTabVisuals)
    {
        var filteredItems = showFavoritesOnly
            ? sourceItems.Where(isFavorite).ToList()
            : sourceItems.ToList();

        emptyState.Visibility = filteredItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        scrollViewer.Visibility = filteredItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        itemsControl.ItemsSource = filteredItems;

        updateTabVisuals();
    }

    public static void HandleFileDrop(System.Windows.DragEventArgs e, Action<string> handlePath)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            foreach (var file in files)
            {
                handlePath(file);
            }
        }

        e.Handled = true;
    }

    public static void HandleDragOver(System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    public static TItem? CreateItemFromPath<TItem>(string path)
        where TItem : class, ILauncherItem, new()
    {
        try
        {
            var targetPath = path;
            var displayName = Path.GetFileNameWithoutExtension(path);

            if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                targetPath = LauncherWidgetHelpers.ResolveShortcut(path) ?? path;
                Log.Debug("Resolved shortcut {Shortcut} to {Target}", path, targetPath);
            }

            var iconPath = File.Exists(targetPath) ? targetPath : path;
            var icon = LauncherWidgetHelpers.TryLoadIcon(iconPath);

            if (icon == null)
            {
                Log.Warning("Failed to extract icon for: {Path}", iconPath);
            }

            return new TItem
            {
                Name = displayName,
                Path = targetPath,
                Icon = icon,
                IsFavorite = false
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create item from path: {Path}", path);
            return null;
        }
    }

    public static void LaunchItem(ILauncherItem item)
    {
        try
        {
            Log.Information("Launching app: {Name} at {Path}", item.Name, item.Path);
            Process.Start(new ProcessStartInfo
            {
                FileName = item.Path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch: {Name} at {Path}", item.Name, item.Path);
        }
    }

    public static void ToggleFavorite<TItem>(object sender, Action refresh, Action save)
        where TItem : class, ILauncherItem
    {
        if (sender is FrameworkElement { DataContext: TItem item })
        {
            item.IsFavorite = !item.IsFavorite;
            Log.Debug("Toggled favorite for {Name}: {IsFavorite}", item.Name, item.IsFavorite);
            refresh();
            save();
        }
    }

    public static void RemoveItem<TItem>(object sender, ObservableCollection<TItem> collection, Action refresh, Action save)
        where TItem : class
    {
        if (sender is FrameworkElement { DataContext: TItem item })
        {
            collection.Remove(item);
            Log.Information("Removed item from collection");
            refresh();
            save();
        }
    }

    public static bool IsDragBlocked(
        DependencyObject? source,
        Thumb resizeTop,
        Thumb resizeBottom,
        Thumb resizeLeft,
        Thumb resizeRight,
        params DependencyObject[] blockedElements)
    {
        while (source != null)
        {
            if (source is Thumb thumb && (thumb == resizeTop || thumb == resizeBottom || thumb == resizeLeft || thumb == resizeRight))
            {
                return true;
            }

            if (blockedElements.Any(be => ReferenceEquals(be, source)))
            {
                return true;
            }

            if (source is FrameworkElement { DataContext: ILauncherItem })
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    public static void ShowAddDialog(Action<string> addCallback)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Executables (*.exe;*.lnk)|*.exe;*.lnk|All files (*.*)|*.*",
            Title = "Select an application"
        };

        if (dialog.ShowDialog() == true)
        {
            addCallback(dialog.FileName);
        }
    }

    public static void HandleItemDoubleClick<TItem>(object sender, System.Windows.Input.MouseButtonEventArgs e, Action<TItem> launchAction)
        where TItem : class, ILauncherItem
    {
        if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: TItem item })
        {
            launchAction(item);
            e.Handled = true;
        }
    }

    public static void HandleItemClick<TItem>(object sender, Action<TItem> launchAction)
        where TItem : class, ILauncherItem
    {
        if (sender is FrameworkElement { DataContext: TItem item })
        {
            launchAction(item);
        }
    }
}
