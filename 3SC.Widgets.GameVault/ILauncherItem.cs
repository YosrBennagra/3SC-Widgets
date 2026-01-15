using System.Windows.Media;

namespace _3SC.Widgets.GameVault.Abstractions;

public interface ILauncherItem
{
    string Name { get; set; }
    string Path { get; set; }
    ImageSource? Icon { get; set; }
    bool IsFavorite { get; set; }
}
