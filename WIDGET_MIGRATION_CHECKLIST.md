# Widget Migration Checklist

## Lessons Learned from Image Viewer Migration

### Critical Requirements
1. **Serilog Version**: Use `3.1.1` (NOT 4.2.0) - must match 3SC app
2. **Serilog.Sinks.File Version**: Use `5.0.0` (NOT 6.0.0) - must match 3SC app
3. **CommunityToolkit.Mvvm**: Use `8.2.2`
4. **Build Method**: Use `dotnet publish` (NOT `dotnet build`) to include all dependencies
5. **Package All Files**: Copy entire publish folder to Community folder

### Project Setup (.csproj)
- TargetFramework: `net8.0-windows`
- OutputType: `WinExe` for Debug, `Library` for Release
- UseWPF: `true`
- UseWindowsForms: `false` (causes DragEventArgs conflicts)
- AllowUnsafeBlocks: `true`
- StartupObject: `Test{WidgetName}Widget.Program` for Debug mode

### Required Files
1. `{WidgetName}ViewModel.cs` - Full ViewModel with all logic
2. `{WidgetName}Window.xaml` - Complete UI with self-contained styles
3. `{WidgetName}Window.xaml.cs` - Code-behind with drag, lifecycle
4. `{WidgetName}WidgetFactory.cs` - IWidget and IWidgetFactory implementations
5. `TestLauncher.cs` - For Debug mode testing
6. `manifest.json` - Widget metadata
7. Helper classes (Converters, Behaviors as needed)

### IWidget Interface Requirements
Must implement ALL members:
- `string WidgetKey { get; }`
- `string DisplayName { get; }`
- `string Version { get; }`
- `bool HasOwnWindow { get; }` - return true
- `bool HasSettings { get; }`
- `Window? CreateWindow()`
- `UserControl GetView()` - throw NotImplementedException
- `void OnInitialize()`
- `void OnDispose()`
- `void ShowSettings()`

### ViewModel Requirements
- Add `OnInitialize()` method
- Add `OnDispose()` method (for cleanup, saving state)
- Use CommunityToolkit.Mvvm for ObservableObject, RelayCommand
- Handle all widget-specific logic

### XAML Requirements
- Use `StaticResource` instead of `DynamicResource`
- Include all color brushes as resources (11 colors from 3SC Dark theme)
- Include ContextMenu, MenuItem, Separator styles
- Use `Style="{StaticResource ResourceKey={x:Type ContextMenu}}"` for explicit references
- Implement drag with MouseLeftButtonDown on root border
- Use local namespace for custom converters/behaviors

### Build Script Pattern
```powershell
dotnet publish -c Release -o "bin\Release\net8.0-windows\publish"
Copy all files from publish folder to Community\{widget-key}\
```

### Solution Integration
- Add project to `widgets.sln`
- Build Contracts project in Debug AND Release modes
- Test in both standalone (Debug) and hosted (Release via 3SC)

### Color Brushes (3SC Dark Theme)
```xml
<SolidColorBrush x:Key="WidgetSurface" Color="#D0080810"/>
<SolidColorBrush x:Key="WidgetText" Color="#FFF1F5F9"/>
<SolidColorBrush x:Key="Accent" Color="#FF2DD4BF"/>
<SolidColorBrush x:Key="SurfaceElevated" Color="#FF16161F"/>
<SolidColorBrush x:Key="Border" Color="#FF2A2A3A"/>
<SolidColorBrush x:Key="SurfaceHover" Color="#FF1C1C28"/>
<SolidColorBrush x:Key="TextPrimary" Color="#FFF1F5F9"/>
<SolidColorBrush x:Key="TextSecondary" Color="#FF94A3B8"/>
<SolidColorBrush x:Key="TextTertiary" Color="#FF64748B"/>
<SolidColorBrush x:Key="CardBackground" Color="#FF101018"/>
<SolidColorBrush x:Key="CardBorder" Color="#FF2A2A3A"/>
<SolidColorBrush x:Key="WidgetBackground" Color="#FF0A0A0F"/>
```

## Migration Steps (Step-by-Step)

### Step 1: Create Project Structure
1. Create folder: `3SC.Widgets.{WidgetName}`
2. Create `.csproj` with correct settings
3. Add ProjectReference to Contracts
4. Add PackageReferences (correct versions!)

### Step 2: Migrate ViewModel
1. Copy ViewModel from 3SC project
2. Update namespace to `_3SC.Widgets.{WidgetName}`
3. Add `OnInitialize()` and `OnDispose()` methods
4. Verify all commands and properties

### Step 3: Create Helper Classes
1. Copy/create any custom Converters
2. Copy/create any custom Behaviors
3. Update namespaces

### Step 4: Create Window XAML
1. Start with Window declaration (700x500, borderless, transparent)
2. Add Resources section (colors, converters, styles)
3. Copy/adapt UI from source View.xaml
4. Replace DynamicResource with StaticResource
5. Update bindings and namespaces

### Step 5: Create Code-Behind
1. Initialize ViewModel in constructor
2. Implement drag with MouseLeftButtonDown
3. Add context menu event handlers
4. Add lifecycle event handlers

### Step 6: Create Factory
1. Implement IWidgetFactory
2. Implement IWidget with all members
3. Return new Window in CreateWindow()

### Step 7: Create Supporting Files
1. TestLauncher.cs for Debug mode
2. manifest.json with correct metadata

### Step 8: Build Script
1. Create/copy Build-And-Package-{WidgetName}.ps1
2. Use dotnet publish
3. Copy all files from publish folder

### Step 9: Build and Test
1. Build Contracts (Debug and Release)
2. Build widget (Release)
3. Run build script
4. Test in 3SC app

### Step 10: Add to Solution
1. Add project to widgets.sln
2. Add configuration entries
3. Verify in Visual Studio
