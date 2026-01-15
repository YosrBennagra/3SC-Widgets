# Contracts & Interfaces

> **Category:** Core | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers all interfaces and contracts that widgets must implement to integrate with the 3SC host application.

## Prerequisites

- [widget-architecture.md](widget-architecture.md)

---

## Core Interfaces

### IWidgetFactory

The entry point for widget creation. The host discovers and instantiates this interface.

```csharp
namespace _3SC.Widgets.Contracts;

/// <summary>
/// Factory interface for creating widget instances.
/// This is the entry point that the host application discovers.
/// </summary>
public interface IWidgetFactory
{
    /// <summary>
    /// Creates a new instance of the widget.
    /// </summary>
    /// <returns>A new IWidget instance</returns>
    IWidget CreateWidget();
}
```

#### Implementation Example

```csharp
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Clock;

public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ClockWidget();
    }
}
```

#### Key Requirements

| Requirement | Description |
|-------------|-------------|
| Parameterless Constructor | Factory must have a public parameterless constructor |
| Stateless | Factory should not maintain state between calls |
| Thread-Safe | Factory may be called from different threads |
| Fast Creation | CreateWidget() should return quickly |

---

### IWidget

The main widget interface defining widget behavior and capabilities.

```csharp
namespace _3SC.Widgets.Contracts;

/// <summary>
/// Core widget interface that all widgets must implement.
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Unique identifier for this widget type.
    /// Must match the widgetKey in manifest.json.
    /// Format: lowercase, kebab-case (e.g., "my-widget")
    /// </summary>
    string WidgetKey { get; }
    
    /// <summary>
    /// Human-readable display name shown in UI.
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Widget version in semantic versioning format.
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Whether this widget manages its own Window.
    /// true = CreateWindow() is used
    /// false = GetView() is used, host wraps in container
    /// </summary>
    bool HasOwnWindow { get; }
    
    /// <summary>
    /// Whether this widget has a settings panel.
    /// </summary>
    bool HasSettings { get; }
    
    /// <summary>
    /// Creates the widget's window. Called when HasOwnWindow is true.
    /// </summary>
    /// <returns>The widget window, or null if HasOwnWindow is false</returns>
    Window? CreateWindow();
    
    /// <summary>
    /// Gets the widget's view. Called when HasOwnWindow is false.
    /// </summary>
    /// <returns>The widget UserControl</returns>
    UserControl GetView();
    
    /// <summary>
    /// Called after creation to initialize the widget.
    /// Load settings, start services, etc.
    /// </summary>
    void OnInitialize();
    
    /// <summary>
    /// Called before destruction to clean up resources.
    /// Stop timers, save state, dispose resources.
    /// </summary>
    void OnDispose();
}
```

#### Complete Implementation Example

```csharp
using System.Windows;
using System.Windows.Controls;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Clock;

public class ClockWidget : IWidget
{
    private ClockWidgetWindow? _window;
    private ClockWidgetViewModel? _viewModel;
    
    // Identity
    public string WidgetKey => "clock";
    public string DisplayName => "Clock";
    public string Version => "1.0.0";
    
    // Capabilities
    public bool HasOwnWindow => true;
    public bool HasSettings => true;
    
    public Window? CreateWindow()
    {
        _viewModel = new ClockWidgetViewModel();
        _window = new ClockWidgetWindow
        {
            DataContext = _viewModel
        };
        return _window;
    }
    
    public UserControl GetView()
    {
        // Not used when HasOwnWindow is true
        throw new NotSupportedException(
            "This widget uses its own window. Use CreateWindow() instead.");
    }
    
    public void OnInitialize()
    {
        // Load settings
        _viewModel?.LoadSettings();
        
        // Start the clock
        _viewModel?.Start();
    }
    
    public void OnDispose()
    {
        // Stop the clock
        _viewModel?.Stop();
        
        // Save settings
        _viewModel?.SaveSettings();
        
        // Clean up
        _viewModel?.Dispose();
        _viewModel = null;
        _window = null;
    }
}
```

---

### IExternalWidget

Alternative simplified interface for external widgets.

```csharp
namespace _3SC.Widgets.Contracts;

/// <summary>
/// Simplified interface for external community widgets.
/// Provides instance ID and settings directly.
/// </summary>
public interface IExternalWidget
{
    /// <summary>
    /// Unique widget type identifier.
    /// </summary>
    string WidgetKey { get; }
    
    /// <summary>
    /// Display name for UI.
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Widget category (General, Productivity, System, Media, etc.)
    /// </summary>
    string Category { get; }
    
    /// <summary>
    /// Widget description for store/listing.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Widget version string.
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Creates the widget window with instance context.
    /// </summary>
    /// <param name="widgetInstanceId">Unique ID for this widget instance</param>
    /// <param name="settingsJson">Serialized settings, or null for defaults</param>
    /// <returns>The widget Window object</returns>
    object CreateWidgetWindow(Guid widgetInstanceId, string? settingsJson);
}
```

#### Implementation Example

```csharp
using System.Text.Json;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Notes;

public class NotesWidget : IExternalWidget
{
    public string WidgetKey => "notes";
    public string DisplayName => "Notes";
    public string Category => "Productivity";
    public string Description => "A simple note-taking widget";
    public string Version => "1.0.0";
    
    public object CreateWidgetWindow(Guid widgetInstanceId, string? settingsJson)
    {
        // Deserialize settings
        var settings = string.IsNullOrEmpty(settingsJson)
            ? NotesSettings.Default()
            : JsonSerializer.Deserialize<NotesSettings>(settingsJson);
        
        // Create view model with context
        var viewModel = new NotesViewModel(widgetInstanceId, settings);
        
        // Create and return window
        return new NotesWindow
        {
            DataContext = viewModel
        };
    }
}
```

---

## Interface Comparison

| Feature | IWidget | IExternalWidget |
|---------|---------|-----------------|
| Factory required | Yes (IWidgetFactory) | No |
| Instance ID | Not provided | Provided |
| Settings | Must load yourself | Provided as JSON |
| Lifecycle methods | OnInitialize, OnDispose | Handle in constructor |
| Window/View choice | Yes (HasOwnWindow) | Always Window |
| Recommended for | Complex widgets | Simple widgets |

---

## Combined Factory + Widget Pattern

For cleaner code, combine factory and widget in one class:

```csharp
using System.Windows;
using System.Windows.Controls;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Clock;

/// <summary>
/// Combined factory and widget implementation.
/// Cleaner for simple widgets.
/// </summary>
public class ClockWidgetFactory : IWidgetFactory, IWidget
{
    private ClockWidgetWindow? _window;
    private ClockWidgetViewModel? _viewModel;
    
    // IWidgetFactory
    public IWidget CreateWidget() => new ClockWidgetFactory();
    
    // IWidget - Identity
    public string WidgetKey => "clock";
    public string DisplayName => "Clock Widget";
    public string Version => "1.0.0";
    
    // IWidget - Capabilities
    public bool HasOwnWindow => true;
    public bool HasSettings => true;
    
    // IWidget - Methods
    public Window? CreateWindow()
    {
        _viewModel = new ClockWidgetViewModel();
        _window = new ClockWidgetWindow { DataContext = _viewModel };
        return _window;
    }
    
    public UserControl GetView() => 
        throw new NotSupportedException();
    
    public void OnInitialize()
    {
        _viewModel?.LoadSettings();
        _viewModel?.Start();
    }
    
    public void OnDispose()
    {
        _viewModel?.Stop();
        _viewModel?.SaveSettings();
        _viewModel = null;
        _window = null;
    }
}
```

---

## Settings Interface Pattern

While not required, this pattern helps with settings:

```csharp
/// <summary>
/// Optional interface for widgets with settings.
/// </summary>
public interface IWidgetWithSettings
{
    /// <summary>
    /// Gets the settings view for the settings panel.
    /// </summary>
    UserControl GetSettingsView();
    
    /// <summary>
    /// Called when settings should be saved.
    /// </summary>
    void SaveSettings();
    
    /// <summary>
    /// Called when settings should be reset to defaults.
    /// </summary>
    void ResetSettings();
}
```

---

## Type Registration

The host discovers widgets by:

1. **Reading manifest.json** for `factoryType`
2. **Scanning assembly** for `IWidgetFactory` implementations

```json
{
    "widgetKey": "clock",
    "factoryType": "_3SC.Widgets.Clock.ClockWidgetFactory"
}
```

```csharp
// Host code (simplified)
if (!string.IsNullOrEmpty(manifest.FactoryType))
{
    // Use explicit type from manifest
    var type = assembly.GetType(manifest.FactoryType);
    factory = (IWidgetFactory)Activator.CreateInstance(type);
}
else
{
    // Scan for IWidgetFactory
    var type = assembly.GetTypes()
        .FirstOrDefault(t => typeof(IWidgetFactory).IsAssignableFrom(t) 
                          && !t.IsInterface 
                          && !t.IsAbstract);
    factory = (IWidgetFactory)Activator.CreateInstance(type);
}
```

---

## Contract Assembly Reference

Your widget must reference `3SC.Widgets.Contracts.dll`:

```xml
<!-- In .csproj -->
<ItemGroup>
  <!-- Option 1: Direct reference (recommended for external widgets) -->
  <Reference Include="3SC.Widgets.Contracts">
    <HintPath>..\packages\3SC.Widgets.Contracts.dll</HintPath>
  </Reference>
  
  <!-- Option 2: Project reference (for in-solution development) -->
  <ProjectReference Include="..\..\3SC\3SC.Widgets.Contracts\3SC.Widgets.Contracts.csproj" />
</ItemGroup>
```

---

## Best Practices

### 1. Explicit Interface Implementation

Use explicit implementation for clean public API:

```csharp
public class ClockWidget : IWidget
{
    // Public API
    public string WidgetKey => "clock";
    
    // Explicit for interface-only
    Window? IWidget.CreateWindow() => CreateClockWindow();
    
    // Internal implementation
    private ClockWidgetWindow CreateClockWindow()
    {
        // Implementation
    }
}
```

### 2. Null Safety

Always handle null in interface methods:

```csharp
public Window? CreateWindow()
{
    try
    {
        return new MyWindow();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to create window");
        return null; // Host handles null gracefully
    }
}
```

### 3. Version Consistency

Keep version in sync:

```csharp
public class ClockWidget : IWidget
{
    // Use assembly version for consistency
    public string Version => 
        GetType().Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
}
```

---

## Common Mistakes

### ❌ Not Implementing Required Members

```csharp
// Error: Missing required members
public class BadWidget : IWidget
{
    public string WidgetKey => "bad";
    // Missing: DisplayName, Version, HasOwnWindow, HasSettings,
    //          CreateWindow, GetView, OnInitialize, OnDispose
}
```

### ❌ Throwing from OnInitialize/OnDispose

```csharp
// Bad - exceptions here crash the host
public void OnInitialize()
{
    throw new Exception("Something went wrong");
}

// Good - catch and log
public void OnInitialize()
{
    try
    {
        DoInitialization();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Initialization failed");
    }
}
```

### ❌ Mismatched Widget Key

```csharp
// manifest.json says "my-widget"
// Code says something different - BROKEN!
public string WidgetKey => "mywidget"; // Wrong!
public string WidgetKey => "my-widget"; // Correct!
```

---

## Related Skills

- [widget-architecture.md](widget-architecture.md) - Widget structure
- [manifest-specification.md](manifest-specification.md) - Manifest format
- [project-setup.md](project-setup.md) - Project configuration

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added IExternalWidget, combined pattern |
| 1.0.0 | 2025-06-01 | Initial version |
