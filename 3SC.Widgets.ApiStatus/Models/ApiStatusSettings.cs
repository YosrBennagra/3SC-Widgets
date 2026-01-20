using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace _3SC.Widgets.ApiStatus;

public class ApiStatusSettings
{
    public List<SavedEndpoint> Endpoints { get; set; } = new();
    public int CheckIntervalSeconds { get; set; } = 60;
    public bool AlertOnDowntime { get; set; } = true;

    private static string GetSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsDir = Path.Combine(appData, "3SC", "Widgets", "ApiStatus");
        Directory.CreateDirectory(settingsDir);
        return Path.Combine(settingsDir, "settings.json");
    }

    public static ApiStatusSettings Load()
    {
        try
        {
            var path = GetSettingsPath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<ApiStatusSettings>(json) ?? new ApiStatusSettings();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to load API Status settings");
        }

        return new ApiStatusSettings();
    }

    public void Save()
    {
        try
        {
            var path = GetSettingsPath();
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to save API Status settings");
        }
    }
}

public class SavedEndpoint
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
