using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using _3SC.Widgets.GameVault.Helpers;

namespace _3SC.Widgets.GameVault;

public partial class GameVaultWindow : Window
{
    private readonly GameVaultWidgetViewModel _viewModel;
    private bool _showFavoritesOnly = false;
    private const double DefaultMinWidgetWidth = 300;
    private const double DefaultMinWidgetHeight = 200;

    public GameVaultWindow()
    {
        InitializeComponent();
        _viewModel = new GameVaultWidgetViewModel();
        DataContext = _viewModel;
    }

    private void UpdateAppDisplay()
    {
        if (EmptyState == null || AppScrollViewer == null || AppItemsControl == null)
        {
            return;
        }

        LauncherWidgetWindowShared.UpdateItemsDisplay(_showFavoritesOnly, _viewModel.Apps, a => a.IsFavorite, EmptyState, AppScrollViewer, AppItemsControl, UpdateTabVisuals);
    }

    private void UpdateTabVisuals()
    {
        if (SelectionIndicator == null || AllTabButton == null || FavoritesTabButton == null)
        {
            return;
        }

        LauncherWidgetHelpers.UpdateTabVisuals(_showFavoritesOnly, SelectionIndicator, AllTabButton, FavoritesTabButton, resourceKey => (System.Windows.Media.Brush)FindResource(resourceKey));
    }

    private void AllTab_Click(object sender, RoutedEventArgs e)
    {
        _showFavoritesOnly = false;
        UpdateAppDisplay();
    }

    private void FavoritesTab_Click(object sender, RoutedEventArgs e)
    {
        _showFavoritesOnly = true;
        UpdateAppDisplay();
    }

    private void Widget_Drop(object sender, System.Windows.DragEventArgs e)
    {
        LauncherWidgetWindowShared.HandleFileDrop(e, AddAppFromPath);
    }

    [SuppressMessage("Usage", "S2325:Make 'Widget_DragOver' a static method.", Justification = "WPF event handlers must be instance methods for XAML wiring.")]
    private void Widget_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        LauncherWidgetWindowShared.HandleDragOver(e);
    }

    private void AddApp_Click(object sender, RoutedEventArgs e)
    {
        LauncherWidgetWindowShared.ShowAddDialog(AddAppFromPath);
    }

    private void AddAppIcon_Click(object sender, MouseButtonEventArgs e)
    {
        AddApp_Click(sender, e);
        e.Handled = true;
    }

    private void AddAppFromPath(string path) => LauncherWidgetWindowShared.AddFromPath(path, _viewModel.Apps, UpdateAppDisplay, _viewModel.SaveApps);

    [SuppressMessage("Usage", "S2325:Make 'AppItem_Click' a static method.", Justification = "WPF event handlers must be instance methods for XAML wiring.")]
    private void AppItem_Click(object sender, MouseButtonEventArgs e)
    {
        LauncherWidgetWindowShared.HandleItemDoubleClick<AppItem>(sender, e, LaunchApp);
    }

    private void LaunchApp_Click(object sender, RoutedEventArgs e)
    {
        LauncherWidgetWindowShared.HandleItemClick<AppItem>(sender, LaunchApp);
    }

    private static void LaunchApp(AppItem app)
    {
        LauncherWidgetWindowShared.LaunchItem(app);
    }

    private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
    {
        LauncherWidgetWindowShared.ToggleFavorite<AppItem>(sender, UpdateAppDisplay, _viewModel.SaveApps);
    }

    private void RemoveApp_Click(object sender, RoutedEventArgs e)
    {
        LauncherWidgetWindowShared.RemoveItem(sender, _viewModel.Apps, UpdateAppDisplay, _viewModel.SaveApps);
    }

    protected virtual Task OnWindowLoadedAsync()
    {
        UpdateAppDisplay();
        return Task.CompletedTask;
    }

    protected virtual async Task OnWindowClosingAsync()
    {
        await Task.CompletedTask;
        _viewModel.Dispose();
    }

    protected virtual bool IsDragBlocked(DependencyObject? source)
    {
        return LauncherWidgetWindowShared.IsDragBlocked(source, ResizeTop, ResizeBottom, ResizeLeft, ResizeRight, AllTabButton, FavoritesTabButton, SelectionIndicator, AppScrollViewer, AppItemsControl);
    }

    protected double MinWindowWidth => DefaultMinWidgetWidth;
    protected double MinWindowHeight => DefaultMinWidgetHeight;
}

public sealed class AppItem : _3SC.Widgets.GameVault.Abstractions.ILauncherItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public System.Windows.Media.ImageSource? Icon { get; set; }
    public bool IsFavorite { get; set; }
}
