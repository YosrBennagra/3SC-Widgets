using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.ImageViewer;

/// <summary>
/// Represents an image file item in the list.
/// </summary>
public partial class ImageFileItem : ObservableObject
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileSize = string.Empty;

    [ObservableProperty]
    private DateTime _lastModified;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private BitmapImage? _thumbnail;

    public string LastModifiedText => LastModified.ToString("MMM d, yyyy", CultureInfo.CurrentCulture);
}

/// <summary>
/// ViewModel for the Image widget - displays images from Pictures/Downloads with gallery capability.
/// </summary>
public partial class ImageWidgetViewModel : ObservableObject, IDisposable
{
    private bool _disposed;
    private static readonly string DataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC", "image_widget.json");

    private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

    private readonly ILogger _logger = Log.ForContext<ImageWidgetViewModel>();

    #region Properties

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private BitmapImage? _currentImage;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileInfo = string.Empty;

    [ObservableProperty]
    private string _dimensions = string.Empty;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double _zoomLevel = 1.0;

    [ObservableProperty]
    private ImageFileItem? _selectedFile;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// All discovered image files.
    /// </summary>
    public ObservableCollection<ImageFileItem> AllImageFiles { get; } = [];

    /// <summary>
    /// Filtered image files based on search.
    /// </summary>
    public ObservableCollection<ImageFileItem> ImageFiles { get; } = [];

    /// <summary>
    /// Recently viewed images (pinned at top).
    /// </summary>
    public ObservableCollection<ImageFileItem> RecentFiles { get; } = [];

    public bool HasFiles => ImageFiles.Count > 0 || RecentFiles.Count > 0;
    public bool HasRecentFiles => RecentFiles.Count > 0;

    public string ZoomText => $"{ZoomLevel:P0}";

    /// <summary>
    /// Drop handler delegate for use with DropFileBehavior.
    /// </summary>
    public Action<string[]> DropHandler => HandleFileDrop;

    #endregion

    public ImageWidgetViewModel()
    {
        _ = DiscoverImageFilesAsync();
        LoadRecentFiles();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedFileChanged(ImageFileItem? value)
    {
        if (value != null)
        {
            LoadImage(value.FilePath);
            AddToRecent(value);
        }
    }

    partial void OnZoomLevelChanged(double value)
    {
        OnPropertyChanged(nameof(ZoomText));
    }

    #region Commands

    [RelayCommand]
    private void SelectFile(ImageFileItem? item)
    {
        if (item == null)
        {
            return;
        }

        // Deselect previous
        foreach (var file in AllImageFiles)
        {
            file.IsSelected = false;
        }

        foreach (var file in RecentFiles)
        {
            file.IsSelected = false;
        }

        item.IsSelected = true;
        SelectedFile = item;
    }

    [RelayCommand]
    private void BrowseImage()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|All Files|*.*",
            Title = "Select an image",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            var item = CreateImageFileItem(dialog.FileName);
            if (item != null)
            {
                SelectFile(item);
            }
        }
    }

    [RelayCommand]
    private void OpenInDefaultApp()
    {
        if (HasImage && !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ImagePath,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        if (HasImage && !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            Process.Start("explorer.exe", $"/select,\"{ImagePath}\"");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await DiscoverImageFilesAsync();
    }

    [RelayCommand]
    private void ClearPreview()
    {
        HasImage = false;
        CurrentImage = null;
        ImagePath = string.Empty;
        FileName = string.Empty;
        FileInfo = string.Empty;
        Dimensions = string.Empty;
        SelectedFile = null;
        ZoomLevel = 1.0;

        foreach (var file in AllImageFiles)
        {
            file.IsSelected = false;
        }

        foreach (var file in RecentFiles)
        {
            file.IsSelected = false;
        }
    }

    [RelayCommand]
    private void RemoveFromRecent(ImageFileItem? item)
    {
        if (item == null)
        {
            return;
        }

        RecentFiles.Remove(item);
        OnPropertyChanged(nameof(HasRecentFiles));
        SaveRecentFiles();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel * 1.25, 5.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel / 1.25, 0.25);
    }

    [RelayCommand]
    private void ResetZoom()
    {
        ZoomLevel = 1.0;
    }

    [RelayCommand]
    private void SetAsWallpaper()
    {
        if (!HasImage || string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath))
        {
            return;
        }

        try
        {
            // Use Windows API to set wallpaper
            _ = NativeMethods.SystemParametersInfo(
                NativeMethods.SPI_SETDESKWALLPAPER,
                0,
                ImagePath,
                NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set wallpaper");
        }
    }

    #endregion

    #region Drag and Drop

    /// <summary>
    /// Handles files dropped onto the widget.
    /// </summary>
    public void HandleFileDrop(string[] files)
    {
        var imageFile = files.FirstOrDefault(f =>
            SupportedExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) && File.Exists(f));

        if (imageFile != null)
        {
            var item = CreateImageFileItem(imageFile);
            if (item != null)
            {
                SelectFile(item);
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task DiscoverImageFilesAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                var imageFiles = new List<ImageFileItem>();

                ScanFolderForImages(imageFiles, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 50);
                ScanFolderForImages(imageFiles, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), 30);
                ScanFolderForImages(imageFiles, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 20);

                UpdateImageFilesOnUiThread(imageFiles);
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to discover image files");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ScanFolderForImages(List<ImageFileItem> imageFiles, string folderPath, int limit)
    {
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        try
        {
            var files = GetImageFiles(folderPath, limit);
            imageFiles.AddRange(files);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to scan folder: {Folder}", folderPath);
        }
    }

    private void UpdateImageFilesOnUiThread(List<ImageFileItem> imageFiles)
    {
        var sortedFiles = imageFiles
            .DistinctBy(f => f.FilePath.ToLowerInvariant())
            .OrderByDescending(f => f.LastModified)
            .ToList();

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            AllImageFiles.Clear();
            foreach (var file in sortedFiles)
            {
                AllImageFiles.Add(file);
            }

            ApplyFilter();
            OnPropertyChanged(nameof(HasFiles));
        });
    }

    private List<ImageFileItem> GetImageFiles(string folderPath, int limit)
    {
        var result = new List<ImageFileItem>();

        try
        {
            var files = Directory.GetFiles(folderPath)
                .Where(f => SupportedExtensions.Any(ext =>
                    f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .Take(limit);

            foreach (var file in files)
            {
                var item = CreateImageFileItem(file);
                if (item != null)
                {
                    result.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to get image files from {Folder}", folderPath);
        }

        return result;
    }

    private void ApplyFilter()
    {
        ImageFiles.Clear();

        var query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

        var filtered = string.IsNullOrEmpty(query)
            ? AllImageFiles
            : AllImageFiles.Where(f =>
                f.FileName.Contains(query, StringComparison.CurrentCultureIgnoreCase));

        foreach (var file in filtered)
        {
            ImageFiles.Add(file);
        }

        OnPropertyChanged(nameof(HasFiles));
    }

    private static ImageFileItem? CreateImageFileItem(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                return null;
            }

            var sizeKb = fileInfo.Length / 1024.0;
            var sizeText = sizeKb > 1024 ? $"{sizeKb / 1024:F1} MB" : $"{sizeKb:F0} KB";

            return new ImageFileItem
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                FileSize = sizeText,
                LastModified = fileInfo.LastWriteTime
            };
        }
        catch
        {
            return null;
        }
    }

    private void LoadImage(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            HasImage = false;
            CurrentImage = null;
            ImagePath = string.Empty;
            FileName = string.Empty;
            FileInfo = string.Empty;
            Dimensions = string.Empty;
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            CurrentImage = bitmap;
            ImagePath = path;
            HasImage = true;
            FileName = Path.GetFileName(path);
            Dimensions = $"{bitmap.PixelWidth} × {bitmap.PixelHeight}";

            var fileInfo = new FileInfo(path);
            var sizeKb = fileInfo.Length / 1024.0;
            FileInfo = sizeKb > 1024 ? $"{sizeKb / 1024:F1} MB" : $"{sizeKb:F0} KB";

            ZoomLevel = 1.0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load image: {Path}", path);
            HasImage = false;
        }
    }

    private void AddToRecent(ImageFileItem item)
    {
        // Remove if already exists
        var existing = RecentFiles.FirstOrDefault(f =>
            f.FilePath.Equals(item.FilePath, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            RecentFiles.Remove(existing);
        }

        // Add to beginning
        var newItem = new ImageFileItem
        {
            FilePath = item.FilePath,
            FileName = item.FileName,
            FileSize = item.FileSize,
            LastModified = DateTime.Now
        };

        RecentFiles.Insert(0, newItem);

        // Keep only last 5
        while (RecentFiles.Count > 5)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }

        OnPropertyChanged(nameof(HasRecentFiles));
        SaveRecentFiles();
    }

    private void LoadRecentFiles()
    {
        try
        {
            if (!File.Exists(DataPath))
            {
                return;
            }

            var json = File.ReadAllText(DataPath);
            var paths = System.Text.Json.JsonSerializer.Deserialize<string[]>(json);

            if (paths == null)
            {
                return;
            }

            foreach (var path in paths.Where(File.Exists).Take(5))
            {
                var item = CreateImageFileItem(path);
                if (item != null)
                {
                    RecentFiles.Add(item);
                }
            }

            OnPropertyChanged(nameof(HasRecentFiles));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to load recent images");
        }
    }

    private void SaveRecentFiles()
    {
        try
        {
            var dir = Path.GetDirectoryName(DataPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var paths = RecentFiles.Select(f => f.FilePath).ToArray();
            var json = System.Text.Json.JsonSerializer.Serialize(paths);
            File.WriteAllText(DataPath, json);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to save recent images");
        }
    }

    #endregion

    #region Lifecycle Methods

    public void OnInitialize()
    {
        _logger.Information("ImageWidgetViewModel initializing");
        _ = DiscoverImageFilesAsync();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.Information("Disposing ImageWidgetViewModel");
        SaveRecentFiles();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Native Methods

    private static class NativeMethods
    {
        public const int SPI_SETDESKWALLPAPER = 0x0014;
        public const int SPIF_UPDATEINIFILE = 0x01;
        public const int SPIF_SENDCHANGE = 0x02;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }

    #endregion
}
