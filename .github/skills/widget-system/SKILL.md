# Widget System

## Overview
The 3SC widget system allows external plugins to be loaded dynamically from the Community folder. Widgets can either provide their own complete Window with full functionality (drag, context menus, settings) or be hosted in a CommunityWidgetWindow as a UserControl.

## Key Concepts

### Widget Loading Architecture
- Widgets are packaged as DLLs with a `manifest.json` file
- Located in: `%APPDATA%\3SC\Widgets\Community\{widget-key}/`
- Discovered by `WidgetLibraryViewModel` which scans the Community folder
- Loaded via reflection by `WidgetWindowService`

### Widget Modes

**1. Own Window Mode (Recommended for complex widgets)**
```csharp
public class ClockWidget : IWidget
{
    public bool HasOwnWindow => true;
    
    public Window? CreateWindow()
    {
        return new ClockWidgetWindow(); // Full featured window
    }
}
```
- Widget provides its own WPF Window
- Complete control over appearance, behavior, drag, menus
- Original functionality preserved from standalone implementation

**2. Hosted Mode (Simple widgets)**
```csharp
public class SimpleWidget : IWidget
{
    public bool HasOwnWindow => false;
    
    public UserControl GetView()
    {
        return new SimpleWidgetView(); // UserControl hosted in CommunityWidgetWindow
    }
}
```

## Required Files

### manifest.json
```json
{
  "packageId": "com.3sc.clock",
  "widgetKey": "clock",
  "displayName": "Clock",
  "version": "1.0.0",
  "author": {
    "name": "3SC Team",
    "email": "team@3sc.app"
  },
  "description": "Displays current time with timezone support",
  "category": "general",
  "icon": "icon.png",
  "entry": "3SC.Widgets.Clock.dll",
  "minAppVersion": "1.0.0",
  "hasSettings": true,
  "permissions": [],
  "tags": ["clock", "time", "timezone"],
  "defaultSize": {
    "width": 300,
    "height": 80
  }
}
```

**Critical:** `entry` must be the DLL filename (e.g., `"3SC.Widgets.Clock.dll"`), NOT a type name.

## Implementation Pattern

### 1. Widget Factory
```csharp
using _3SC.Widgets.Contracts;

public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ClockWidget();
    }
}
```

### 2. Widget Implementation
```csharp
public class ClockWidget : IWidget
{
    private ClockWidgetWindow? _window;

    public string WidgetKey => "clock";
    public string DisplayName => "Clock";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new ClockWidgetWindow();
        return _window;
    }

    public UserControl GetView()
    {
        throw new NotSupportedException("This widget provides its own window.");
    }

    public void OnInitialize()
    {
        // Window handles its own initialization
    }

    public void OnDispose()
    {
        _window = null;
    }

    public void ShowSettings()
    {
        // Settings accessed via window's context menu
    }
}
```

### 3. Widget Window (Full Featured)
```csharp
public partial class ClockWidgetWindow : Window
{
    private readonly DispatcherTimer _timer;
    
    public ClockWidgetWindow()
    {
        InitializeComponent();
        
        // Setup timer, drag support, context menu, etc.
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
        
        Loaded += OnLoaded;
    }
    
    // Full drag, context menu, settings implementation...
}
```

## Project Structure

### Widget Project (.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType Condition="'$(Configuration)' == 'Debug'">WinExe</OutputType>
    <OutputType Condition="'$(Configuration)' != 'Debug'">Library</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\3SC\3SC.Widgets.Contracts\3SC.Widgets.Contracts.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="manifest.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

## Loading Flow

1. **Widget Library Scan**
   - `WidgetLibraryViewModel.LoadWidgetsAsync()` scans Community folder
   - Reads each widget's `manifest.json`
   - Displays in Widget Library UI

2. **Widget Activation**
   - User clicks "Add Widget" button
   - `WidgetWindowService.ShowCommunityWidget()` called
   - Loads assembly via `Assembly.LoadFrom(dllPath)`

3. **Factory Discovery**
   - Searches for types implementing `IWidgetFactory`
   - Creates factory instance via `Activator.CreateInstance()`
   - Calls `factory.CreateWidget()`

4. **Display Logic**
   ```csharp
   if (widget.HasOwnWindow)
   {
       var window = widget.CreateWindow();
       window.Show(); // Native window with all features
   }
   else
   {
       // Wrap in CommunityWidgetWindow
       var container = new CommunityWidgetWindow(...);
       container.Show();
   }
   ```

## Best Practices

1. **Use HasOwnWindow for complex widgets**
   - Preserves original functionality
   - Full control over drag, menus, styling
   - No wrapper overhead

2. **Manifest entry field**
   - Always use DLL filename: `"MyWidget.dll"`
   - Never use type name: `"_3SC.Widgets.MyWidget.Plugin"`

3. **Testing**
   - Build in Release mode: `dotnet build -c Release`
   - Copy to: `%APPDATA%\3SC\Widgets\Community\{widget-key}/`
   - Include: DLL, manifest.json, deps.json, contracts DLL

4. **Dependencies**
   - Include `3SC.Widgets.Contracts.dll` in package
   - Reference project, not copy local DLL

## Common Pitfalls

❌ **Wrong manifest entry**
```json
"entry": "_3SC.Widgets.Clock.ClockWidgetPlugin"  // DON'T
```

✅ **Correct manifest entry**
```json
"entry": "3SC.Widgets.Clock.dll"  // DO
```

❌ **Missing project reference**
- Widget won't compile without contracts reference

❌ **Using UserControl for complex widgets**
- Loses drag functionality, context menus, custom window features

## References
- [Widget Contracts](references/widget-contracts.md)
- [Widget Window Implementation](references/widget-window.md)
- [Widget Packaging](references/widget-packaging.md)
