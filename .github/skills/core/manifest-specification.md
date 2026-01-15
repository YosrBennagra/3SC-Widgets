# Manifest Specification

> **Category:** Core | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

The `manifest.json` file is the identity card of your widget. It tells the 3SC host application everything it needs to know to discover, load, and display your widget.

## Prerequisites

- [widget-architecture.md](widget-architecture.md)

---

## Manifest Location

```
3SC.Widgets.{WidgetName}/
├── manifest.json          ← Must be in project root
├── 3SC.Widgets.{Name}.dll
└── ...
```

The manifest must be copied to output directory:

```xml
<!-- In .csproj -->
<ItemGroup>
  <None Update="manifest.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

## Full Manifest Schema

```json
{
  "$schema": "https://3sc.app/schemas/widget-manifest-v2.json",
  
  "packageId": "com.author.widget-key",
  "widgetKey": "widget-key",
  "displayName": "Widget Display Name",
  "description": "A detailed description of what the widget does.",
  "version": "1.0.0",
  
  "author": {
    "name": "Author Name",
    "email": "author@example.com",
    "url": "https://github.com/author"
  },
  
  "category": "general",
  "tags": ["tag1", "tag2", "tag3"],
  "icon": "icon.png",
  
  "entry": "3SC.Widgets.WidgetName.dll",
  "factoryType": "_3SC.Widgets.WidgetName.WidgetNameWidgetFactory",
  
  "minAppVersion": "2.0.0",
  "maxAppVersion": "3.0.0",
  
  "hasSettings": true,
  "permissions": ["filesystem", "network"],
  
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
  
  "previewImages": [
    "preview1.png",
    "preview2.png"
  ],
  
  "repository": "https://github.com/author/widget-repo",
  "license": "MIT",
  "homepage": "https://widget-homepage.com",
  
  "dependencies": {
    "3SC.Widgets.Contracts": "2.0.0"
  }
}
```

---

## Field Reference

### Required Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `widgetKey` | string | Unique identifier. Lowercase, kebab-case. | `"clock"`, `"notes"`, `"image-viewer"` |
| `displayName` | string | Human-readable name | `"Clock Widget"` |
| `version` | string | Semantic version | `"1.0.0"`, `"2.1.3"` |
| `entry` | string | Main DLL filename | `"3SC.Widgets.Clock.dll"` |

### Recommended Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `packageId` | string | Reverse domain identifier | `"com.3sc.clock"` |
| `description` | string | Widget description | `"Displays current time..."` |
| `author` | object | Author information | See below |
| `factoryType` | string | Fully qualified factory type | `"_3SC.Widgets.Clock.ClockWidgetFactory"` |
| `category` | string | Widget category | `"general"` |
| `hasSettings` | boolean | Has settings UI | `true` |
| `defaultSize` | object | Default dimensions | `{"width": 300, "height": 200}` |

### Optional Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `minAppVersion` | string | Minimum 3SC version | `"2.0.0"` |
| `maxAppVersion` | string | Maximum 3SC version | `"3.0.0"` |
| `tags` | string[] | Search tags | `["productivity", "time"]` |
| `icon` | string | Icon filename | `"icon.png"` |
| `permissions` | string[] | Required permissions | `["filesystem"]` |
| `minSize` | object | Minimum dimensions | `{"width": 100, "height": 100}` |
| `maxSize` | object | Maximum dimensions | `{"width": 800, "height": 600}` |
| `previewImages` | string[] | Store preview images | `["preview.png"]` |
| `repository` | string | Source code URL | `"https://github.com/..."` |
| `license` | string | License identifier | `"MIT"` |
| `homepage` | string | Widget homepage | `"https://..."` |
| `dependencies` | object | Required packages | `{"pkg": "1.0.0"}` |

---

## Author Object

```json
{
  "author": {
    "name": "John Doe",
    "email": "john@example.com",
    "url": "https://github.com/johndoe"
  }
}
```

Or simplified:

```json
{
  "author": "John Doe"
}
```

---

## Categories

| Category | Description |
|----------|-------------|
| `general` | General purpose widgets |
| `productivity` | Work and productivity tools |
| `system` | System information and utilities |
| `media` | Images, video, audio |
| `utilities` | Utility widgets |
| `communication` | Chat, email, notifications |
| `finance` | Financial and business |
| `weather` | Weather information |
| `sports` | Sports and fitness |
| `games` | Games and entertainment |
| `developer` | Developer tools |

---

## Permissions

| Permission | Description |
|------------|-------------|
| `filesystem` | Read/write files |
| `network` | Internet access |
| `clipboard` | Access clipboard |
| `notifications` | Show notifications |
| `camera` | Access camera |
| `microphone` | Access microphone |
| `location` | Access location |
| `startup` | Run at startup |

---

## Manifest Examples

### Minimal Manifest

```json
{
  "widgetKey": "simple-clock",
  "displayName": "Simple Clock",
  "version": "1.0.0",
  "entry": "3SC.Widgets.SimpleClock.dll"
}
```

### Standard Manifest

```json
{
  "packageId": "com.3sc.clock",
  "widgetKey": "clock",
  "displayName": "Clock Widget",
  "description": "Displays current time with customizable format and timezone support.",
  "version": "1.0.0",
  
  "author": {
    "name": "3SC Team",
    "email": "team@3sc.app",
    "url": "https://3sc.app"
  },
  
  "category": "general",
  "tags": ["clock", "time", "timezone"],
  "icon": "icon.png",
  
  "entry": "3SC.Widgets.Clock.dll",
  "factoryType": "_3SC.Widgets.Clock.ClockWidgetFactory",
  
  "minAppVersion": "2.0.0",
  "hasSettings": true,
  
  "defaultSize": {
    "width": 300,
    "height": 120
  }
}
```

### Full Manifest

```json
{
  "$schema": "https://3sc.app/schemas/widget-manifest-v2.json",
  
  "packageId": "com.3sc.image-viewer",
  "widgetKey": "image-viewer",
  "displayName": "Image Viewer",
  "description": "View images with zoom, pan, and slideshow capabilities. Supports PNG, JPEG, GIF, BMP, and WebP formats.",
  "version": "2.1.0",
  
  "author": {
    "name": "3SC Team",
    "email": "team@3sc.app",
    "url": "https://github.com/3sc-team"
  },
  
  "category": "media",
  "tags": ["image", "viewer", "photo", "gallery", "slideshow"],
  "icon": "Resources/icon.png",
  
  "entry": "3SC.Widgets.ImageViewer.dll",
  "factoryType": "_3SC.Widgets.ImageViewer.ImageViewerWidgetFactory",
  
  "minAppVersion": "2.0.0",
  "maxAppVersion": "3.0.0",
  
  "hasSettings": true,
  "permissions": ["filesystem"],
  
  "defaultSize": {
    "width": 400,
    "height": 300
  },
  "minSize": {
    "width": 200,
    "height": 150
  },
  "maxSize": {
    "width": 1920,
    "height": 1080
  },
  
  "previewImages": [
    "preview/light-mode.png",
    "preview/dark-mode.png",
    "preview/slideshow.png"
  ],
  
  "repository": "https://github.com/3sc/widgets",
  "license": "MIT",
  "homepage": "https://3sc.app/widgets/image-viewer",
  
  "dependencies": {
    "3SC.Widgets.Contracts": "2.0.0"
  }
}
```

---

## Validation Rules

### Widget Key Rules

```
✅ Valid:
  - clock
  - image-viewer
  - my-awesome-widget
  - notes2
  
❌ Invalid:
  - Clock          (uppercase)
  - image_viewer   (underscore)
  - My Widget      (spaces)
  - my--widget     (double dash)
  - -widget        (starts with dash)
  - widget-        (ends with dash)
```

**Regex Pattern:** `^[a-z][a-z0-9]*(-[a-z0-9]+)*$`

### Version Rules

Follow [Semantic Versioning](https://semver.org/):

```
✅ Valid:
  - 1.0.0
  - 2.1.3
  - 1.0.0-beta.1
  - 2.0.0-rc.1
  
❌ Invalid:
  - 1.0           (missing patch)
  - v1.0.0        (no 'v' prefix)
  - 1.0.0.0       (no fourth number)
```

### Entry Rules

```
✅ Valid:
  - 3SC.Widgets.Clock.dll
  - MyWidget.dll
  
❌ Invalid:
  - 3SC.Widgets.Clock.exe   (must be DLL)
  - ./path/to/dll           (no paths)
```

---

## Common Patterns

### Multi-Widget Package

One package containing multiple widgets:

```json
{
  "packageId": "com.author.widget-bundle",
  "widgets": [
    {
      "widgetKey": "clock-analog",
      "displayName": "Analog Clock",
      "entry": "3SC.Widgets.ClockBundle.dll",
      "factoryType": "_3SC.Widgets.ClockBundle.AnalogClockFactory"
    },
    {
      "widgetKey": "clock-digital",
      "displayName": "Digital Clock",
      "entry": "3SC.Widgets.ClockBundle.dll",
      "factoryType": "_3SC.Widgets.ClockBundle.DigitalClockFactory"
    }
  ]
}
```

### Development Manifest

For testing with different settings:

```json
{
  "widgetKey": "my-widget-dev",
  "displayName": "My Widget (DEV)",
  "version": "0.0.1-dev",
  "entry": "3SC.Widgets.MyWidget.dll",
  
  "_devMode": true,
  "_debugLogging": true
}
```

---

## Schema Validation

Use JSON Schema for IDE support:

```json
{
  "$schema": "https://3sc.app/schemas/widget-manifest-v2.json",
  "widgetKey": "my-widget"
}
```

Or local schema:

```json
{
  "$schema": "./schemas/widget-manifest.schema.json",
  "widgetKey": "my-widget"
}
```

---

## Programmatic Access

Reading manifest in your widget:

```csharp
public static class ManifestReader
{
    public static WidgetManifest? ReadManifest()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation);
        var manifestPath = Path.Combine(directory!, "manifest.json");
        
        if (!File.Exists(manifestPath))
            return null;
            
        var json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<WidgetManifest>(json);
    }
}

public class WidgetManifest
{
    [JsonPropertyName("widgetKey")]
    public string WidgetKey { get; set; } = "";
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
    
    [JsonPropertyName("hasSettings")]
    public bool HasSettings { get; set; }
    
    [JsonPropertyName("defaultSize")]
    public SizeInfo? DefaultSize { get; set; }
}

public class SizeInfo
{
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
}
```

---

## Common Mistakes

### ❌ Missing Required Fields

```json
{
  "displayName": "My Widget"
}
// Missing: widgetKey, version, entry
```

### ❌ Inconsistent Widget Key

```json
{
  "widgetKey": "my-widget"
}
```

```csharp
// In code - DIFFERENT!
public string WidgetKey => "myWidget";
```

### ❌ Wrong Entry Format

```json
{
  "entry": "MyWidget"
}
// Should be: "MyWidget.dll"
```

### ❌ Invalid Version

```json
{
  "version": "1.0"
}
// Should be: "1.0.0"
```

---

## Related Skills

- [widget-architecture.md](widget-architecture.md) - Widget structure
- [contracts-interfaces.md](contracts-interfaces.md) - Interfaces
- [project-setup.md](project-setup.md) - Project configuration
- [versioning.md](../packaging/versioning.md) - Version management

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added full schema, validation rules |
| 1.0.0 | 2025-06-01 | Initial version |
