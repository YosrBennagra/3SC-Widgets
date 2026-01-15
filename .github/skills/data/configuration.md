# Widget Configuration

> **Category:** Data | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers widget configuration patterns, including build-time, runtime, and user configuration.

## Prerequisites

- [settings-management.md](settings-management.md)

---

## Configuration Layers

```
┌─────────────────────────────────────────┐
│         User Settings (Runtime)         │  ← Changed by user in settings UI
├─────────────────────────────────────────┤
│        Instance Settings (DB)           │  ← Per-widget-instance settings
├─────────────────────────────────────────┤
│      Default Settings (manifest)        │  ← Defined by developer
├─────────────────────────────────────────┤
│      Build Configuration (.csproj)      │  ← Compile-time settings
└─────────────────────────────────────────┘
```

---

## Build-Time Configuration

### .csproj Configuration

```xml
<PropertyGroup>
  <!-- Version embedded in assembly -->
  <Version>1.0.0</Version>
  <FileVersion>1.0.0.0</FileVersion>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  
  <!-- Widget metadata -->
  <Product>Clock Widget</Product>
  <Company>3SC</Company>
  <Authors>3SC Team</Authors>
  <Description>A customizable clock widget</Description>
  
  <!-- Build-time constants -->
  <DefineConstants Condition="'$(Configuration)' == 'Debug'">DEBUG;TRACE</DefineConstants>
  <DefineConstants Condition="'$(Configuration)' == 'Release'">RELEASE</DefineConstants>
</PropertyGroup>

<!-- Custom build properties -->
<PropertyGroup>
  <WidgetKey>clock</WidgetKey>
  <WidgetCategory>General</WidgetCategory>
</PropertyGroup>
```

### Conditional Compilation

```csharp
public class WidgetConfig
{
    public static bool IsDebugMode =>
#if DEBUG
        true;
#else
        false;
#endif
    
    public static string ApiEndpoint =>
#if DEBUG
        "https://api.dev.example.com";
#else
        "https://api.example.com";
#endif
    
    public static LogLevel MinLogLevel =>
#if DEBUG
        LogLevel.Debug;
#else
        LogLevel.Information;
#endif
}
```

---

## Manifest Default Configuration

```json
{
  "widgetKey": "my-widget",
  "displayName": "My Widget",
  "version": "1.0.0",
  
  "defaultSize": {
    "width": 300,
    "height": 200
  },
  
  "minSize": {
    "width": 150,
    "height": 100
  },
  
  "maxSize": {
    "width": 800,
    "height": 600
  },
  
  "defaults": {
    "refreshInterval": 60,
    "theme": "dark",
    "showTitle": true
  },
  
  "features": {
    "settings": true,
    "resize": true,
    "contextMenu": true
  }
}
```

### Reading Manifest Defaults

```csharp
public class ManifestConfig
{
    private static WidgetManifest? _manifest;
    
    public static WidgetManifest Manifest
    {
        get
        {
            _manifest ??= LoadManifest();
            return _manifest;
        }
    }
    
    private static WidgetManifest LoadManifest()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation);
        var manifestPath = Path.Combine(directory!, "manifest.json");
        
        if (!File.Exists(manifestPath))
        {
            Log.Warning("Manifest not found at {Path}", manifestPath);
            return new WidgetManifest();
        }
        
        var json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<WidgetManifest>(json) ?? new WidgetManifest();
    }
}
```

---

## Runtime Configuration

### Configuration Class

```csharp
/// <summary>
/// Widget runtime configuration.
/// Combines defaults with user settings.
/// </summary>
public class WidgetConfiguration
{
    // Identity (from manifest, read-only)
    public string WidgetKey { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Version { get; init; } = "1.0.0";
    
    // Size constraints (from manifest)
    public Size DefaultSize { get; init; } = new(300, 200);
    public Size MinSize { get; init; } = new(100, 100);
    public Size MaxSize { get; init; } = new(800, 600);
    
    // Runtime settings (user-modifiable)
    public int RefreshIntervalSeconds { get; set; } = 60;
    public double Opacity { get; set; } = 1.0;
    public bool IsLocked { get; set; }
    public string Theme { get; set; } = "dark";
    
    // Feature flags
    public bool HasSettings { get; init; }
    public bool CanResize { get; init; }
    public bool HasContextMenu { get; init; }
    
    /// <summary>
    /// Creates configuration from manifest with optional user overrides.
    /// </summary>
    public static WidgetConfiguration Create(WidgetManifest manifest, UserSettings? userSettings = null)
    {
        var config = new WidgetConfiguration
        {
            // From manifest
            WidgetKey = manifest.WidgetKey,
            DisplayName = manifest.DisplayName,
            Version = manifest.Version,
            DefaultSize = manifest.DefaultSize ?? new Size(300, 200),
            MinSize = manifest.MinSize ?? new Size(100, 100),
            MaxSize = manifest.MaxSize ?? new Size(800, 600),
            HasSettings = manifest.HasSettings,
            CanResize = manifest.Features?.Resize ?? true,
            HasContextMenu = manifest.Features?.ContextMenu ?? true,
        };
        
        // Apply user overrides
        if (userSettings != null)
        {
            config.RefreshIntervalSeconds = userSettings.RefreshInterval;
            config.Opacity = userSettings.Opacity;
            config.IsLocked = userSettings.IsLocked;
            config.Theme = userSettings.Theme;
        }
        
        return config;
    }
}
```

---

## Environment-Based Configuration

```csharp
public static class EnvironmentConfig
{
    public static string GetWidgetDataPath()
    {
        // Check for override environment variable
        var customPath = Environment.GetEnvironmentVariable("3SC_WIDGET_DATA_PATH");
        if (!string.IsNullOrEmpty(customPath))
            return customPath;
        
        // Default path
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Widgets");
    }
    
    public static string GetLogLevel()
    {
        return Environment.GetEnvironmentVariable("3SC_LOG_LEVEL") ?? "Information";
    }
    
    public static bool IsDevelopmentMode()
    {
        var env = Environment.GetEnvironmentVariable("3SC_ENVIRONMENT");
        return env?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
```

---

## Feature Flags

```csharp
/// <summary>
/// Feature flag configuration for progressive feature rollout.
/// </summary>
public class FeatureFlags
{
    private readonly Dictionary<string, bool> _flags;
    
    public FeatureFlags()
    {
        _flags = new Dictionary<string, bool>
        {
            // Default feature states
            ["new_ui"] = false,
            ["dark_mode"] = true,
            ["analytics"] = false,
            ["experimental_api"] = false
        };
    }
    
    public bool IsEnabled(string feature)
    {
        // Check user override first
        var userOverride = Environment.GetEnvironmentVariable($"FEATURE_{feature.ToUpperInvariant()}");
        if (bool.TryParse(userOverride, out var userValue))
            return userValue;
        
        // Fall back to default
        return _flags.TryGetValue(feature, out var value) && value;
    }
    
    public void SetFlag(string feature, bool enabled)
    {
        _flags[feature] = enabled;
    }
}

// Usage
if (features.IsEnabled("new_ui"))
{
    // Show new UI
}
```

---

## Validation

```csharp
public class ConfigurationValidator
{
    public static ValidationResult Validate(WidgetConfiguration config)
    {
        var errors = new List<string>();
        
        // Validate widget key
        if (string.IsNullOrEmpty(config.WidgetKey))
            errors.Add("Widget key is required");
        else if (!Regex.IsMatch(config.WidgetKey, @"^[a-z][a-z0-9-]*$"))
            errors.Add("Widget key must be lowercase alphanumeric with dashes");
        
        // Validate refresh interval
        if (config.RefreshIntervalSeconds < 5)
            errors.Add("Refresh interval must be at least 5 seconds");
        if (config.RefreshIntervalSeconds > 86400)
            errors.Add("Refresh interval must not exceed 24 hours");
        
        // Validate opacity
        if (config.Opacity < 0.1 || config.Opacity > 1.0)
            errors.Add("Opacity must be between 0.1 and 1.0");
        
        // Validate size constraints
        if (config.MinSize.Width > config.MaxSize.Width)
            errors.Add("Min width cannot exceed max width");
        if (config.MinSize.Height > config.MaxSize.Height)
            errors.Add("Min height cannot exceed max height");
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
}
```

---

## Configuration Service

```csharp
/// <summary>
/// Centralized configuration service for widgets.
/// </summary>
public class ConfigurationService
{
    private readonly string _widgetKey;
    private WidgetConfiguration _config;
    private SettingsService<UserSettings> _settingsService;
    
    public WidgetConfiguration Configuration => _config;
    
    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
    
    public ConfigurationService(string widgetKey)
    {
        _widgetKey = widgetKey;
        _settingsService = new SettingsService<UserSettings>(widgetKey);
        _config = LoadConfiguration();
    }
    
    private WidgetConfiguration LoadConfiguration()
    {
        var manifest = ManifestConfig.Manifest;
        var userSettings = _settingsService.Load();
        return WidgetConfiguration.Create(manifest, userSettings);
    }
    
    public void UpdateSetting<T>(string key, T value)
    {
        var property = typeof(WidgetConfiguration).GetProperty(key);
        if (property == null || !property.CanWrite)
        {
            Log.Warning("Cannot update setting: {Key}", key);
            return;
        }
        
        var oldValue = property.GetValue(_config);
        property.SetValue(_config, value);
        
        // Persist changes
        SaveUserSettings();
        
        // Notify listeners
        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
        {
            Key = key,
            OldValue = oldValue,
            NewValue = value
        });
    }
    
    private void SaveUserSettings()
    {
        var userSettings = new UserSettings
        {
            RefreshInterval = _config.RefreshIntervalSeconds,
            Opacity = _config.Opacity,
            IsLocked = _config.IsLocked,
            Theme = _config.Theme
        };
        
        _settingsService.Save(userSettings);
    }
    
    public void Reset()
    {
        _settingsService.Delete();
        _config = LoadConfiguration();
        
        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
        {
            Key = "*",
            OldValue = null,
            NewValue = null
        });
    }
}

public class ConfigurationChangedEventArgs : EventArgs
{
    public string Key { get; init; } = "";
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
```

---

## Dependency Injection Pattern

```csharp
/// <summary>
/// Simple service locator for widget configuration.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();
    
    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }
    
    public static T? Get<T>() where T : class
    {
        return _services.TryGetValue(typeof(T), out var service) ? (T)service : null;
    }
    
    public static T GetRequired<T>() where T : class
    {
        return Get<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }
    
    public static void Clear()
    {
        _services.Clear();
    }
}

// Registration (in widget initialization)
public void OnInitialize()
{
    var config = new ConfigurationService(WidgetKey);
    ServiceLocator.Register(config);
    
    var logger = CreateLogger();
    ServiceLocator.Register(logger);
}

// Usage
var config = ServiceLocator.GetRequired<ConfigurationService>();
var refreshInterval = config.Configuration.RefreshIntervalSeconds;
```

---

## Best Practices

1. **Layer configuration** - Build-time → Manifest → User settings
2. **Validate early** - Validate configuration on load
3. **Provide defaults** - Never assume values exist
4. **Support reset** - Allow users to reset to defaults
5. **Emit events** - Notify when configuration changes

---

## Related Skills

- [settings-management.md](settings-management.md) - User settings
- [manifest-specification.md](../core/manifest-specification.md) - Manifest format

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added validation, service pattern |
| 1.0.0 | 2025-06-01 | Initial version |
