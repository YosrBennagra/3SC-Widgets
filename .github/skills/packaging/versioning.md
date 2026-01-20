# Versioning

> **Category:** Packaging | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers semantic versioning for widgets, version synchronization between manifest and assembly, and upgrade compatibility.

---

## Semantic Versioning

Widgets follow [SemVer](https://semver.org/) format: `MAJOR.MINOR.PATCH`

```
v1.2.3
│ │ └── PATCH: Bug fixes, no API changes
│ └──── MINOR: New features, backward compatible
└────── MAJOR: Breaking changes
```

### Examples

| Change | Version Bump | Example |
|--------|--------------|---------|
| Fix settings save bug | PATCH | 1.0.0 → 1.0.1 |
| Add new display option | MINOR | 1.0.1 → 1.1.0 |
| Restructure settings format | MAJOR | 1.1.0 → 2.0.0 |

## Version Locations

Version must be consistent across multiple files:

```
┌─────────────────────────────────────────┐
│           manifest.json                  │  ← Source of truth
│           "version": "1.2.3"             │
└───────────────────┬─────────────────────┘
                    │
        ┌───────────┼──────────────┐
        ▼           ▼              ▼
   .csproj     CHANGELOG    Build-Widget.ps1
   <Version>      v1.2.3     [reads from manifest]
```

**NOTE:** The centralized `Build-Widget.ps1` script automatically reads the version from `manifest.json`, so you don't need custom packaging scripts.

---

## Manifest Version

```json
{
  "widgetKey": "my-widget",
  "version": "1.2.3",
  "minHostVersion": "3.0.0"
}
```

### Required Fields

| Field | Description | Example |
|-------|-------------|---------|
| `version` | Widget version (SemVer) | "1.2.3" |
| `minHostVersion` | Minimum 3SC version required | "3.0.0" |

---

## Assembly Version (.csproj)

```xml
<PropertyGroup>
  <!-- Semantic version (matches manifest) -->
  <Version>1.2.3</Version>
  
  <!-- File version (4-part) -->
  <FileVersion>1.2.3.0</FileVersion>
  
  <!-- Assembly version (can be fixed for compatibility) -->
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
</PropertyGroup>
```

### Version Types

| Property | Format | Purpose |
|----------|--------|---------|
| `Version` | X.Y.Z | NuGet, display |
| `FileVersion` | X.Y.Z.B | Windows properties |
| `AssemblyVersion` | X.Y.Z.B | .NET binding (keep stable) |

---

## Auto-Sync Version Script

### sync-version.ps1

```powershell
#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Syncs version from manifest.json to .csproj
#>

$manifest = Get-Content 'manifest.json' | ConvertFrom-Json
$version = $manifest.version

Write-Host "Syncing version: $version" -ForegroundColor Cyan

# Find .csproj
$csproj = Get-ChildItem '*.csproj' | Select-Object -First 1
if (-not $csproj) {
    throw "No .csproj found"
}

$content = Get-Content $csproj.FullName -Raw

# Update Version
$content = $content -replace '<Version>[^<]*</Version>', "<Version>$version</Version>"

# Update FileVersion
$content = $content -replace '<FileVersion>[^<]*</FileVersion>', "<FileVersion>$version.0</FileVersion>"

Set-Content $csproj.FullName $content -NoNewline

Write-Host "Updated: $($csproj.Name)" -ForegroundColor Green
```

### MSBuild Auto-Sync

```xml
<Target Name="SyncVersionFromManifest" BeforeTargets="GetAssemblyVersion">
  <PropertyGroup>
    <ManifestPath>$(MSBuildProjectDirectory)\manifest.json</ManifestPath>
  </PropertyGroup>
  
  <ReadLinesFromFile File="$(ManifestPath)" Condition="Exists('$(ManifestPath)')">
    <Output TaskParameter="Lines" ItemName="ManifestLines" />
  </ReadLinesFromFile>
  
  <PropertyGroup>
    <ManifestContent>@(ManifestLines)</ManifestContent>
  </PropertyGroup>
  
  <Exec Command="powershell -Command &quot;(Get-Content '$(ManifestPath)' | ConvertFrom-Json).version&quot;"
        ConsoleToMSBuild="true"
        Condition="Exists('$(ManifestPath)')">
    <Output TaskParameter="ConsoleOutput" PropertyName="ManifestVersion" />
  </Exec>
  
  <PropertyGroup Condition="'$(ManifestVersion)' != ''">
    <Version>$(ManifestVersion)</Version>
    <FileVersion>$(ManifestVersion).0</FileVersion>
  </PropertyGroup>
  
  <Message Importance="high" Text="Building v$(Version)" />
</Target>
```

---

## Changelog

### CHANGELOG.md Template

```markdown
# Changelog

All notable changes to this widget will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- New feature X

## [1.2.0] - 2025-03-15

### Added
- Option to customize refresh interval
- Keyboard shortcuts for common actions

### Changed
- Improved performance of data loading

### Fixed
- Settings not persisting on close

## [1.1.0] - 2025-02-01

### Added
- Dark theme support
- Context menu with quick actions

## [1.0.0] - 2025-01-15

### Added
- Initial release
- Basic widget functionality
- Settings window

[Unreleased]: https://github.com/3sc/widgets/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/3sc/widgets/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/3sc/widgets/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/3sc/widgets/releases/tag/v1.0.0
```

---

## Version Comparison

```csharp
/// <summary>
/// Semantic version comparison utility.
/// </summary>
public class SemanticVersion : IComparable<SemanticVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Prerelease { get; }
    
    private SemanticVersion(int major, int minor, int patch, string? prerelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
    }
    
    public static SemanticVersion Parse(string version)
    {
        var match = Regex.Match(version, @"^(\d+)\.(\d+)\.(\d+)(?:-(.+))?$");
        if (!match.Success)
            throw new FormatException($"Invalid version: {version}");
        
        return new SemanticVersion(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value),
            match.Groups[4].Success ? match.Groups[4].Value : null
        );
    }
    
    public static bool TryParse(string version, out SemanticVersion? result)
    {
        try
        {
            result = Parse(version);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
    
    public int CompareTo(SemanticVersion? other)
    {
        if (other is null) return 1;
        
        var major = Major.CompareTo(other.Major);
        if (major != 0) return major;
        
        var minor = Minor.CompareTo(other.Minor);
        if (minor != 0) return minor;
        
        var patch = Patch.CompareTo(other.Patch);
        if (patch != 0) return patch;
        
        // Pre-release versions are lower
        if (Prerelease != null && other.Prerelease == null) return -1;
        if (Prerelease == null && other.Prerelease != null) return 1;
        
        return string.Compare(Prerelease, other.Prerelease, StringComparison.Ordinal);
    }
    
    public bool IsCompatibleWith(SemanticVersion other)
    {
        // Same major version = compatible
        return Major == other.Major;
    }
    
    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        return Prerelease != null ? $"{version}-{Prerelease}" : version;
    }
    
    public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) > 0;
    public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) < 0;
    public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;
    public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;
    public static bool operator ==(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == 0;
    public static bool operator !=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) != 0;
    
    public override bool Equals(object? obj) => obj is SemanticVersion v && this == v;
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Prerelease);
}
```

---

## Upgrade Detection

```csharp
public class VersionManager
{
    private readonly string _widgetKey;
    private readonly ILogger _log;
    
    public VersionManager(string widgetKey, ILogger log)
    {
        _widgetKey = widgetKey;
        _log = log;
    }
    
    public UpgradeInfo CheckUpgrade()
    {
        var currentVersion = GetCurrentVersion();
        var previousVersion = GetPreviousVersion();
        
        if (previousVersion == null)
        {
            _log.Information("Fresh install: v{Version}", currentVersion);
            return new UpgradeInfo
            {
                IsNewInstall = true,
                CurrentVersion = currentVersion
            };
        }
        
        if (currentVersion > previousVersion)
        {
            _log.Information("Upgraded: v{From} → v{To}", previousVersion, currentVersion);
            return new UpgradeInfo
            {
                IsUpgrade = true,
                PreviousVersion = previousVersion,
                CurrentVersion = currentVersion,
                IsMajorUpgrade = currentVersion.Major > previousVersion.Major
            };
        }
        
        return new UpgradeInfo { CurrentVersion = currentVersion };
    }
    
    private SemanticVersion GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return SemanticVersion.Parse($"{version?.Major}.{version?.Minor}.{version?.Build}");
    }
    
    private SemanticVersion? GetPreviousVersion()
    {
        var path = GetVersionFilePath();
        if (!File.Exists(path)) return null;
        
        var stored = File.ReadAllText(path).Trim();
        return SemanticVersion.TryParse(stored, out var v) ? v : null;
    }
    
    public void SaveCurrentVersion()
    {
        var version = GetCurrentVersion();
        File.WriteAllText(GetVersionFilePath(), version.ToString());
    }
    
    private string GetVersionFilePath()
    {
        var dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", _widgetKey);
        
        Directory.CreateDirectory(dataPath);
        return Path.Combine(dataPath, ".version");
    }
}

public class UpgradeInfo
{
    public bool IsNewInstall { get; init; }
    public bool IsUpgrade { get; init; }
    public bool IsMajorUpgrade { get; init; }
    public SemanticVersion? PreviousVersion { get; init; }
    public SemanticVersion CurrentVersion { get; init; } = null!;
}
```

---

## Migration On Upgrade

```csharp
public class MigrationManager
{
    private readonly Dictionary<string, Action<SemanticVersion>> _migrations = new();
    private readonly ILogger _log;
    
    public MigrationManager(ILogger log)
    {
        _log = log;
        
        // Register migrations
        _migrations["1.0.0"] = MigrateFrom_1_0_0;
        _migrations["1.1.0"] = MigrateFrom_1_1_0;
        _migrations["2.0.0"] = MigrateFrom_2_0_0;
    }
    
    public void ApplyMigrations(SemanticVersion from, SemanticVersion to)
    {
        _log.Information("Checking migrations from {From} to {To}", from, to);
        
        var orderedMigrations = _migrations.Keys
            .Select(SemanticVersion.Parse)
            .Where(v => v > from && v <= to)
            .OrderBy(v => v);
        
        foreach (var version in orderedMigrations)
        {
            _log.Information("Applying migration for v{Version}", version);
            _migrations[version.ToString()](version);
        }
    }
    
    private void MigrateFrom_1_0_0(SemanticVersion v)
    {
        // Migration logic for 1.0.0 → 1.1.0
        _log.Debug("Migrating settings format");
    }
    
    private void MigrateFrom_1_1_0(SemanticVersion v)
    {
        // Migration logic for 1.1.0 → 2.0.0
    }
    
    private void MigrateFrom_2_0_0(SemanticVersion v)
    {
        // Migration logic for 2.0.0+
    }
}
```

---

## Pre-release Versions

```json
{
  "version": "2.0.0-beta.1"
}
```

### Conventions

| Tag | Purpose | Example |
|-----|---------|---------|
| `alpha` | Early development | 2.0.0-alpha.1 |
| `beta` | Feature complete, testing | 2.0.0-beta.1 |
| `rc` | Release candidate | 2.0.0-rc.1 |

---

## Best Practices

1. **Manifest is truth** - Version flows from manifest.json
2. **Auto-sync** - Use MSBuild targets to sync versions
3. **Keep changelog** - Document all changes
4. **Handle migrations** - Support upgrade paths
5. **Lock AssemblyVersion** - Keep at 1.0.0.0 for binding stability

---

## Related Skills

- [build-configuration.md](build-configuration.md) - Build setup
- [packaging-deployment.md](packaging-deployment.md) - Creating packages

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added migration manager |
| 1.0.0 | 2025-06-01 | Initial version |
