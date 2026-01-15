# Widget Architecture

> **Category:** Core | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers the fundamental architecture of 3SC widgets, including structure, lifecycle, discovery mechanisms, and integration with the host application.

## Prerequisites

None - This is a foundational skill.

---

## Core Concepts

### What is a Widget?

A widget is a self-contained, interactive UI component that runs on the Windows desktop. In 3SC:

- **External Widgets** - Loaded dynamically from DLLs (this repository)
- **Built-in Widgets** - Compiled into the main application

### Widget Structure

Every widget project follows this structure:

```
3SC.Widgets.{WidgetName}/
├── 3SC.Widgets.{WidgetName}.csproj    # Project configuration
├── manifest.json                       # Widget metadata (REQUIRED)
├── {WidgetName}WidgetFactory.cs       # Factory + Widget implementation
├── {WidgetName}ViewModel.cs           # Business logic (MVVM)
├── {WidgetName}Window.xaml            # Widget UI (Window-based)
├── {WidgetName}Window.xaml.cs         # Code-behind with drag support
├── TestLauncher.cs                    # Standalone testing (Debug only)
├── ValueObjects/                      # Settings & data classes
│   └── {WidgetName}Settings.cs
├── Converters/                        # Value converters
├── Behaviors/                         # Attached behaviors
└── Resources/                         # Images, icons, assets
```

### Component Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                    3SC Host Application                      │
│  ┌────────────────────────────────────────────────────────┐│
│  │              ExternalWidgetLoader                       ││
│  │  1. Scans %APPDATA%\3SC\Widgets\Community\             ││
│  │  2. Reads manifest.json from each folder               ││
│  │  3. Loads DLL via AssemblyLoadContext                  ││
│  │  4. Finds IWidgetFactory implementation                ││
│  │  5. Creates widget instance                            ││
│  └────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Your Widget                              │
│  ┌──────────────┐     ┌──────────────┐     ┌─────────────┐ │
│  │WidgetFactory │────▶│    Widget    │────▶│   Window    │ │
│  │(Entry Point) │     │  (IWidget)   │     │   (XAML)    │ │
│  └──────────────┘     └──────────────┘     └─────────────┘ │
│                              │                    │         │
│                              ▼                    ▼         │
│                       ┌──────────────┐     ┌───────────┐   │
│                       │  ViewModel   │◀───▶│  Settings │   │
│                       │   (MVVM)     │     │   (JSON)  │   │
│                       └──────────────┘     └───────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## Widget Lifecycle

### 1. Discovery Phase

```csharp
// Host scans this path for widget folders
string communityPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "3SC", "Widgets", "Community"
);

// Each subfolder should contain:
// - manifest.json
// - 3SC.Widgets.{Name}.dll
// - 3SC.Widgets.Contracts.dll
// - Other dependencies
```

### 2. Loading Phase

```csharp
// Host reads manifest.json
var manifest = JsonSerializer.Deserialize<WidgetManifest>(File.ReadAllText("manifest.json"));

// Host loads the assembly
var assembly = Assembly.LoadFrom(manifest.Entry);

// Host finds IWidgetFactory
var factoryType = assembly.GetTypes()
    .FirstOrDefault(t => typeof(IWidgetFactory).IsAssignableFrom(t));

// Host creates factory instance
var factory = (IWidgetFactory)Activator.CreateInstance(factoryType);
```

### 3. Creation Phase

```csharp
// Host creates widget via factory
IWidget widget = factory.CreateWidget();

// Host calls initialization
widget.OnInitialize();

// Host creates and shows window
if (widget.HasOwnWindow)
{
    Window window = widget.CreateWindow();
    window.Show();
}
else
{
    UserControl view = widget.GetView();
    // Host wraps in container window
}
```

### 4. Runtime Phase

```
┌─────────────────────────────────────────┐
│              Widget Active               │
│                                          │
│  • Responds to user interaction          │
│  • Updates UI via data binding           │
│  • Saves settings automatically          │
│  • Can be dragged, resized, locked       │
│                                          │
│  Events:                                 │
│  ├── Position changed → Save to DB       │
│  ├── Size changed → Save to DB           │
│  ├── Settings changed → Save JSON        │
│  └── Context menu → Show options         │
└─────────────────────────────────────────┘
```

### 5. Disposal Phase

```csharp
// When widget is closed or app shuts down
widget.OnDispose();

// Your cleanup code should:
// - Stop timers
// - Cancel async operations
// - Save final state
// - Dispose resources
// - Unsubscribe from events
```

### Complete Lifecycle Diagram

```
    ┌──────────┐
    │ manifest │
    │  .json   │
    └────┬─────┘
         │ Discovery
         ▼
    ┌──────────┐
    │  Load    │
    │   DLL    │
    └────┬─────┘
         │ Reflection
         ▼
    ┌──────────┐
    │  Create  │
    │ Factory  │
    └────┬─────┘
         │ IWidgetFactory.CreateWidget()
         ▼
    ┌──────────┐
    │  Create  │
    │  Widget  │
    └────┬─────┘
         │ IWidget.OnInitialize()
         ▼
    ┌──────────┐
    │Initialize│──────────────┐
    │ Settings │              │
    └────┬─────┘              │
         │ IWidget.CreateWindow()
         ▼                    │
    ┌──────────┐              │ User
    │  Show    │              │ Interaction
    │  Window  │◀─────────────┘
    └────┬─────┘
         │ Window.Closed / App.Exit
         ▼
    ┌──────────┐
    │  Dispose │
    │  Widget  │
    └──────────┘
```

---

## Widget Types

### Window-Based Widget (Recommended)

Most flexible approach - widget manages its own window.

```csharp
public class MyWidget : IWidget
{
    public bool HasOwnWindow => true;
    
    public Window? CreateWindow()
    {
        return new MyWidgetWindow();
    }
    
    public UserControl GetView()
    {
        throw new NotSupportedException("Use CreateWindow instead");
    }
}
```

### View-Based Widget

Widget provides only a UserControl - host wraps it.

```csharp
public class MyWidget : IWidget
{
    public bool HasOwnWindow => false;
    
    public Window? CreateWindow()
    {
        return null;
    }
    
    public UserControl GetView()
    {
        return new MyWidgetView();
    }
}
```

### External Widget (Alternative Interface)

Simplified interface for external widgets.

```csharp
public class MyExternalWidget : IExternalWidget
{
    public string WidgetKey => "my-widget";
    public string DisplayName => "My Widget";
    public string Category => "General";
    public string Description => "Widget description";
    public string Version => "1.0.0";
    
    public object CreateWidgetWindow(Guid widgetInstanceId, string? settingsJson)
    {
        var settings = DeserializeSettings(settingsJson);
        return new MyWidgetWindow(widgetInstanceId, settings);
    }
}
```

---

## Discovery Paths

### Community Widgets (Primary)

```
%APPDATA%\3SC\Widgets\Community\{widget-key}\
├── manifest.json
├── 3SC.Widgets.{Name}.dll
├── 3SC.Widgets.Contracts.dll
└── dependencies...
```

### Custom Widgets (User-installed)

```
%APPDATA%\3SC\Widgets\Custom\{widget-key}\
```

### Built-in Widgets

Compiled into `3SC.exe`, not discoverable externally.

---

## Best Practices

### 1. Single Responsibility

Each widget should do ONE thing well:
- ✅ Clock widget shows time
- ✅ Notes widget manages notes
- ❌ "Super widget" that does everything

### 2. Stateless Factory

Factory should not hold state:

```csharp
// ✅ Good - factory is stateless
public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget() => new ClockWidget();
}

// ❌ Bad - factory holds state
public class ClockWidgetFactory : IWidgetFactory
{
    private ClockWidget _instance; // Don't do this!
    
    public IWidget CreateWidget()
    {
        _instance ??= new ClockWidget();
        return _instance;
    }
}
```

### 3. Clean Initialization

```csharp
public void OnInitialize()
{
    // 1. Load settings first
    LoadSettings();
    
    // 2. Initialize services
    InitializeServices();
    
    // 3. Start background tasks last
    StartBackgroundTasks();
}
```

### 4. Proper Disposal

```csharp
public void OnDispose()
{
    // 1. Stop background tasks first
    _timer?.Stop();
    _cts?.Cancel();
    
    // 2. Save state
    SaveSettings();
    
    // 3. Dispose resources
    _timer?.Dispose();
    _cts?.Dispose();
    
    // 4. Clear references
    _timer = null;
}
```

### 5. Consistent Widget Key

The `widgetKey` must be identical everywhere:

```json
// manifest.json
{
    "widgetKey": "my-widget"
}
```

```csharp
// Widget class
public string WidgetKey => "my-widget";
```

```
// Folder name
%APPDATA%\3SC\Widgets\Community\my-widget\
```

---

## Common Mistakes

### ❌ Blocking OnInitialize

```csharp
// Bad - blocks UI thread
public void OnInitialize()
{
    var data = HttpClient.GetAsync(url).Result; // Blocks!
}

// Good - use async pattern
public void OnInitialize()
{
    _ = LoadDataAsync(); // Fire and forget
}

private async Task LoadDataAsync()
{
    var data = await HttpClient.GetAsync(url);
    // Update UI via dispatcher
}
```

### ❌ Missing Null Checks

```csharp
// Bad - crashes if settings file missing
var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(path));
settings.Property = value; // NullReferenceException!

// Good - handle missing file
var settings = File.Exists(path) 
    ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(path)) 
    : new Settings();
```

### ❌ Hardcoded Paths

```csharp
// Bad - won't work on other machines
var path = @"C:\Users\John\AppData\3SC\...";

// Good - use environment
var path = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "3SC", "Widgets", widgetKey
);
```

---

## Related Skills

- [contracts-interfaces.md](contracts-interfaces.md) - Interface details
- [manifest-specification.md](manifest-specification.md) - Manifest format
- [project-setup.md](project-setup.md) - Project configuration

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Complete rewrite with lifecycle diagrams |
| 1.0.0 | 2025-06-01 | Initial version |
