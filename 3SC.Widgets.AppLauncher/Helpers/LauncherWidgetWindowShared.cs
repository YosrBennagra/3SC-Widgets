using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _3SC.Widgets.AppLauncher.Helpers
{
    public static class LauncherWidgetWindowShared
    {
        public static void UpdateItemsDisplay<T>(bool showFavoritesOnly, ObservableCollection<T> apps, Func<T, bool> isFavorite, FrameworkElement emptyState, ScrollViewer scrollViewer, ItemsControl itemsControl, Action updateTabVisuals)
            where T : class
        {
            if (apps == null) return;

            int count = showFavoritesOnly ? apps.Count(isFavorite) : apps.Count;
            if (emptyState != null && scrollViewer != null)
            {
                emptyState.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
                scrollViewer.Visibility = count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            updateTabVisuals?.Invoke();
        }

        public static void HandleFileDrop(DragEventArgs e, Action<string> addPath)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var f in files)
                {
                    try { addPath?.Invoke(f); } catch { }
                }
            }
        }

        public static void HandleDragOver(DragEventArgs e)
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

        public static void ShowAddDialog(Action<string> addPath)
        {
            using var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                addPath?.Invoke(dlg.FileName);
            }
        }

        public static void AddFromPath<T>(string path, ObservableCollection<T> apps, Action updateDisplay, Action save)
            where T : class
        {
            try
            {
                if (!System.IO.File.Exists(path)) return;
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                // Try to create a T if possible via reflection; otherwise expect caller to handle adding
                var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    var instance = ctor.Invoke(null) as T;
                    var propName = typeof(T).GetProperty("Name");
                    var propPath = typeof(T).GetProperty("Path");
                    propName?.SetValue(instance, name);
                    propPath?.SetValue(instance, path);
                    apps.Add(instance);
                }
                save?.Invoke();
                updateDisplay?.Invoke();
            }
            catch { }
        }

        public static void HandleItemDoubleClick<T>(object sender, MouseButtonEventArgs e, Action<T> action)
            where T : class
        {
            if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is T item)
            {
                action?.Invoke(item);
            }
        }

        public static void HandleItemClick<T>(object sender, Action<T> action)
            where T : class
        {
            if (sender is FrameworkElement fe && fe.DataContext is T item)
            {
                action?.Invoke(item);
            }
        }

        public static void LaunchItem<T>(T item) where T : class
        {
            if (item == null) return;
            var propPath = typeof(T).GetProperty("Path");
            var path = propPath?.GetValue(item) as string;
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch { }
        }

        public static void ToggleFavorite<T>(object sender, Action updateDisplay, Action save) where T : class
        {
            if (sender is FrameworkElement fe && fe.DataContext is T item)
            {
                var prop = typeof(T).GetProperty("IsFavorite");
                if (prop != null)
                {
                    var val = prop.GetValue(item) as bool?;
                    prop.SetValue(item, !(val ?? false));
                }
                save?.Invoke();
                updateDisplay?.Invoke();
            }
        }

        public static void RemoveItem<T>(object sender, ObservableCollection<T> apps, Action updateDisplay, Action save) where T : class
        {
            if (sender is FrameworkElement fe && fe.DataContext is T item)
            {
                apps.Remove(item);
                save?.Invoke();
                updateDisplay?.Invoke();
            }
        }

        public static bool IsDragBlocked(DependencyObject? source, params UIElement[] controls)
        {
            // Basic conservative implementation: do not block drag by default.
            return false;
        }
    }
}
