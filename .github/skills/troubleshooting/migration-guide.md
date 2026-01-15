# Migration Guide

> **Category:** Troubleshooting | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Guide for migrating widgets between versions, handling breaking changes, and upgrading dependencies.

---

## Version Migration Checklist

### Pre-Migration
- [ ] Backup current working version
- [ ] Review changelog for breaking changes
- [ ] Test in isolated environment first
- [ ] Update version numbers consistently

### Post-Migration
- [ ] Run all tests
- [ ] Verify widget loads in host
- [ ] Check settings migration
- [ ] Test all user workflows

---

## Package Version Updates

### Safe Update Process

```powershell
# 1. Check current versions
dotnet list package

# 2. Check for updates
dotnet list package --outdated

# 3. Update specific package
dotnet add package PackageName --version X.Y.Z

# 4. Build and test
dotnet build
dotnet test
```

### ⚠️ CRITICAL: Locked Versions

These packages MUST NOT be updated beyond specified versions:

| Package | Max Version | Reason |
|---------|-------------|--------|
| Serilog | 3.1.1 | Host binding conflict with 4.x |
| CommunityToolkit.Mvvm | 8.2.2 | Breaking changes in 8.3.x |

---

## Settings Migration

### Version-Based Migration

```csharp
public class SettingsMigrator
{
    private readonly ILogger _log;
    
    public SettingsMigrator(ILogger log)
    {
        _log = log;
    }
    
    public MySettings Migrate(MySettings settings, int fromVersion, int toVersion)
    {
        _log.Information("Migrating settings from v{From} to v{To}", fromVersion, toVersion);
        
        var current = settings;
        
        // Apply migrations sequentially
        if (fromVersion < 2 && toVersion >= 2)
        {
            current = MigrateV1ToV2(current);
        }
        
        if (fromVersion < 3 && toVersion >= 3)
        {
            current = MigrateV2ToV3(current);
        }
        
        current.Version = toVersion;
        return current;
    }
    
    private MySettings MigrateV1ToV2(MySettings settings)
    {
        _log.Debug("Applying v1 -> v2 migration");
        
        // Example: Rename property
        // settings.NewPropertyName = settings.OldPropertyName;
        
        // Example: Change default
        if (settings.RefreshInterval == 0)
        {
            settings.RefreshInterval = 30;
        }
        
        return settings;
    }
    
    private MySettings MigrateV2ToV3(MySettings settings)
    {
        _log.Debug("Applying v2 -> v3 migration");
        
        // Example: Convert format
        // settings.Items = settings.LegacyItems.Select(x => new NewItem(x)).ToList();
        
        return settings;
    }
}
```

### Settings Service with Migration

```csharp
public class SettingsService<T> where T : IVersionedSettings, new()
{
    private const int CurrentVersion = 3;
    
    public async Task LoadAsync()
    {
        var settings = await ReadFromFileAsync();
        
        if (settings.Version < CurrentVersion)
        {
            var migrator = new SettingsMigrator(_log);
            settings = (T)migrator.Migrate(settings, settings.Version, CurrentVersion);
            await SaveAsync(); // Persist migrated settings
        }
        
        Current = settings;
    }
}

public interface IVersionedSettings
{
    int Version { get; set; }
}
```

---

## Data Format Migration

### JSON Structure Changes

```csharp
public class DataMigrator
{
    public string MigrateJson(string oldJson, int fromVersion)
    {
        var doc = JsonDocument.Parse(oldJson);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        
        writer.WriteStartObject();
        
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            switch (prop.Name)
            {
                // Rename property
                case "oldName" when fromVersion < 2:
                    writer.WritePropertyName("newName");
                    prop.Value.WriteTo(writer);
                    break;
                
                // Transform value
                case "items" when fromVersion < 3:
                    writer.WritePropertyName("items");
                    MigrateItemsArray(prop.Value, writer);
                    break;
                
                // Keep as-is
                default:
                    prop.WriteTo(writer);
                    break;
            }
        }
        
        // Add new required properties
        if (fromVersion < 2)
        {
            writer.WriteNumber("version", 2);
        }
        
        writer.WriteEndObject();
        writer.Flush();
        
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
```

---

## API Breaking Changes

### Handling Interface Changes

```csharp
// Old interface (v1)
public interface IWidgetV1
{
    void Initialize();
    void Close();
}

// New interface (v2) - adds method
public interface IWidget
{
    void OnInitialize();
    void OnResize(double width, double height);
    void OnClose();
}

// Adapter for gradual migration
public class WidgetV1Adapter : IWidget
{
    private readonly IWidgetV1 _legacyWidget;
    
    public WidgetV1Adapter(IWidgetV1 legacy)
    {
        _legacyWidget = legacy;
    }
    
    public void OnInitialize() => _legacyWidget.Initialize();
    public void OnResize(double width, double height) { /* No-op for v1 */ }
    public void OnClose() => _legacyWidget.Close();
}
```

---

## Manifest Migration

### Updating manifest.json

```json
// v1 manifest
{
  "name": "My Widget",
  "version": "1.0.0",
  "author": "Developer"
}

// v2 manifest (new required fields)
{
  "key": "my-widget",
  "name": "My Widget", 
  "version": "1.0.0",
  "author": {
    "name": "Developer",
    "email": "dev@example.com"
  },
  "minHostVersion": "1.0.0",
  "entryPoint": "MyWidget.dll",
  "factoryType": "MyWidget.WidgetFactory"
}
```

### Manifest Validator

```csharp
public class ManifestValidator
{
    public ValidationResult Validate(WidgetManifest manifest, int schemaVersion)
    {
        var errors = new List<string>();
        
        // v1 requirements
        if (string.IsNullOrEmpty(manifest.Name))
            errors.Add("'name' is required");
        
        if (string.IsNullOrEmpty(manifest.Version))
            errors.Add("'version' is required");
        
        // v2 requirements
        if (schemaVersion >= 2)
        {
            if (string.IsNullOrEmpty(manifest.Key))
                errors.Add("'key' is required in schema v2+");
            
            if (string.IsNullOrEmpty(manifest.EntryPoint))
                errors.Add("'entryPoint' is required in schema v2+");
        }
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
```

---

## .NET Version Migration

### Upgrading Target Framework

```xml
<!-- Before -->
<TargetFramework>net6.0-windows</TargetFramework>

<!-- After -->
<TargetFramework>net8.0-windows</TargetFramework>
```

### Breaking Changes Checklist (.NET 6 → 8)
- [ ] Review `System.Text.Json` behavior changes
- [ ] Check nullable reference type warnings
- [ ] Update deprecated API calls
- [ ] Verify WPF compatibility

---

## Rollback Procedure

```powershell
# 1. Keep backup of working version
Copy-Item -Path ".\MyWidget" -Destination ".\MyWidget.backup" -Recurse

# 2. If migration fails, restore
Remove-Item -Path ".\MyWidget" -Recurse
Move-Item -Path ".\MyWidget.backup" -Destination ".\MyWidget"

# 3. Restore previous package versions
dotnet add package Serilog --version 3.1.1
dotnet add package CommunityToolkit.Mvvm --version 8.2.2
```

---

## Best Practices

1. **Incremental migrations** - One version at a time
2. **Backup before migrating** - Always have rollback option
3. **Test thoroughly** - Especially settings/data migration
4. **Document changes** - Update changelog
5. **Version lock critical packages** - Prevent accidental updates

---

## Related Skills

- [versioning.md](../packaging/versioning.md) - Version management
- [common-issues.md](common-issues.md) - Post-migration issues

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added .NET migration guide |
| 1.0.0 | 2025-06-01 | Initial version |
