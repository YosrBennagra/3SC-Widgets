using System.Windows.Media.Imaging;

namespace _3SC.Widgets.ClipboardHistory;

public class ClipboardItem
{
    public string? Text { get; set; }
    public BitmapSource? Image { get; set; }
    public ClipboardItemType Type { get; set; }
    public string DisplayText => Text != null && Text.Length > 100 ? Text.Substring(0, 100) + "..." : Text ?? "";
    public bool IsImage => Type == ClipboardItemType.Image;
    public bool IsText => Type == ClipboardItemType.Text;
}

public enum ClipboardItemType
{
    Text,
    Image
}
