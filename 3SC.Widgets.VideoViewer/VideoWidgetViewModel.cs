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

namespace _3SC.Widgets.VideoViewer;

/// <summary>
/// Represents a video file item in the list.
/// </summary>
public partial class VideoFileItem : ObservableObject
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
/// ViewModel for the Video widget - displays videos from Videos/Downloads with playback controls.
/// </summary>
public partial class VideoWidgetViewModel : ObservableObject
{
    private static readonly string DataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC", "Widgets", "video-viewer", "recent_videos.json");

    private static readonly string[] SupportedExtensions = [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm", ".flv", ".m4v"];

    private readonly ILogger _logger = Log.ForContext<VideoWidgetViewModel>();

    #region Properties

    [ObservableProperty]
    private string _videoPath = string.Empty;

    [ObservableProperty]
    private Uri? _videoSource;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileInfo = string.Empty;

    [ObservableProperty]
    private bool _hasVideo;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double _volume = 0.5;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private TimeSpan _currentPosition;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private VideoFileItem? _selectedFile;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// All discovered video files.
    /// </summary>
    public ObservableCollection<VideoFileItem> AllVideoFiles { get; } = [];

    /// <summary>
    /// Filtered video files based on search.
    /// </summary>
    public ObservableCollection<VideoFileItem> VideoFiles { get; } = [];

    /// <summary>
    /// Recently viewed videos (pinned at top).
    /// </summary>
    public ObservableCollection<VideoFileItem> RecentFiles { get; } = [];

    public bool HasRecentFiles => RecentFiles.Count > 0;
    public bool HasVideoFiles => VideoFiles.Count > 0;

    public string PlayPauseIcon => IsPlaying ? "\uE769" : "\uE768";
    public string PlayPauseTooltip => IsPlaying ? "Pause" : "Play";

    public string VolumeIcon
    {
        get
        {
            if (IsMuted || Volume <= 0)
                return "\uE74F";
            if (Volume > 0.66)
                return "\uE995";
            if (Volume > 0.33)
                return "\uE994";
            return "\uE992";
        }
    }

    public string PositionText => $"{FormatTime(CurrentPosition)} / {FormatTime(Duration)}";

    /// <summary>
    /// Drop handler for drag and drop support.
    /// </summary>
    public Action<string[]>? DropHandler { get; }

    #endregion

    #region Events

    /// <summary>
    /// Events for view to handle media control.
    /// </summary>
    public event Action? PlayRequested;
    public event Action? PauseRequested;
    public event Action? StopRequested;
    public event Action<TimeSpan>? SeekRequested;

    #endregion

    public VideoWidgetViewModel()
    {
        DropHandler = OnFilesDropped;
    }

    #region Lifecycle Methods

    /// <summary>
    /// Initialize the widget - load recent files and discover videos.
    /// </summary>
    public void OnInitialize()
    {
        _logger.Debug("Initializing Video Viewer widget");
        _ = InitializeAsync();
    }

    /// <summary>
    /// Cleanup when widget is disposed.
    /// </summary>
    public void OnDispose()
    {
        _logger.Debug("Disposing Video Viewer widget");
        StopRequested?.Invoke();
        IsPlaying = false;
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            await LoadRecentFilesAsync();
            await DiscoverVideoFilesAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize Video widget");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRecentFilesAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(DataPath))
                    return;

                var json = File.ReadAllText(DataPath);
                var recentPaths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);

                if (recentPaths == null)
                    return;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var path in recentPaths.Where(File.Exists).Take(5))
                    {
                        var fileInfo = new FileInfo(path);
                        RecentFiles.Add(new VideoFileItem
                        {
                            FilePath = path,
                            FileName = Path.GetFileName(path),
                            FileSize = FormatFileSize(fileInfo.Length),
                            LastModified = fileInfo.LastWriteTime
                        });
                    }
                    OnPropertyChanged(nameof(HasRecentFiles));
                });
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to load recent videos");
            }
        });
    }

    private async Task DiscoverVideoFilesAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var videosFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                var downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var allFiles = new List<VideoFileItem>();

                // Get videos from Videos folder (limit to 50)
                if (Directory.Exists(videosFolder))
                {
                    allFiles.AddRange(GetVideoFilesFromFolder(videosFolder, 50));
                }

                // Get videos from Downloads (limit to 30)
                if (Directory.Exists(downloadsFolder))
                {
                    allFiles.AddRange(GetVideoFilesFromFolder(downloadsFolder, 30));
                }

                // Get videos from Desktop (limit to 20)
                if (Directory.Exists(desktopFolder))
                {
                    allFiles.AddRange(GetVideoFilesFromFolder(desktopFolder, 20));
                }

                // Sort by last modified, most recent first
                var sortedFiles = allFiles
                    .DistinctBy(f => f.FilePath, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(f => f.LastModified)
                    .ToList();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AllVideoFiles.Clear();
                    foreach (var file in sortedFiles)
                    {
                        AllVideoFiles.Add(file);
                    }

                    ApplyFilter();
                });
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to discover video files");
            }
        });
    }

    private List<VideoFileItem> GetVideoFilesFromFolder(string folderPath, int limit)
    {
        var files = new List<VideoFileItem>();

        try
        {
            var videoFiles = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Take(limit);

            foreach (var filePath in videoFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    files.Add(new VideoFileItem
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        FileSize = FormatFileSize(fileInfo.Length),
                        LastModified = fileInfo.LastWriteTime
                    });
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to read video file info: {Path}", filePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Failed to enumerate videos in folder: {Folder}", folderPath);
        }

        return files;
    }

    #endregion

    #region Property Changed Handlers

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedFileChanged(VideoFileItem? value)
    {
        if (value != null)
        {
            _ = LoadVideoAsync(value.FilePath);
        }
    }

    partial void OnIsPlayingChanged(bool value)
    {
        OnPropertyChanged(nameof(PlayPauseIcon));
        OnPropertyChanged(nameof(PlayPauseTooltip));
    }

    partial void OnVolumeChanged(double value)
    {
        OnPropertyChanged(nameof(VolumeIcon));
    }

    partial void OnIsMutedChanged(bool value)
    {
        OnPropertyChanged(nameof(VolumeIcon));
    }

    partial void OnCurrentPositionChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(PositionText));
        if (Duration.TotalSeconds > 0)
        {
            Progress = value.TotalSeconds / Duration.TotalSeconds * 100;
        }
    }

    partial void OnDurationChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(PositionText));
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void BrowseVideo()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.webm;*.flv;*.m4v|All Files|*.*",
            Title = "Select a video",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            _ = LoadVideoAsync(dialog.FileName);
        }
    }

    [RelayCommand]
    private void SelectFile(VideoFileItem? item)
    {
        if (item == null)
            return;

        // Deselect previous
        foreach (var file in AllVideoFiles)
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
    private void OpenInDefaultApp()
    {
        if (!HasVideo || string.IsNullOrEmpty(VideoPath))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = VideoPath,
                UseShellExecute = true
            });
            _logger.Debug("Opened video in default app: {Path}", VideoPath);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to open video in default app: {Path}", VideoPath);
        }
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        if (!HasVideo || string.IsNullOrEmpty(VideoPath) || !File.Exists(VideoPath))
            return;

        try
        {
            Process.Start("explorer.exe", $"/select,\"{VideoPath}\"");
            _logger.Debug("Opened video in Explorer: {Path}", VideoPath);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to open video in Explorer: {Path}", VideoPath);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            AllVideoFiles.Clear();
            VideoFiles.Clear();
            await DiscoverVideoFilesAsync();
            _logger.Debug("Refreshed video files list");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearVideo()
    {
        StopRequested?.Invoke();
        IsPlaying = false;
        HasVideo = false;
        VideoSource = null;
        VideoPath = string.Empty;
        FileName = string.Empty;
        FileInfo = string.Empty;
        CurrentPosition = TimeSpan.Zero;
        Duration = TimeSpan.Zero;
        Progress = 0;

        foreach (var file in AllVideoFiles)
        {
            file.IsSelected = false;
        }
        foreach (var file in RecentFiles)
        {
            file.IsSelected = false;
        }

        SelectedFile = null;
        _logger.Debug("Cleared video");
    }

    [RelayCommand]
    private void RemoveFromRecent(VideoFileItem? item)
    {
        if (item == null)
            return;

        RecentFiles.Remove(item);
        OnPropertyChanged(nameof(HasRecentFiles));
        SaveRecentFiles();
        _logger.Debug("Removed from recent: {Path}", item.FilePath);
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (!HasVideo)
            return;

        if (IsPlaying)
        {
            PauseRequested?.Invoke();
            IsPlaying = false;
        }
        else
        {
            PlayRequested?.Invoke();
            IsPlaying = true;
        }
    }

    [RelayCommand]
    private void Stop()
    {
        if (!HasVideo)
            return;

        StopRequested?.Invoke();
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
    }

    [RelayCommand]
    private void ToggleMute()
    {
        IsMuted = !IsMuted;
    }

    [RelayCommand]
    private void VolumeUp()
    {
        Volume = Math.Min(1.0, Volume + 0.1);
        IsMuted = false;
    }

    [RelayCommand]
    private void VolumeDown()
    {
        Volume = Math.Max(0.0, Volume - 0.1);
    }

    [RelayCommand]
    private void SkipForward()
    {
        if (!HasVideo || Duration == TimeSpan.Zero)
            return;

        var newPosition = CurrentPosition + TimeSpan.FromSeconds(10);
        if (newPosition > Duration)
            newPosition = Duration;

        SeekRequested?.Invoke(newPosition);
    }

    [RelayCommand]
    private void SkipBackward()
    {
        if (!HasVideo)
            return;

        var newPosition = CurrentPosition - TimeSpan.FromSeconds(10);
        if (newPosition < TimeSpan.Zero)
            newPosition = TimeSpan.Zero;

        SeekRequested?.Invoke(newPosition);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Seek to a specific percentage of the video.
    /// </summary>
    public void Seek(double percentage)
    {
        if (!HasVideo || Duration == TimeSpan.Zero)
            return;

        var seekTime = TimeSpan.FromSeconds(Duration.TotalSeconds * percentage / 100);
        SeekRequested?.Invoke(seekTime);
    }

    /// <summary>
    /// Update the current playback position (called by view).
    /// </summary>
    public void UpdatePosition(TimeSpan position)
    {
        CurrentPosition = position;
    }

    /// <summary>
    /// Set the video duration (called by view when media is loaded).
    /// </summary>
    public void SetDuration(TimeSpan duration)
    {
        Duration = duration;

        if (!string.IsNullOrEmpty(VideoPath) && File.Exists(VideoPath))
        {
            var fileInfo = new FileInfo(VideoPath);
            var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
            FileInfo = $"{FormatTime(duration)} • {sizeMb:F1} MB";
        }
    }

    /// <summary>
    /// Notify that playback has ended (called by view).
    /// </summary>
    public void NotifyPlaybackEnded()
    {
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
    }

    #endregion

    #region Private Methods

    private void ApplyFilter()
    {
        VideoFiles.Clear();

        var query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

        var filtered = string.IsNullOrEmpty(query)
            ? AllVideoFiles
            : AllVideoFiles.Where(f => f.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var file in filtered)
        {
            VideoFiles.Add(file);
        }

        OnPropertyChanged(nameof(HasVideoFiles));
    }

    private async Task LoadVideoAsync(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            _logger.Warning("Video file not found: {Path}", path);
            return;
        }

        IsLoading = true;

        try
        {
            // Stop current playback
            StopRequested?.Invoke();
            IsPlaying = false;

            await Task.Run(() =>
            {
                var fileInfo = new FileInfo(path);
                var sizeMb = fileInfo.Length / (1024.0 * 1024.0);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    VideoPath = path;
                    VideoSource = new Uri(path, UriKind.Absolute);
                    HasVideo = true;
                    FileName = Path.GetFileName(path);
                    FileInfo = $"{sizeMb:F1} MB";
                    CurrentPosition = TimeSpan.Zero;
                    Duration = TimeSpan.Zero;
                    Progress = 0;
                });
            });

            AddToRecentFiles(path);
            _logger.Debug("Loaded video: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load video: {Path}", path);
            HasVideo = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnFilesDropped(string[] files)
    {
        var videoFile = files.FirstOrDefault(f =>
            SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        if (!string.IsNullOrEmpty(videoFile))
        {
            _ = LoadVideoAsync(videoFile);
        }
    }

    private void AddToRecentFiles(string path)
    {
        // Check if already in recent files
        var existing = RecentFiles.FirstOrDefault(f =>
            f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            // Move to top
            RecentFiles.Remove(existing);
            RecentFiles.Insert(0, existing);
        }
        else
        {
            // Add new entry
            try
            {
                var fileInfo = new FileInfo(path);
                var newItem = new VideoFileItem
                {
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    FileSize = FormatFileSize(fileInfo.Length),
                    LastModified = fileInfo.LastWriteTime
                };

                RecentFiles.Insert(0, newItem);

                // Keep only 5 recent files
                while (RecentFiles.Count > 5)
                {
                    RecentFiles.RemoveAt(RecentFiles.Count - 1);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to add to recent files: {Path}", path);
            }
        }

        OnPropertyChanged(nameof(HasRecentFiles));
        SaveRecentFiles();
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

            var recentPaths = RecentFiles.Select(f => f.FilePath).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(recentPaths);
            File.WriteAllText(DataPath, json);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to save recent videos");
        }
    }

    private static string FormatFileSize(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        return bytes switch
        {
            >= GB => $"{bytes / (double)GB:F1} GB",
            >= MB => $"{bytes / (double)MB:F1} MB",
            >= KB => $"{bytes / (double)KB:F1} KB",
            _ => $"{bytes} B"
        };
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.Hours > 0
            ? $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}"
            : $"{time.Minutes}:{time.Seconds:D2}";
    }

    #endregion
}
