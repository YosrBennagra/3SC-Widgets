using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace _3SC.Widgets.Folders.Helpers
{
    public static class LauncherWidgetHelpers
    {
        public static void UpdateTabVisuals(
            bool showSecondTab,
            UIElement selectionIndicator,
            UIElement firstTab,
            UIElement secondTab,
            Func<string, Brush> resourceLookup)
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
}
