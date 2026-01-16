using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace _3SC.Widgets.AppLauncher
{
    public class AppLauncherWidgetViewModel : IDisposable
    {
        private readonly string _storagePath;
        public ObservableCollection<AppItem> Apps { get; } = new ObservableCollection<AppItem>();

        public AppLauncherWidgetViewModel()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "3SC");
            Directory.CreateDirectory(dir);
            _storagePath = Path.Combine(dir, "app_launcher.json");
            LoadApps();
        }

        private void LoadApps()
        {
            try
            {
                if (!File.Exists(_storagePath)) return;
                var json = File.ReadAllText(_storagePath);
                var items = JsonSerializer.Deserialize<AppItemDto[]>(json);
                if (items == null) return;
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var i in items)
                {
                    var p = i.Path ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(p)) continue;
                    if (seen.Contains(p)) continue;
                    seen.Add(p);
                    Apps.Add(new AppItem { Name = i.Name ?? string.Empty, Path = p, IsFavorite = i.IsFavorite });
                }
            }
            catch { }
        }

        public void SaveApps()
        {
            try
            {
                var dto = new AppItemDto[Apps.Count];
                for (int i = 0; i < Apps.Count; i++)
                {
                    dto[i] = new AppItemDto { Name = Apps[i].Name, Path = Apps[i].Path, IsFavorite = Apps[i].IsFavorite };
                }
                var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_storagePath, json);
            }
            catch { }
        }

        public void Dispose()
        {
            // No unmanaged resources
        }

        private record AppItemDto
        {
            public string? Name { get; init; }
            public string? Path { get; init; }
            public bool IsFavorite { get; init; }
        }
    }
}
