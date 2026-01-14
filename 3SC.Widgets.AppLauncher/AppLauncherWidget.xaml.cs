using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using _3SC.Widgets.AppLauncher.Helpers;

namespace _3SC.Widgets.AppLauncher;

public partial class AppLauncherWindow : WidgetWindowBase
{
    private readonly AppLauncherWidgetViewModel _viewModel;
    private bool _showFavoritesOnly = false;
    private const double DefaultMinWidgetWidth = 300;
    private const double DefaultMinWidgetHeight = 200;

    public AppLauncherWindow()
    {
        InitializeComponent();
        _viewModel = new AppLauncherWidgetViewModel();
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

        // Safe resource lookup with sensible fallbacks
        var primaryBrush = TryFindResource("Brushes.TextPrimary") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.White;
        var tertiaryBrush = TryFindResource("Brushes.TextTertiary") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Gray;

        AllTabButton.SetValue(System.Windows.Controls.Control.ForegroundProperty, _showFavoritesOnly ? tertiaryBrush : primaryBrush);
        FavoritesTabButton.SetValue(System.Windows.Controls.Control.ForegroundProperty, _showFavoritesOnly ? primaryBrush : tertiaryBrush);

        System.Windows.Media.TranslateTransform? transform = null;
        if (SelectionIndicator.RenderTransform is System.Windows.Media.TranslateTransform tt)
        {
            transform = tt;
        }
        else if (SelectionIndicator.RenderTransform is System.Windows.Media.TransformGroup tg)
        {
            foreach (var child in tg.Children)
            {
                if (child is System.Windows.Media.TranslateTransform childTt)
                {
                    transform = childTt;
                    break;
                }
            }
        }

        double tabWidth = AllTabButton.ActualWidth > 0 ? AllTabButton.ActualWidth : 32.0;
        double targetX = (_showFavoritesOnly ? 1 : 0) * tabWidth;

        if (transform != null)
        {
            var anim = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
            };

            transform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }
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

    protected override Task OnWidgetLoadedAsync()
    {
        UpdateAppDisplay();
        return Task.CompletedTask;
    }

    protected override async Task OnWidgetClosingAsync()
    {
        await Task.CompletedTask;
        _viewModel.Dispose();
    }

    protected override bool IsDragBlocked(DependencyObject? source)
    {
        return LauncherWidgetWindowShared.IsDragBlocked(source, ResizeTop, ResizeBottom, ResizeLeft, ResizeRight, AllTabButton, FavoritesTabButton, SelectionIndicator, AppScrollViewer, AppItemsControl);
    }

    protected override double MinWidgetWidth => DefaultMinWidgetWidth;
    protected override double MinWidgetHeight => DefaultMinWidgetHeight;
}

public sealed class AppItem : _3SC.Widgets.AppLauncher.Abstractions.ILauncherItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public System.Windows.Media.ImageSource? Icon { get; set; }
    public bool IsFavorite { get; set; }
}
