using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using _3SC.Widgets.Folders.Helpers;

namespace _3SC.Widgets.Folders
{
    public partial class FoldersWindow : WidgetWindowBase
    {
        private readonly FolderHubWidgetViewModel _viewModel;
        private bool _showFavoritesOnly = false;

        private const double DefaultMinWidgetWidth = 300;
        private const double DefaultMinWidgetHeight = 200;

        public FoldersWindow(Guid widgetInstanceId, double left, double top, double width, double height, bool isLocked)
        {
            InitializeComponent();

            InitializeWidgetWindow(
                new WidgetWindowInit(widgetInstanceId, left, top, width, height, isLocked),
                new WidgetWindowParts(
                    LockWidgetMenuItem: LockWidgetMenuItem,
                    ResizeToggleMenuItem: ResizeToggleMenuItem,
                    ResizeOutlineElement: ResizeOutline,
                    ResizeTopThumb: ResizeTop,
                    ResizeBottomThumb: ResizeBottom,
                    ResizeLeftThumb: ResizeLeft,
                    ResizeRightThumb: ResizeRight,
                    WidgetKey: "folder-hub"));

            _viewModel = new FolderHubWidgetViewModel();
            DataContext = _viewModel;
        }

        private void UpdateFolderDisplay()
        {
            if (EmptyState == null || FolderScrollViewer == null || FolderItemsControl == null)
            {
                return;
            }

            var filteredFolders = _showFavoritesOnly
                ? _viewModel.Folders.Where(f => f.IsFavorite).ToList()
                : _viewModel.Folders.ToList();

            EmptyState.Visibility = filteredFolders.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            FolderScrollViewer.Visibility = filteredFolders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FolderItemsControl.ItemsSource = filteredFolders;

            UpdateTabVisuals();
        }

        private void UpdateTabVisuals()
        {
            if (SelectionIndicator == null || AllTabButton == null || FavoritesTabButton == null || SelectionIndicatorTransform == null)
            {
                return;
            }

            // Update foregrounds with local resource lookup + fallbacks
            Brush primaryBrush = TryFindResource("Brushes.TextPrimary") as Brush ?? Brushes.White;
            Brush tertiaryBrush = TryFindResource("Brushes.TextTertiary") as Brush ?? Brushes.Gray;

            AllTabButton.SetValue(Control.ForegroundProperty, _showFavoritesOnly ? tertiaryBrush : primaryBrush);
            FavoritesTabButton.SetValue(Control.ForegroundProperty, _showFavoritesOnly ? primaryBrush : tertiaryBrush);

            // Animate the selection indicator between columns
            var targetIndex = _showFavoritesOnly ? 1 : 0;

            // Calculate target X based on tab button widths (assumes consistent button widths)
            double tabWidth = AllTabButton.ActualWidth > 0 ? AllTabButton.ActualWidth : 32.0;
            double targetX = targetIndex * tabWidth;

            var anim = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
            };

            SelectionIndicatorTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }

        private void AllTab_Click(object sender, RoutedEventArgs e)
        {
            _showFavoritesOnly = false;
            UpdateFolderDisplay();
        }

        private void FavoritesTab_Click(object sender, RoutedEventArgs e)
        {
            _showFavoritesOnly = true;
            UpdateFolderDisplay();
        }

        private void Widget_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var items = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var item in items.Where(Directory.Exists))
                {
                    AddFolderFromPath(item);
                }
            }

            e.Handled = true;
        }

        [SuppressMessage("Usage", "S2325:Make 'Widget_DragOver' a static method.", Justification = "WPF event handlers must be instance methods for XAML wiring.")]
        private void Widget_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder to add",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AddFolderFromPath(dialog.SelectedPath);
            }
        }

        private void AddFolderIcon_Click(object sender, MouseButtonEventArgs e)
        {
            AddFolder_Click(sender, e);
            e.Handled = true;
        }

        private void AddFolderFromPath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                var displayName = Path.GetFileName(path);
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = path;
                }

                var folder = new FolderItem
                {
                    Name = displayName,
                    Path = path,
                    IsFavorite = false
                };

                _viewModel.Folders.Add(folder);
                UpdateFolderDisplay();
                _viewModel.SaveFolders();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to add folder: {ex.Message}");
            }
        }

        private void FolderItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: FolderItem folder })
            {
                OpenFolder(folder);
                e.Handled = true;
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: FolderItem folder })
            {
                OpenFolder(folder);
            }
        }

        private static void OpenFolder(FolderItem folder)
        {
            try
            {
                if (Directory.Exists(folder.Path))
                {
                    var explorer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = explorer,
                        Arguments = $"\"{folder.Path}\"",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open folder: {ex.Message}");
            }
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: FolderItem folder })
            {
                folder.IsFavorite = !folder.IsFavorite;
                UpdateFolderDisplay();
                _viewModel.SaveFolders();
            }
        }

        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: FolderItem folder })
            {
                _viewModel.Folders.Remove(folder);
                UpdateFolderDisplay();
                _viewModel.SaveFolders();
            }
        }

        protected override Task OnWidgetLoadedAsync()
        {
            UpdateFolderDisplay();
            return Task.CompletedTask;
        }

        protected override async Task OnWidgetClosingAsync()
        {
            await base.OnWidgetClosingAsync();
            _viewModel.Dispose();
        }

        protected override bool IsDragBlocked(DependencyObject? source)
        {
            while (source != null)
            {
                if (source is Thumb thumb &&
                    (thumb == ResizeTop || thumb == ResizeBottom || thumb == ResizeLeft || thumb == ResizeRight))
                {
                    return true;
                }

                if (source == AllTabButton || source == FavoritesTabButton || source == SelectionIndicator)
                {
                    return true;
                }

                if (source == FolderScrollViewer || source == FolderItemsControl)
                {
                    return true;
                }

                if (source is FrameworkElement { DataContext: FolderItem })
                {
                    return true;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        protected override double MinWidgetWidth => DefaultMinWidgetWidth;

        protected override double MinWidgetHeight => DefaultMinWidgetHeight;

        protected override bool SaveOnResize => true;
    }

    public sealed class FolderItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}

