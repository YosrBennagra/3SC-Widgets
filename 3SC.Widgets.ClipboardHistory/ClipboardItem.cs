using System.Windows.Media.Imaging;

namespace _3SC.Widgets.ClipboardHistory;

public class ClipboardItem
{
    public string? Text { get; set; }
    public BitmapSource? Image { get; set; }
    public ClipboardItemType Type { get; set; }

    public string DisplayText
    {
        get
        {
            if (Type == ClipboardItemType.Image)
            {
                if (Image != null)
                {
                    return $"Screenshot ({Image.PixelWidth}x{Image.PixelHeight})";
                }
                return "Image";
            }

            return Text != null && Text.Length > 80 ? Text.Substring(0, 80) + "..." : Text ?? "";
        }
    }

    public bool IsImage => Type == ClipboardItemType.Image;
    public bool IsText => Type == ClipboardItemType.Text;
}

public enum ClipboardItemType
{
    Text,
    Image
}
