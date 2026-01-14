# Widget Contracts Reference

## IWidgetFactory

Entry point for widget discovery. The 3SC app searches loaded assemblies for types implementing this interface.

```csharp
namespace _3SC.Widgets.Contracts;

public interface IWidgetFactory
{
    IWidget CreateWidget();
}
```

**Implementation:**
```csharp
public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ClockWidget();
    }
}
```

## IWidget

Core widget interface. Widgets can provide either their own Window or a UserControl.

```csharp
namespace _3SC.Widgets.Contracts;

public interface IWidget
{
    // Identity
    string WidgetKey { get; }
    string DisplayName { get; }
    string Version { get; }
    
    // Display Mode
    bool HasOwnWindow { get; }
    Window? CreateWindow();      // Called if HasOwnWindow = true
    UserControl GetView();       // Called if HasOwnWindow = false
    
    // Lifecycle
    void OnInitialize();
    void OnDispose();
    
    // Settings
    bool HasSettings { get; }
    void ShowSettings();
}
```

## Widget Key Matching

The `WidgetKey` property must match the `widgetKey` in manifest.json:

```json
{
  "widgetKey": "clock",
  ...
}
```

```csharp
public string WidgetKey => "clock";  // Must match
```

## Own Window vs Hosted

### Own Window (HasOwnWindow = true)
```csharp
public bool HasOwnWindow => true;

public Window? CreateWindow()
{
    return new MyWidgetWindow();  // Your custom Window
}

public UserControl GetView()
{
    throw new NotSupportedException("This widget provides its own window.");
}
```

**Use when:**
- Widget needs custom drag behavior
- Widget has context menus
- Widget has complex window features
- Porting existing standalone window

### Hosted Mode (HasOwnWindow = false)
```csharp
public bool HasOwnWindow => false;

public Window? CreateWindow()
{
    return null;
}

public UserControl GetView()
{
    return new MyWidgetView();  // UserControl hosted by 3SC
}
```

**Use when:**
- Simple widget with basic UI
- No custom window features needed
- Widget is purely content-focused

## Lifecycle Methods

### OnInitialize()
Called after the widget is created and before it's displayed.

```csharp
public void OnInitialize()
{
    // Setup timers, load data, initialize state
    _timer.Start();
    LoadSettings();
}
```

### OnDispose()
Called when the widget is being closed/removed.

```csharp
public void OnDispose()
{
    // Cleanup resources, stop timers, save state
    _timer?.Stop();
    SaveSettings();
    _window = null;
}
```

## Settings Support

```csharp
public bool HasSettings => true;

public void ShowSettings()
{
    var settingsWindow = new WidgetSettingsWindow(_currentSettings);
    if (settingsWindow.ShowDialog() == true)
    {
        ApplySettings(settingsWindow.UpdatedSettings);
    }
}
```

For own-window widgets, settings are typically accessed via the window's context menu, so `ShowSettings()` can be empty if the window handles this internally.
