using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.PdfViewer;

/// <summary>
/// Represents a PDF file item in the list.
/// </summary>
public partial class PdfFileItem : ObservableObject
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

    public string LastModifiedText => LastModified.ToString("MMM d, yyyy", System.Globalization.CultureInfo.CurrentCulture);
}

/// <summary>
/// ViewModel for the PDF widget - displays PDF files from Downloads/Recent with preview capability.
/// </summary>
public partial class PdfWidgetViewModel : ObservableObject
{
    private static readonly string DataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC", "pdf_widget.json");

    private readonly ILogger _logger = Log.ForContext<PdfWidgetViewModel>();

    #region Properties

    [ObservableProperty]
    private string _pdfPath = string.Empty;

    [ObservableProperty]
    private string _pdfFileName = string.Empty;

    [ObservableProperty]
    private string _fileInfo = string.Empty;

    [ObservableProperty]
    private bool _hasPdf;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoadingPdf;

    [ObservableProperty]
    private bool _showFileList = true;

    [ObservableProperty]
    private PdfFileItem? _selectedFile;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// All discovered PDF files.
    /// </summary>
    public ObservableCollection<PdfFileItem> AllPdfFiles { get; } = [];

    /// <summary>
    /// Filtered PDF files based on search.
    /// </summary>
    public ObservableCollection<PdfFileItem> PdfFiles { get; } = [];

    /// <summary>
    /// Recently opened PDF files (pinned at top).
    /// </summary>
    public ObservableCollection<PdfFileItem> RecentFiles { get; } = [];

    public bool HasFiles => PdfFiles.Count > 0 || RecentFiles.Count > 0;
    public bool HasRecentFiles => RecentFiles.Count > 0;

    /// <summary>
    /// Drop handler delegate for use with DropFileBehavior.
    /// </summary>
    public Action<string[]> DropHandler => HandleFileDrop;

    #endregion

    public PdfWidgetViewModel()
    {
        _ = DiscoverPdfFilesAsync();
        LoadRecentFiles();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedFileChanged(PdfFileItem? value)
    {
        if (value != null)
        {
            LoadPdf(value.FilePath);
            AddToRecent(value);
        }
    }

    #region Commands

    [RelayCommand]
    private void ToggleFileList()
    {
        ShowFileList = !ShowFileList;
    }

    [RelayCommand]
    private void SelectFile(PdfFileItem? item)
    {
        if (item == null)
        {
            return;
        }

        // Deselect previous
        foreach (var file in AllPdfFiles)
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
    private void BrowsePdf()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF Files|*.pdf|All Files|*.*",
            Title = "Select a PDF",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            var item = CreatePdfFileItem(dialog.FileName);
            if (item != null)
            {
                SelectFile(item);
            }
        }
    }

    [RelayCommand]
    private void OpenInDefaultApp()
    {
        if (HasPdf && File.Exists(GetLocalPath()))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GetLocalPath(),
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        if (HasPdf && File.Exists(GetLocalPath()))
        {
            Process.Start("explorer.exe", $"/select,\"{GetLocalPath()}\"");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await DiscoverPdfFilesAsync();
    }

    [RelayCommand]
    private void ClearPreview()
    {
        HasPdf = false;
        PdfPath = string.Empty;
        PdfFileName = string.Empty;
        FileInfo = string.Empty;
        SelectedFile = null;

        foreach (var file in AllPdfFiles)
        {
            file.IsSelected = false;
        }

        foreach (var file in RecentFiles)
        {
            file.IsSelected = false;
        }
    }

    [RelayCommand]
    private void RemoveFromRecent(PdfFileItem? item)
    {
        if (item == null)
        {
            return;
        }

        RecentFiles.Remove(item);
        OnPropertyChanged(nameof(HasRecentFiles));
        SaveRecentFiles();
    }

    #endregion

    #region Drag and Drop

    /// <summary>
    /// Handles files dropped onto the widget.
    /// </summary>
    public void HandleFileDrop(string[] files)
    {
        var pdfFile = files.FirstOrDefault(f =>
            f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(f));

        if (pdfFile != null)
        {
            var item = CreatePdfFileItem(pdfFile);
            if (item != null)
            {
                SelectFile(item);
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task DiscoverPdfFilesAsync()
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
                var pdfFiles = new List<PdfFileItem>();

                ScanFolderForPdfs(pdfFiles, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), 50);
                ScanFolderForPdfs(pdfFiles, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 30);
                ScanFolderForPdfs(pdfFiles, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 20);

                UpdatePdfFilesOnUiThread(pdfFiles);
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to discover PDF files");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ScanFolderForPdfs(List<PdfFileItem> pdfFiles, string folderPath, int limit)
    {
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        try
        {
            var files = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly)
                .Take(limit)
                .Select(CreatePdfFileItem)
                .OfType<PdfFileItem>();

            pdfFiles.AddRange(files);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to scan folder: {Folder}", folderPath);
        }
    }

    private void UpdatePdfFilesOnUiThread(List<PdfFileItem> pdfFiles)
    {
        var sortedFiles = pdfFiles
            .DistinctBy(f => f.FilePath.ToLowerInvariant())
            .OrderByDescending(f => f.LastModified)
            .ToList();

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            AllPdfFiles.Clear();
            foreach (var file in sortedFiles)
            {
                AllPdfFiles.Add(file);
            }

            ApplyFilter();
            OnPropertyChanged(nameof(HasFiles));
        });
    }

    private void ApplyFilter()
    {
        PdfFiles.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? AllPdfFiles
            : AllPdfFiles.Where(f =>
                f.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var file in filtered)
        {
            PdfFiles.Add(file);
        }

        OnPropertyChanged(nameof(HasFiles));
    }

    private static PdfFileItem? CreatePdfFileItem(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var fileInfo = new FileInfo(path);
            var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
            var sizeText = sizeMb > 1 ? $"{sizeMb:F1} MB" : $"{fileInfo.Length / 1024.0:F0} KB";

            return new PdfFileItem
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

    private void LoadPdf(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            HasPdf = false;
            PdfPath = string.Empty;
            PdfFileName = string.Empty;
            FileInfo = string.Empty;
            IsLoadingPdf = false;
            return;
        }

        try
        {
            IsLoadingPdf = true;

            // Convert file path to proper URI format for WebView2
            var uri = new Uri(path).AbsoluteUri;
            PdfPath = uri;
            HasPdf = true;
            PdfFileName = Path.GetFileName(path);

            var fileInfo = new FileInfo(path);
            var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
            var sizeText = sizeMb > 1 ? $"{sizeMb:F1} MB" : $"{fileInfo.Length / 1024.0:F0} KB";
            FileInfo = sizeText;

            // Reset loading state after a short delay
            Task.Delay(500).ContinueWith(_ => IsLoadingPdf = false,
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load PDF: {Path}", path);
            HasPdf = false;
            IsLoadingPdf = false;
        }
    }

    private string GetLocalPath()
    {
        if (string.IsNullOrEmpty(PdfPath))
        {
            return string.Empty;
        }

        try
        {
            var uri = new Uri(PdfPath);
            return uri.LocalPath;
        }
        catch
        {
            return PdfPath;
        }
    }

    private void AddToRecent(PdfFileItem item)
    {
        // Remove if already exists
        var existing = RecentFiles.FirstOrDefault(f =>
            f.FilePath.Equals(item.FilePath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            RecentFiles.Remove(existing);
        }

        // Add at top
        RecentFiles.Insert(0, new PdfFileItem
        {
            FilePath = item.FilePath,
            FileName = item.FileName,
            FileSize = item.FileSize,
            LastModified = DateTime.Now
        });

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

            if (paths != null)
            {
                foreach (var path in paths.Where(File.Exists).Take(5))
                {
                    var item = CreatePdfFileItem(path);
                    if (item != null)
                    {
                        RecentFiles.Add(item);
                    }
                }
            }

            OnPropertyChanged(nameof(HasRecentFiles));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to load recent PDFs");
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
            _logger.Warning(ex, "Failed to save recent PDFs");
        }
    }

    #endregion

    #region Lifecycle Methods

    public void OnInitialize()
    {
        // Already initialized in constructor, but can be called again if needed
        _ = DiscoverPdfFilesAsync();
    }

    public void OnDispose()
    {
        SaveRecentFiles();
    }

    #endregion
}
