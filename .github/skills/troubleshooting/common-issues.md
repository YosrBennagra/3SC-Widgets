# Troubleshooting Guide

> **Category:** Troubleshooting | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers common issues encountered during widget development and their solutions.

---

## Quick Diagnostics

```
┌─────────────────────────────────────────────────────────────┐
│                  Troubleshooting Checklist                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ □ Widget not loading?      → Check assembly binding          │
│ □ Styles not applying?     → Check StaticResource usage      │
│ □ Settings not saving?     → Check file path permissions     │
│ □ Build errors?            → Check NuGet versions            │
│ □ Host crashes?            → Check Serilog version           │
│ □ Memory leak?             → Check event subscriptions       │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Common Issues

### 1. Widget Not Loading

**Symptoms:**
- Widget doesn't appear in host
- "Could not load widget" error
- Assembly binding exceptions

**Solutions:**

```xml
<!-- Check target framework matches -->
<TargetFramework>net8.0-windows</TargetFramework>

<!-- Ensure UseWPF is enabled -->
<UseWPF>true</UseWPF>

<!-- Check for correct output type -->
<OutputType>Library</OutputType>
```

```csharp
// Verify factory returns valid instance
public class MyWidgetFactory : IWidgetFactory
{
    public IWidget Create()
    {
        return new MyWidget(); // Must not return null
    }
}
```

---

### 2. ⚠️ Serilog Version Conflict (CRITICAL)

**Symptoms:**
- `FileLoadException` or `TypeLoadException`
- "Could not load file or assembly 'Serilog'"
- Host crashes when widget loads

**Solution:**

```xml
<!-- ⚠️ MUST use version 3.1.1, NOT 4.x -->
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

**Why:** The 3SC host uses Serilog 3.1.1. Using Serilog 4.x causes binding conflicts.

---

### 3. ⚠️ CommunityToolkit.Mvvm Version Conflict

**Symptoms:**
- `MissingMethodException`
- ObservableProperty not generating
- Commands not working

**Solution:**

```xml
<!-- ⚠️ MUST use version 8.2.2, NOT 8.3.x+ -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

---

### 4. Styles Not Applying

**Symptoms:**
- Widget appears unstyled
- Colors different from theme
- Resources not found

**Solution:**

```xml
<!-- ❌ WRONG for external widgets -->
<TextBlock Foreground="{DynamicResource TextBrush}"/>

<!-- ✅ CORRECT - Use StaticResource -->
<TextBlock Foreground="{StaticResource TextBrush}"/>
```

**Why:** External widgets can't access host's dynamic resources.

**Embed resources in widget:**

```xml
<UserControl.Resources>
    <SolidColorBrush x:Key="TextBrush" Color="#E8E8E8"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="#1A1A2E"/>
</UserControl.Resources>
```

---

### 5. Settings Not Saving

**Symptoms:**
- Settings reset on restart
- File access denied
- Settings corrupted

**Debugging:**

```csharp
public void SaveSettings(Settings settings)
{
    var path = GetSettingsPath();
    _log.Debug("Saving settings to: {Path}", path);
    
    // Check directory exists
    var dir = Path.GetDirectoryName(path);
    if (!Directory.Exists(dir))
    {
        _log.Information("Creating settings directory: {Dir}", dir);
        Directory.CreateDirectory(dir!);
    }
    
    // Check write permissions
    try
    {
        File.WriteAllText(path, JsonSerializer.Serialize(settings));
        _log.Information("Settings saved successfully");
    }
    catch (UnauthorizedAccessException ex)
    {
        _log.Error(ex, "Access denied to settings file");
        throw;
    }
}
```

**Check path:**

```csharp
// Settings should be in:
// %APPDATA%\3SC\WidgetData\{widget-key}\settings.json
var correctPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "3SC", "WidgetData", "my-widget", "settings.json");
```

---

### 6. Memory Leaks

**Symptoms:**
- Memory grows over time
- App becomes slow
- OutOfMemoryException

**Common Causes:**

```csharp
// ❌ Event handler leak
public class LeakyWidget : IExternalWidget
{
    public void OnInitialize()
    {
        _timer.Tick += OnTick; // Never unsubscribed!
    }
}

// ✅ Fixed
public class FixedWidget : IExternalWidget
{
    public void OnInitialize()
    {
        _timer.Tick += OnTick;
    }
    
    public void OnClose()
    {
        _timer.Tick -= OnTick; // Unsubscribe!
        _timer.Stop();
    }
}
```

**Use WeakEventManager:**

```csharp
// Instead of strong reference
WeakEventManager<DispatcherTimer, EventArgs>.AddHandler(
    _timer, "Tick", OnTick);

// Unsubscribe on close
WeakEventManager<DispatcherTimer, EventArgs>.RemoveHandler(
    _timer, "Tick", OnTick);
```

---

### 7. XAML Designer Errors

**Symptoms:**
- Designer shows errors
- "Could not create instance"
- Preview not loading

**Solutions:**

```csharp
// Add design-time DataContext
public MainViewModel()
{
    // Design-time data
    if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
    {
        Title = "Design Time Title";
        Items = new List<string> { "Item 1", "Item 2" };
    }
}
```

```xml
<!-- Add design-time DataContext in XAML -->
<UserControl
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    d:DataContext="{d:DesignInstance Type=local:MainViewModel, IsDesignTimeCreatable=True}"
    mc:Ignorable="d">
```

---

### 8. Build Errors

**"The name 'InitializeComponent' does not exist"**

```xml
<!-- Ensure XAML file build action is correct -->
<Page Include="Views\MainView.xaml">
  <Generator>MSBuild:Compile</Generator>
  <SubType>Designer</SubType>
</Page>
```

**"Could not find type 'ObservableProperty'"**

```csharp
// Ensure partial keyword is present
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "";
}
```

---

### 9. Widget Crashes Host

**Symptoms:**
- 3SC closes when widget loads
- Unhandled exception dialog

**Add global exception handler:**

```csharp
public void OnInitialize()
{
    try
    {
        DoInitialization();
    }
    catch (Exception ex)
    {
        _log.Fatal(ex, "Widget initialization failed");
        
        // Show error UI instead of crashing
        _view = new ErrorView { ErrorMessage = ex.Message };
    }
}
```

---

### 10. Manifest Validation Errors

**"Widget key invalid"**

```json
// ❌ Invalid
{ "widgetKey": "My Widget" }  // Spaces not allowed
{ "widgetKey": "MyWidget" }   // Must be lowercase

// ✅ Valid
{ "widgetKey": "my-widget" }
```

**"Entry point not found"**

```json
// Ensure assembly name matches
{
  "entryPoint": "3SC.Widgets.MyWidget.dll",  // Must match actual filename
  "factoryType": "_3SC.Widgets.MyWidget.MyWidgetFactory"  // Full type name
}
```

---

## Debugging Tips

### Enable Debug Logging

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Debug()  // Output to VS Debug window
    .WriteTo.File("logs/widget-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Check Assembly Loading

```csharp
AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
{
    Debug.WriteLine($"Loaded: {args.LoadedAssembly.FullName}");
};

AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
    Debug.WriteLine($"Failed to resolve: {args.Name}");
    return null;
};
```

### Inspect Binding Errors

```xml
<!-- Add to App.xaml or Window -->
<Window xmlns:diag="clr-namespace:System.Diagnostics;assembly=WindowsBase">
    <Window.Resources>
        <diag:PresentationTraceSources.TraceLevel>High</diag:PresentationTraceSources.TraceLevel>
    </Window.Resources>
```

---

## Getting Help

1. **Check logs:** `%APPDATA%\3SC\WidgetData\{widget-key}\logs\`
2. **Enable debug mode:** Build with Debug configuration
3. **Use TestLauncher:** Test widget in isolation
4. **Review skill files:** Check related documentation

---

## Related Skills

- [logging.md](../quality/logging.md) - Logging setup
- [error-handling.md](../quality/error-handling.md) - Error patterns
- [testing-strategies.md](../quality/testing-strategies.md) - Testing

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added more common issues |
| 1.0.0 | 2025-06-01 | Initial version |
