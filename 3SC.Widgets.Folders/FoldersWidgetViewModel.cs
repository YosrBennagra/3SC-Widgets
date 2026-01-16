using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.Folders
{
    public partial class FolderHubWidgetViewModel : ObservableObject, IDisposable
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<FolderHubWidgetViewModel>();
        private bool _disposed;
        private static readonly string FoldersFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "folder_hub.json");
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        [ObservableProperty]
        private string _title = "Folder Hub";

        public ObservableCollection<FolderItem> Folders { get; } = new();

        public FolderHubWidgetViewModel()
        {
            LoadFolders();
            Log.Debug("FolderHubWidgetViewModel initialized with {Count} folders", Folders.Count);
        }

        public void LoadFolders()
        {
            try
            {
                var data = LoadFolderData();
                if (data == null)
                {
                    return;
                }

                foreach (var item in data.Where(i => !string.IsNullOrEmpty(i.Path) && Directory.Exists(i.Path)))
                {
                    Folders.Add(new FolderItem
                    {
                        Name = item.Name ?? string.Empty,
                        Path = item.Path ?? string.Empty,
                        IsFavorite = item.IsFavorite
                    });
                }

                Log.Information("Loaded {Count} folders from storage", Folders.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load folders");
            }
        }

        public void SaveFolders()
        {
            try
            {
                var dir = Path.GetDirectoryName(FoldersFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var data = Folders.Select(f => new FolderData
                {
                    Name = f.Name,
                    Path = f.Path,
                    IsFavorite = f.IsFavorite
                }).ToArray();

                var json = JsonSerializer.Serialize(data, JsonOptions);
                File.WriteAllText(FoldersFilePath, json);

                Log.Debug("Saved {Count} folders to storage", Folders.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save folders");
            }
        }

        [RelayCommand]
        private void OpenSettings()
            => Debug.WriteLine("Folder Hub settings opened");

        private static FolderData[]? LoadFolderData()
        {
            if (!File.Exists(FoldersFilePath))
            {
                return null;
            }

            var json = File.ReadAllText(FoldersFilePath);
            return JsonSerializer.Deserialize<FolderData[]>(json);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                SaveFolders();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private sealed class FolderData
        {
            public string? Name { get; set; }
            public string? Path { get; set; }
            public bool IsFavorite { get; set; }
        }
    }
}
