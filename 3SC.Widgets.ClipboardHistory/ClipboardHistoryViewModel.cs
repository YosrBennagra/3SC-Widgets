using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace _3SC.Widgets.ClipboardHistory;

public partial class ClipboardHistoryViewModel : ObservableObject
{
    private readonly System.Windows.Threading.DispatcherTimer _monitorTimer;

    [ObservableProperty]
    private ObservableCollection<ClipboardItem> _clipboardItems = new();

    [ObservableProperty]
    private bool _isMonitoring = true;

    private string _lastClipboardText = string.Empty;
    private BitmapSource? _lastClipboardImage;

    public ClipboardHistoryViewModel()
    {
        ClipboardItems.Add(new ClipboardItem
        {
            Text = "(Clipboard monitoring active)",
            Type = ClipboardItemType.Text
        });

        _monitorTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _monitorTimer.Tick += MonitorClipboard;
        _monitorTimer.Start();
    }

    private void MonitorClipboard(object? sender, EventArgs e)
    {
        try
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                var image = System.Windows.Clipboard.GetImage();
                if (image != null && !IsSameImage(image, _lastClipboardImage))
                {
                    _lastClipboardImage = image;
                    _lastClipboardText = string.Empty;

                    RemovePlaceholder();

                    ClipboardItems.Insert(0, new ClipboardItem
                    {
                        Image = image,
                        Type = ClipboardItemType.Image
                    });

                    LimitItems();
                }
            }
            else if (System.Windows.Clipboard.ContainsText())
            {
                var text = System.Windows.Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(text) && text != _lastClipboardText)
                {
                    _lastClipboardText = text;
                    _lastClipboardImage = null;

                    RemovePlaceholder();

                    ClipboardItems.Insert(0, new ClipboardItem
                    {
                        Text = text,
                        Type = ClipboardItemType.Text
                    });

                    LimitItems();
                }
            }
        }
        catch
        {
            // Ignore clipboard access errors
        }
    }

    private void RemovePlaceholder()
    {
        if (ClipboardItems.Count == 1 && ClipboardItems[0].Text?.Contains("monitoring") == true)
        {
            ClipboardItems.Clear();
        }
    }

    private void LimitItems()
    {
        while (ClipboardItems.Count > 20)
        {
            ClipboardItems.RemoveAt(ClipboardItems.Count - 1);
        }
    }

    private static bool IsSameImage(BitmapSource? img1, BitmapSource? img2)
    {
        if (img1 == null || img2 == null)
            return false;
        return img1.PixelWidth == img2.PixelWidth &&
               img1.PixelHeight == img2.PixelHeight;
    }

    // Remove an item from the clipboard history
    public void RemoveItem(ClipboardItem item)
    {
        if (item == null) return;
        if (ClipboardItems.Contains(item))
        {
            ClipboardItems.Remove(item);
        }
    }

    // Clear all clipboard history
    public void ClearAll()
    {
        ClipboardItems.Clear();
        _lastClipboardText = string.Empty;
        _lastClipboardImage = null;
    }

    // Dispose and clean up resources
    public void Dispose()
    {
        _monitorTimer?.Stop();
        ClipboardItems.Clear();
    }
}
