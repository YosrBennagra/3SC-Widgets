namespace _3SC.Widgets.AppLauncher.Abstractions
{
    public interface ILauncherItem
    {
        string Name { get; set; }
        string Path { get; set; }
        System.Windows.Media.ImageSource? Icon { get; set; }
        bool IsFavorite { get; set; }
    }
}
