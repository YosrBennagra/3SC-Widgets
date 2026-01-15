using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using _3SC.Widgets.GameVault.Abstractions;
using _3SC.Widgets.GameVault.Helpers;

namespace _3SC.Widgets.GameVault;

public abstract partial class LauncherAppsViewModelBase<TItem> : ObservableObject, IDisposable
    where TItem : class, ILauncherItem, new()
{
    private bool _disposed;

    protected abstract string AppsFilePath { get; }
    protected abstract string SettingsOpenedMessage { get; }

    [ObservableProperty]
    private string _title;

    public ObservableCollection<TItem> Apps { get; } = new();

    protected LauncherAppsViewModelBase(string defaultTitle)
    {
        _title = defaultTitle;
        LoadApps();
    }

    public void LoadApps()
    {
        try
        {
            var data = LoadAppData();
            if (data == null)
            {
                return;
            }

            foreach (var item in data)
            {
                Apps.Add(CreateItem(item));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load apps: {ex.Message}");
        }
    }

    public void SaveApps()
    {
        try
        {
            var dir = Path.GetDirectoryName(AppsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var data = Apps.Select(a => new AppData
            {
                Name = a.Name,
                Path = a.Path,
                IsFavorite = a.IsFavorite
            }).ToArray();

            var json = JsonHelper.Serialize(data);
            File.WriteAllText(AppsFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save apps: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenSettings() => Debug.WriteLine(SettingsOpenedMessage);

    private AppData[]? LoadAppData()
    {
        if (!File.Exists(AppsFilePath))
        {
            return null;
        }

        var json = File.ReadAllText(AppsFilePath);
        return JsonHelper.Deserialize<AppData[]>(json);
    }

    private static TItem CreateItem(AppData item)
    {
        var app = new TItem
        {
            Name = item.Name ?? string.Empty,
            Path = item.Path ?? string.Empty,
            IsFavorite = item.IsFavorite
        };

        app.Icon = LauncherWidgetHelpers.TryLoadIcon(app.Path);

        return app;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            SaveApps();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private sealed class AppData
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool IsFavorite { get; set; }
    }
}
