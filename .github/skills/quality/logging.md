# Logging

> **Category:** Quality | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers logging with Serilog, including setup, structured logging, log levels, and file output configuration.

---

## ⚠️ CRITICAL VERSION REQUIREMENT

```
┌─────────────────────────────────────────────────────────────┐
│  ⚠️  SERILOG VERSION: MUST USE 3.1.1 (NOT 4.x)              │
│                                                              │
│  The 3SC host application uses Serilog 3.1.1                │
│  Using Serilog 4.x causes assembly binding failures!        │
│                                                              │
│  <PackageReference Include="Serilog" Version="3.1.1" />     │
│  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />  │
└─────────────────────────────────────────────────────────────┘
```

---

## Setup

### NuGet Packages

```xml
<ItemGroup>
  <!-- CRITICAL: Use version 3.1.1 exactly -->
  <PackageReference Include="Serilog" Version="3.1.1" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
</ItemGroup>
```

### Logger Initialization

```csharp
public static class LoggingConfiguration
{
    public static ILogger CreateLogger(string widgetKey)
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", widgetKey, "logs", "widget-.log");
        
        return new LoggerConfiguration()
            .MinimumLevel.Information()
#if DEBUG
            .MinimumLevel.Debug()
            .WriteTo.Debug()
#endif
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Enrich.WithProperty("WidgetKey", widgetKey)
            .CreateLogger();
    }
}
```

### Initialize in Widget

```csharp
public class MyWidget : IExternalWidget
{
    private readonly ILogger _log;
    
    public MyWidget()
    {
        _log = LoggingConfiguration.CreateLogger("my-widget");
        Log.Logger = _log; // Set as default for static access
    }
    
    public void OnInitialize()
    {
        _log.Information("Widget initialized");
    }
    
    public void OnClose()
    {
        _log.Information("Widget closing");
        Log.CloseAndFlush();
    }
}
```

---

## Log Levels

| Level | Usage | Example |
|-------|-------|---------|
| **Verbose** | Detailed diagnostics | Loop iterations |
| **Debug** | Development info | Method entry/exit |
| **Information** | Normal events | User actions |
| **Warning** | Recoverable issues | Missing optional config |
| **Error** | Errors requiring attention | Failed operations |
| **Fatal** | System failures | Unrecoverable errors |

```csharp
// Level usage examples
_log.Verbose("Processing item {Index} of {Total}", i, items.Count);
_log.Debug("LoadSettings called with path: {Path}", settingsPath);
_log.Information("Widget started successfully");
_log.Warning("Settings file not found, using defaults");
_log.Error(exception, "Failed to save settings");
_log.Fatal(exception, "Widget cannot recover from error");
```

---

## Structured Logging

### Use Templates, Not String Interpolation

```csharp
// ❌ WRONG - String interpolation (loses structure)
_log.Information($"User {username} performed action {action}");

// ✅ CORRECT - Message template (preserves structure)
_log.Information("User {Username} performed action {Action}", username, action);
```

### Semantic Property Names

```csharp
// Use PascalCase for property names
_log.Information("Loading widget {WidgetKey} version {Version}", widgetKey, version);

// Include relevant context
_log.Information("Settings saved in {ElapsedMs}ms for {SettingsType}", 
    stopwatch.ElapsedMilliseconds, typeof(T).Name);
```

### Complex Objects

```csharp
// Log objects with @ for destructuring
_log.Debug("Settings loaded: {@Settings}", settings);

// Use $ for simple ToString()
_log.Information("Widget position: {$Position}", position);
```

---

## Contextual Logging

### Using ForContext

```csharp
public class SettingsService
{
    private readonly ILogger _log;
    
    public SettingsService(ILogger log)
    {
        // Create child logger with context
        _log = log.ForContext<SettingsService>();
    }
    
    public void Save(WidgetSettings settings)
    {
        _log.Information("Saving settings");
        // Log messages include SourceContext property
    }
}
```

### Adding Context Properties

```csharp
// Add properties to specific log call
_log.ForContext("OperationId", operationId)
    .Information("Operation started");

// Add properties to all subsequent logs
_log = _log.ForContext("SessionId", sessionId);
```

---

## Performance Logging

```csharp
public class PerformanceLogger
{
    private readonly ILogger _log;
    
    public PerformanceLogger(ILogger log)
    {
        _log = log.ForContext<PerformanceLogger>();
    }
    
    public IDisposable TimeOperation(string operationName)
    {
        return new TimedOperation(_log, operationName);
    }
}

public class TimedOperation : IDisposable
{
    private readonly ILogger _log;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    
    public TimedOperation(ILogger log, string operationName)
    {
        _log = log;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
        
        _log.Debug("Starting {Operation}", operationName);
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        _log.Information("{Operation} completed in {ElapsedMs}ms", 
            _operationName, _stopwatch.ElapsedMilliseconds);
    }
}

// Usage
using (_perfLog.TimeOperation("LoadSettings"))
{
    settings = await LoadSettingsAsync();
}
// Output: "LoadSettings completed in 45ms"
```

---

## Exception Logging

```csharp
// Always pass exception as first parameter
try
{
    await LoadDataAsync();
}
catch (Exception ex)
{
    // ✅ Exception logged with full details
    _log.Error(ex, "Failed to load data from {Source}", source);
    
    // ❌ Wrong - exception details lost
    _log.Error("Failed to load data: {Message}", ex.Message);
}
```

### Enriching Exception Context

```csharp
try
{
    await ProcessItemAsync(item);
}
catch (Exception ex)
{
    _log.ForContext("ItemId", item.Id)
        .ForContext("ItemType", item.GetType().Name)
        .Error(ex, "Failed to process item");
    throw;
}
```

---

## File Sink Configuration

```csharp
.WriteTo.File(
    path: logPath,
    
    // Roll daily, keep 7 days
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7,
    
    // Max file size before rolling
    fileSizeLimitBytes: 10_000_000, // 10MB
    rollOnFileSizeLimit: true,
    
    // Async for performance
    buffered: true,
    flushToDiskInterval: TimeSpan.FromSeconds(1),
    
    // Format
    outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
)
```

### Output Templates

```csharp
// Compact
"{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"

// Detailed
"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}"

// JSON (for log aggregation)
.WriteTo.File(
    new CompactJsonFormatter(),
    logPath)
```

---

## Log File Locations

```
%APPDATA%\3SC\WidgetData\{widget-key}\logs\
├── widget-20250315.log
├── widget-20250316.log
└── widget-20250317.log
```

### Accessing Logs

```csharp
public static string GetLogDirectory(string widgetKey)
{
    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC", "WidgetData", widgetKey, "logs");
}

public static void OpenLogDirectory(string widgetKey)
{
    var logDir = GetLogDirectory(widgetKey);
    if (Directory.Exists(logDir))
    {
        Process.Start("explorer.exe", logDir);
    }
}
```

---

## Conditional Logging

```csharp
// Check level before expensive operations
if (_log.IsEnabled(LogEventLevel.Debug))
{
    var expensiveData = ComputeDebugInfo();
    _log.Debug("Debug info: {@Data}", expensiveData);
}
```

---

## Testing with Logs

```csharp
// Create a testable logger that captures logs
public class TestLogger : ILogger
{
    public List<LogEvent> Events { get; } = new();
    
    public void Write(LogEvent logEvent)
    {
        Events.Add(logEvent);
    }
    
    // ... implement other ILogger members
}

// In tests
[Fact]
public void Operation_ShouldLogSuccess()
{
    var testLog = new TestLogger();
    var service = new MyService(testLog);
    
    service.DoOperation();
    
    testLog.Events.Should().Contain(e => 
        e.Level == LogEventLevel.Information && 
        e.MessageTemplate.Text.Contains("Operation completed"));
}
```

---

## Best Practices

1. **Use structured logging** - Templates, not interpolation
2. **Include context** - Use ForContext for enrichment
3. **Log at boundaries** - Service entry/exit, API calls
4. **Avoid sensitive data** - Never log passwords, tokens
5. **Close on shutdown** - Always call Log.CloseAndFlush()

---

## Related Skills

- [error-handling.md](error-handling.md) - Error logging
- [testing-strategies.md](testing-strategies.md) - Testing logs

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added performance logging |
| 1.0.0 | 2025-06-01 | Initial version |
