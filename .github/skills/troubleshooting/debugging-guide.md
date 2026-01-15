# Debugging Guide

> **Category:** Troubleshooting | **Priority:** 🔴 Essential
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Comprehensive debugging techniques for 3SC widgets including Visual Studio setup, logging analysis, and common debugging scenarios.

---

## Debug Launch Configuration

### Debug via TestLauncher (Recommended)

```xml
<!-- .csproj dual-mode configuration -->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <OutputType>WinExe</OutputType>
</PropertyGroup>
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <OutputType>Library</OutputType>
</PropertyGroup>
```

### launchSettings.json
```json
{
  "profiles": {
    "TestLauncher": {
      "commandName": "Project",
      "environmentVariables": {
        "WIDGET_DEBUG": "true"
      }
    }
  }
}
```

---

## Attaching to Host Process

When debugging widget loaded by 3SC host:

1. **Build widget in Release mode** (Library output)
2. **Install to widget folder**:
   ```
   %APPDATA%\3SC\Widgets\Community\{widget-key}\
   ```
3. **Start 3SC host application**
4. **In VS: Debug → Attach to Process**
5. **Select 3SC.exe or 3SC.UI.exe**
6. **Set breakpoints in widget code**

---

## Diagnostic Logging

```csharp
public static class DebugHelper
{
    public static void ConfigureDebugLogging(ILogger log)
    {
#if DEBUG
        // Enhanced logging in debug mode
        log.Debug("=== DEBUG SESSION STARTED ===");
        log.Debug("Widget Assembly: {Assembly}", Assembly.GetExecutingAssembly().Location);
        log.Debug("Working Directory: {Dir}", Environment.CurrentDirectory);
        log.Debug("App Data: {AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        log.Debug("CLR Version: {Version}", Environment.Version);
#endif
    }
    
    public static void DumpObject(ILogger log, string name, object? obj)
    {
#if DEBUG
        if (obj == null)
        {
            log.Debug("{Name} = null", name);
            return;
        }
        
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        log.Debug("{Name} = {Value}", name, json);
#endif
    }
}
```

---

## XAML Debugging

### Enable XAML Binding Failures

Add to App.xaml.cs or widget initialization:
```csharp
public void OnInitialize()
{
#if DEBUG
    // Show binding errors in Output window
    PresentationTraceSources.DataBindingSource.Listeners.Add(
        new ConsoleTraceListener());
    PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
#endif
}
```

### Visual Studio XAML Tools
1. **Live Visual Tree**: Debug → Windows → Live Visual Tree
2. **Live Property Explorer**: Debug → Windows → Live Property Explorer
3. **Hot Reload**: Edit XAML while running (Ctrl+Shift+Enter)

---

## Conditional Breakpoints

```csharp
// Set conditional breakpoint on this line
// Condition: item.Name == "SpecificItem"
foreach (var item in items)
{
    ProcessItem(item); // Right-click → Breakpoint → Conditions
}
```

### Useful Breakpoint Conditions
- `i == 5` - Break on specific iteration
- `string.IsNullOrEmpty(name)` - Break on null/empty
- `exception != null` - Break when exception caught
- `items.Count > 100` - Break on threshold

---

## Exception Debugging

### First Chance Exceptions
1. **Debug → Windows → Exception Settings**
2. **Check "Common Language Runtime Exceptions"**
3. **Break when exception is thrown**, not just unhandled

### Exception Helper Code
```csharp
public static class ExceptionDebug
{
    [Conditional("DEBUG")]
    public static void LogException(ILogger log, Exception ex, string context)
    {
        log.Error(ex, "Exception in {Context}", context);
        log.Debug("Stack: {Stack}", ex.StackTrace);
        
        if (ex.InnerException != null)
        {
            log.Debug("Inner: {Inner}", ex.InnerException.Message);
        }
    }
}
```

---

## Memory Debugging

### Visual Studio Memory Tools
1. **Debug → Windows → Diagnostic Tools**
2. **Take snapshot** at different points
3. **Compare snapshots** for leaks

### Manual Memory Check
```csharp
[Conditional("DEBUG")]
public static void CheckMemory(ILogger log, string checkpoint)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    
    var memory = GC.GetTotalMemory(true) / 1024.0 / 1024.0;
    log.Debug("[{Checkpoint}] Memory: {Memory:F2} MB", checkpoint, memory);
}
```

---

## Performance Debugging

### Stopwatch Profiling
```csharp
public class DebugTimer : IDisposable
{
    private readonly ILogger _log;
    private readonly string _operation;
    private readonly Stopwatch _sw;
    
    public DebugTimer(ILogger log, string operation)
    {
        _log = log;
        _operation = operation;
        _sw = Stopwatch.StartNew();
    }
    
    public void Dispose()
    {
        _sw.Stop();
        _log.Debug("{Operation} took {Ms}ms", _operation, _sw.ElapsedMilliseconds);
    }
}

// Usage
using (new DebugTimer(_log, "LoadData"))
{
    await LoadDataAsync();
}
```

---

## Common Debug Scenarios

### Widget Not Loading

```csharp
// Add to factory
public IWidget Create()
{
    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Factory.Create() called");
    
    try
    {
        var widget = new MyWidget();
        Debug.WriteLine($"Widget created: {widget.GetType().FullName}");
        return widget;
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Factory failed: {ex}");
        throw;
    }
}
```

### Binding Not Working

```csharp
// Check DataContext
public void OnInitialize()
{
    Debug.WriteLine($"DataContext type: {DataContext?.GetType().Name ?? "null"}");
    
    if (DataContext is MainViewModel vm)
    {
        Debug.WriteLine($"VM.Title = {vm.Title}");
    }
}
```

### Command Not Firing

```csharp
[RelayCommand]
private void MyAction()
{
    Debug.WriteLine("MyAction command executed!");
    _log.Debug("MyAction executed");
    // ... implementation
}
```

---

## Log File Locations

| Widget | Log Path |
|--------|----------|
| MyWidget | `%APPDATA%\3SC\Widgets\Community\my-widget\logs\` |
| 3SC Host | `%APPDATA%\3SC\logs\` |

### Viewing Logs
```powershell
# PowerShell: Tail widget log
Get-Content "$env:APPDATA\3SC\Widgets\Community\my-widget\logs\widget*.log" -Tail 50 -Wait
```

---

## Debug Output Window

```csharp
using System.Diagnostics;

// Writes to VS Output window (Debug pane)
Debug.WriteLine("Debug message");
Trace.WriteLine("Trace message");

// Conditional
Debug.WriteLineIf(condition, "Conditional message");
```

---

## Best Practices

1. **Use TestLauncher first** - Faster iteration than host attachment
2. **Enable XAML diagnostics** - Catch binding errors early
3. **Log extensively in debug** - Use `#if DEBUG` for verbose logging
4. **Use conditional breakpoints** - Save time on complex loops
5. **Check log files** - They persist after crashes

---

## Related Skills

- [common-issues.md](common-issues.md) - Common problems
- [logging.md](../quality/logging.md) - Logging setup

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added memory debugging |
| 1.0.0 | 2025-06-01 | Initial version |
