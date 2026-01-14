using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3SC.Widgets.GameVault.Helpers;

public static class LauncherWidgetHelpers
{
    public static string? ResolveShortcut(string shortcutPath)
    {
        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return null;
            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return null;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            var targetPath = shortcut.TargetPath;
            return string.IsNullOrEmpty(targetPath) ? null : targetPath;
        }
        catch
        {
            return null;
        }
    }

    public static BitmapSource? TryLoadIcon(string path)
    {
        try
        {
            if (path.EndsWith(".url", StringComparison.OrdinalIgnoreCase)) return null;
            if (!File.Exists(path)) return null;
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
            if (icon == null) return null;
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load icon for '{path}': {ex.Message}");
            return null;
        }
    }

    public static void UpdateTabVisuals(
        bool showSecondTab,
        UIElement selectionIndicator,
        UIElement firstTab,
        UIElement secondTab,
        Func<string, System.Windows.Media.Brush> resourceLookup)
    {
        if (showSecondTab)
        {
            Grid.SetColumn(selectionIndicator, 1);
            firstTab.SetValue(Control.ForegroundProperty, resourceLookup("Brushes.TextTertiary"));
            secondTab.SetValue(Control.ForegroundProperty, resourceLookup("Brushes.TextPrimary"));
        }
        else
        {
            Grid.SetColumn(selectionIndicator, 0);
            firstTab.SetValue(Control.ForegroundProperty, resourceLookup("Brushes.TextPrimary"));
            secondTab.SetValue(Control.ForegroundProperty, resourceLookup("Brushes.TextTertiary"));
        }
    }
}
