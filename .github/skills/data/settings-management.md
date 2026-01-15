# Settings Management

> **Category:** Data | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers implementing settings storage, retrieval, and UI for 3SC widgets.

## Prerequisites

- [mvvm-patterns.md](../ui/mvvm-patterns.md)
- [serialization.md](serialization.md)

---

## Settings Storage Location

```
%APPDATA%\3SC\Widgets\{widget-key}\
├── settings.json       # Widget settings
├── state.json          # Runtime state (optional)
└── logs\               # Widget logs
```

### Path Helper

```csharp
public static class WidgetPaths
{
    public static string GetWidgetDataPath(string widgetKey)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Widgets", widgetKey);
    }
    
    public static string GetSettingsPath(string widgetKey)
    {
        return Path.Combine(GetWidgetDataPath(widgetKey), "settings.json");
    }
    
    public static string GetStatePath(string widgetKey)
    {
        return Path.Combine(GetWidgetDataPath(widgetKey), "state.json");
    }
    
    public static string GetLogPath(string widgetKey)
    {
        return Path.Combine(GetWidgetDataPath(widgetKey), "logs");
    }
    
    public static void EnsureDirectoryExists(string widgetKey)
    {
        var path = GetWidgetDataPath(widgetKey);
        Directory.CreateDirectory(path);
    }
}
```

---

## Settings Class Pattern

### Basic Settings

```csharp
using System.Text.Json.Serialization;

namespace _3SC.Widgets.Clock.ValueObjects;

/// <summary>
/// Settings for the Clock widget.
/// All properties should have sensible defaults.
/// </summary>
public class ClockWidgetSettings
{
    /// <summary>
    /// IANA timezone ID (e.g., "America/New_York", "Europe/London").
    /// </summary>
    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;
    
    /// <summary>
    /// Whether to use 24-hour format.
    /// </summary>
    [JsonPropertyName("use24HourFormat")]
    public bool Use24HourFormat { get; set; } = false;
    
    /// <summary>
    /// Whether to show seconds.
    /// </summary>
    [JsonPropertyName("showSeconds")]
    public bool ShowSeconds { get; set; } = true;
    
    /// <summary>
    /// Whether to show date below time.
    /// </summary>
    [JsonPropertyName("showDate")]
    public bool ShowDate { get; set; } = true;
    
    /// <summary>
    /// Date format string.
    /// </summary>
    [JsonPropertyName("dateFormat")]
    public string DateFormat { get; set; } = "dddd, MMMM d";
    
    /// <summary>
    /// Creates default settings.
    /// </summary>
    public static ClockWidgetSettings Default() => new();
    
    /// <summary>
    /// Validates settings and fixes invalid values.
    /// </summary>
    public void Validate()
    {
        // Validate timezone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch
        {
            TimeZoneId = TimeZoneInfo.Local.Id;
        }
        
        // Validate date format
        if (string.IsNullOrWhiteSpace(DateFormat))
        {
            DateFormat = "dddd, MMMM d";
        }
    }
}
```

### Settings with Position/Size

```csharp
public class WidgetSettings
{
    // User preferences
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";
    
    [JsonPropertyName("opacity")]
    public double Opacity { get; set; } = 1.0;
    
    // Window state
    [JsonPropertyName("position")]
    public PositionSettings? Position { get; set; }
    
    [JsonPropertyName("size")]
    public SizeSettings? Size { get; set; }
    
    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; set; }
}

public class PositionSettings
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
}

public class SizeSettings
{
    [JsonPropertyName("width")]
    public double Width { get; set; }
    
    [JsonPropertyName("height")]
    public double Height { get; set; }
}
```

---

## Settings Service

```csharp
using System.Text.Json;
using Serilog;

namespace _3SC.Widgets.MyWidget.Services;

/// <summary>
/// Service for loading and saving widget settings.
/// </summary>
public class SettingsService<T> where T : class, new()
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private T? _cachedSettings;
    
    public SettingsService(string widgetKey)
    {
        _settingsPath = WidgetPaths.GetSettingsPath(widgetKey);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }
    
    /// <summary>
    /// Loads settings from disk. Returns default if file doesn't exist.
    /// </summary>
    public T Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                Log.Debug("Settings file not found, using defaults: {Path}", _settingsPath);
                _cachedSettings = new T();
                return _cachedSettings;
            }
            
            var json = File.ReadAllText(_settingsPath);
            _cachedSettings = JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            
            Log.Debug("Settings loaded from {Path}", _settingsPath);
            return _cachedSettings;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load settings, using defaults");
            _cachedSettings = new T();
            return _cachedSettings;
        }
    }
    
    /// <summary>
    /// Loads settings asynchronously.
    /// </summary>
    public async Task<T> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                _cachedSettings = new T();
                return _cachedSettings;
            }
            
            var json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
            _cachedSettings = JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            
            return _cachedSettings;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load settings async");
            _cachedSettings = new T();
            return _cachedSettings;
        }
    }
    
    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    public void Save(T settings)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(_settingsPath, json);
            
            _cachedSettings = settings;
            Log.Debug("Settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings to {Path}", _settingsPath);
        }
    }
    
    /// <summary>
    /// Saves settings asynchronously.
    /// </summary>
    public async Task SaveAsync(T settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json, cancellationToken);
            
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings async");
        }
    }
    
    /// <summary>
    /// Gets cached settings (last loaded/saved).
    /// </summary>
    public T? GetCached() => _cachedSettings;
    
    /// <summary>
    /// Deletes settings file.
    /// </summary>
    public void Delete()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
                Log.Information("Settings deleted: {Path}", _settingsPath);
            }
            
            _cachedSettings = null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete settings");
        }
    }
}
```

---

## ViewModel Integration

```csharp
public partial class ClockWidgetViewModel : ObservableObject, IDisposable
{
    private readonly SettingsService<ClockWidgetSettings> _settingsService;
    private ClockWidgetSettings _settings = new();
    
    // Observable settings properties
    [ObservableProperty]
    private bool _use24HourFormat;
    
    [ObservableProperty]
    private bool _showSeconds;
    
    [ObservableProperty]
    private bool _showDate;
    
    [ObservableProperty]
    private string _selectedTimeZoneId = TimeZoneInfo.Local.Id;
    
    public ClockWidgetViewModel()
    {
        _settingsService = new SettingsService<ClockWidgetSettings>("clock");
    }
    
    public void LoadSettings()
    {
        _settings = _settingsService.Load();
        _settings.Validate();
        
        // Update observable properties
        Use24HourFormat = _settings.Use24HourFormat;
        ShowSeconds = _settings.ShowSeconds;
        ShowDate = _settings.ShowDate;
        SelectedTimeZoneId = _settings.TimeZoneId;
    }
    
    public void SaveSettings()
    {
        // Update settings from observable properties
        _settings.Use24HourFormat = Use24HourFormat;
        _settings.ShowSeconds = ShowSeconds;
        _settings.ShowDate = ShowDate;
        _settings.TimeZoneId = SelectedTimeZoneId;
        
        _settingsService.Save(_settings);
    }
    
    // Auto-save when properties change
    partial void OnUse24HourFormatChanged(bool value) => SaveSettings();
    partial void OnShowSecondsChanged(bool value) => SaveSettings();
    partial void OnShowDateChanged(bool value) => SaveSettings();
    partial void OnSelectedTimeZoneIdChanged(string value) => SaveSettings();
    
    public void Dispose()
    {
        SaveSettings();
    }
}
```

---

## Debounced Auto-Save

For settings that change frequently (like text input):

```csharp
public partial class NotesWidgetViewModel : ObservableObject
{
    private readonly SettingsService<NotesSettings> _settingsService;
    private Timer? _saveTimer;
    private const int SaveDelayMs = 1000; // 1 second debounce
    
    [ObservableProperty]
    private string _noteContent = "";
    
    [ObservableProperty]
    private DateTime? _lastSaved;
    
    partial void OnNoteContentChanged(string value)
    {
        // Reset debounce timer
        _saveTimer?.Dispose();
        _saveTimer = new Timer(
            _ => Application.Current.Dispatcher.Invoke(SaveSettings),
            null,
            SaveDelayMs,
            Timeout.Infinite);
    }
    
    private void SaveSettings()
    {
        var settings = new NotesSettings { Content = NoteContent };
        _settingsService.Save(settings);
        LastSaved = DateTime.Now;
    }
    
    public void Dispose()
    {
        _saveTimer?.Dispose();
        SaveSettings(); // Final save
    }
}
```

---

## Settings UI

### Settings Window

```xml
<Window x:Class="_3SC.Widgets.Clock.ClockSettingsWindow"
        Title="Clock Settings"
        Width="350" Height="400"
        WindowStartupLocation="CenterOwner"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent">
    
    <Border Background="{StaticResource WidgetSurface}"
            BorderBrush="{StaticResource Border}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="16">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <!-- Header -->
            <DockPanel Grid.Row="0" Margin="0,0,0,16">
                <TextBlock Text="Settings" 
                           Style="{StaticResource HeadingText}"
                           DockPanel.Dock="Left"/>
                <Button Style="{StaticResource IconButton}"
                        Command="{Binding CloseCommand}"
                        Content="✕"
                        HorizontalAlignment="Right"/>
            </DockPanel>
            
            <!-- Settings Content -->
            <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
                <StackPanel>
                    
                    <!-- Time Format Section -->
                    <TextBlock Text="TIME FORMAT" 
                               Style="{StaticResource CaptionText}"
                               Margin="0,0,0,8"/>
                    
                    <CheckBox Content="Use 24-hour format"
                              IsChecked="{Binding Use24HourFormat}"
                              Margin="0,0,0,8"/>
                    
                    <CheckBox Content="Show seconds"
                              IsChecked="{Binding ShowSeconds}"
                              Margin="0,0,0,16"/>
                    
                    <!-- Date Section -->
                    <TextBlock Text="DATE" 
                               Style="{StaticResource CaptionText}"
                               Margin="0,0,0,8"/>
                    
                    <CheckBox Content="Show date"
                              IsChecked="{Binding ShowDate}"
                              Margin="0,0,0,16"/>
                    
                    <!-- Timezone Section -->
                    <TextBlock Text="TIMEZONE" 
                               Style="{StaticResource CaptionText}"
                               Margin="0,0,0,8"/>
                    
                    <ComboBox ItemsSource="{Binding TimeZones}"
                              SelectedValue="{Binding SelectedTimeZoneId}"
                              SelectedValuePath="Id"
                              DisplayMemberPath="DisplayName"
                              Style="{StaticResource WidgetComboBox}"/>
                    
                </StackPanel>
            </ScrollViewer>
            
            <!-- Footer -->
            <StackPanel Grid.Row="2" 
                        Orientation="Horizontal"
                        HorizontalAlignment="Right"
                        Margin="0,16,0,0">
                
                <Button Content="Reset to Defaults"
                        Command="{Binding ResetCommand}"
                        Style="{StaticResource SecondaryButton}"
                        Margin="0,0,8,0"/>
                
                <Button Content="Done"
                        Command="{Binding CloseCommand}"
                        Style="{StaticResource PrimaryButton}"/>
                
            </StackPanel>
        </Grid>
    </Border>
</Window>
```

### Settings ViewModel

```csharp
public partial class ClockSettingsViewModel : ObservableObject
{
    private readonly Action _onClose;
    private readonly ClockWidgetViewModel _mainViewModel;
    
    public ObservableCollection<TimeZoneInfo> TimeZones { get; } = new();
    
    [ObservableProperty]
    private bool _use24HourFormat;
    
    [ObservableProperty]
    private bool _showSeconds;
    
    [ObservableProperty]
    private bool _showDate;
    
    [ObservableProperty]
    private string _selectedTimeZoneId = TimeZoneInfo.Local.Id;
    
    public ClockSettingsViewModel(ClockWidgetViewModel mainViewModel, Action onClose)
    {
        _mainViewModel = mainViewModel;
        _onClose = onClose;
        
        // Load timezones
        foreach (var tz in TimeZoneInfo.GetSystemTimeZones().OrderBy(t => t.BaseUtcOffset))
        {
            TimeZones.Add(tz);
        }
        
        // Sync with main viewmodel
        Use24HourFormat = mainViewModel.Use24HourFormat;
        ShowSeconds = mainViewModel.ShowSeconds;
        ShowDate = mainViewModel.ShowDate;
        SelectedTimeZoneId = mainViewModel.SelectedTimeZoneId;
    }
    
    // Sync changes back to main viewmodel
    partial void OnUse24HourFormatChanged(bool value) => _mainViewModel.Use24HourFormat = value;
    partial void OnShowSecondsChanged(bool value) => _mainViewModel.ShowSeconds = value;
    partial void OnShowDateChanged(bool value) => _mainViewModel.ShowDate = value;
    partial void OnSelectedTimeZoneIdChanged(string value) => _mainViewModel.SelectedTimeZoneId = value;
    
    [RelayCommand]
    private void Reset()
    {
        Use24HourFormat = false;
        ShowSeconds = true;
        ShowDate = true;
        SelectedTimeZoneId = TimeZoneInfo.Local.Id;
    }
    
    [RelayCommand]
    private void Close() => _onClose();
}
```

---

## Best Practices

### 1. Always Have Defaults

```csharp
public class Settings
{
    public int RefreshInterval { get; set; } = 60; // Default: 60 seconds
    public bool AutoStart { get; set; } = true;    // Default: enabled
}
```

### 2. Validate on Load

```csharp
public void Validate()
{
    // Clamp values to valid ranges
    RefreshInterval = Math.Clamp(RefreshInterval, 10, 3600);
    
    // Fix invalid values
    if (string.IsNullOrEmpty(ApiUrl))
    {
        ApiUrl = DefaultApiUrl;
    }
}
```

### 3. Handle Corrupted Files

```csharp
public T Load()
{
    try
    {
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
    }
    catch (JsonException ex)
    {
        Log.Warning(ex, "Corrupted settings file, backing up and using defaults");
        BackupCorruptedFile();
        return new T();
    }
}
```

### 4. Version Settings

```csharp
public class Settings
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
    
    // Settings properties...
    
    public void Migrate()
    {
        if (Version < 2)
        {
            // Migrate from v1 to v2
            Version = 2;
        }
    }
}
```

---

## Related Skills

- [serialization.md](serialization.md) - JSON patterns
- [data-persistence.md](data-persistence.md) - File storage
- [mvvm-patterns.md](../ui/mvvm-patterns.md) - ViewModel binding

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added settings UI patterns |
| 1.0.0 | 2025-06-01 | Initial version |
