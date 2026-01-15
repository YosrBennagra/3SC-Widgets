# Host Communication

> **Category:** Integration | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers communication between widgets and the 3SC host application, including the widget lifecycle, host callbacks, and event handling.

---

## Communication Model

```
┌─────────────────────────────────────────────────────────────┐
│                      3SC Host Application                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   ┌─────────────────┐        ┌─────────────────┐            │
│   │  Widget Manager │───────▶│  Widget Instance │            │
│   └─────────────────┘        └─────────────────┘            │
│           │                          │                       │
│           │ Creates                  │ Implements            │
│           │ Manages                  │ IExternalWidget       │
│           │ Destroys                 │                       │
│           │                          │                       │
│           ▼                          ▼                       │
│   ┌─────────────────┐        ┌─────────────────┐            │
│   │    Lifecycle    │        │  Host Callbacks │            │
│   │    Events       │───────▶│  (via interface)│            │
│   └─────────────────┘        └─────────────────┘            │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Widget Lifecycle

The host calls these methods in order:

```csharp
public class MyWidget : IExternalWidget
{
    // 1. Factory creates instance
    public MyWidget()
    {
        // Basic initialization only
        // Don't access files, settings, or UI here
    }
    
    // 2. Host calls OnInitialize after loading
    public void OnInitialize()
    {
        // ✅ Safe to:
        // - Load settings
        // - Create UI elements
        // - Start timers
        // - Connect to services
        InitializeServices();
        LoadSettings();
        StartBackgroundTasks();
    }
    
    // 3. Host calls OnResize when widget resizes
    public void OnResize(Size newSize)
    {
        // Adjust UI for new size
        UpdateLayout(newSize);
    }
    
    // 4. Host calls OnClose before unloading
    public void OnClose()
    {
        // ✅ Must:
        // - Save state
        // - Stop timers
        // - Dispose resources
        // - Cancel async operations
        SaveState();
        Cleanup();
    }
}
```

---

## IExternalWidget Interface

```csharp
/// <summary>
/// Interface that external widgets must implement.
/// </summary>
public interface IExternalWidget : IWidget
{
    /// <summary>
    /// Called by host when widget should initialize.
    /// </summary>
    void OnInitialize();
    
    /// <summary>
    /// Called by host when widget container resizes.
    /// </summary>
    /// <param name="newSize">The new size of the widget container.</param>
    void OnResize(Size newSize);
    
    /// <summary>
    /// Called by host before widget is unloaded.
    /// </summary>
    void OnClose();
}

/// <summary>
/// Base widget interface.
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Unique identifier for this widget type.
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Display name shown in UI.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// The UI content to display.
    /// </summary>
    FrameworkElement Content { get; }
    
    /// <summary>
    /// Whether this widget has a settings dialog.
    /// </summary>
    bool HasSettings { get; }
    
    /// <summary>
    /// Opens the settings dialog.
    /// </summary>
    void ShowSettings();
}
```

---

## Implementing Host Callbacks

### Complete Implementation

```csharp
public class ClockWidget : IExternalWidget
{
    private readonly ILogger _log;
    private ClockView? _view;
    private ClockViewModel? _viewModel;
    private DispatcherTimer? _timer;
    private CancellationTokenSource? _cts;
    
    public string Key => "clock";
    public string Name => "Clock Widget";
    public bool HasSettings => true;
    public FrameworkElement Content => _view!;
    
    public ClockWidget()
    {
        _log = LoggingConfiguration.CreateLogger(Key);
    }
    
    public void OnInitialize()
    {
        _log.Information("Initializing widget");
        
        try
        {
            _cts = new CancellationTokenSource();
            
            // Create view model
            _viewModel = new ClockViewModel(_log);
            
            // Create view
            _view = new ClockView { DataContext = _viewModel };
            
            // Load saved settings
            _viewModel.LoadSettings();
            
            // Start clock timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
            
            _log.Information("Widget initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize widget");
            throw;
        }
    }
    
    public void OnResize(Size newSize)
    {
        _log.Debug("Widget resized to {Width}x{Height}", newSize.Width, newSize.Height);
        
        if (_view != null)
        {
            _view.Width = newSize.Width;
            _view.Height = newSize.Height;
        }
        
        // Notify view model for adaptive layout
        _viewModel?.HandleResize(newSize);
    }
    
    public void OnClose()
    {
        _log.Information("Closing widget");
        
        // Cancel pending operations
        _cts?.Cancel();
        
        // Stop timer
        _timer?.Stop();
        _timer = null;
        
        // Save state
        try
        {
            _viewModel?.SaveSettings();
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to save settings on close");
        }
        
        // Dispose resources
        (_viewModel as IDisposable)?.Dispose();
        _cts?.Dispose();
        
        // Flush logs
        Log.CloseAndFlush();
    }
    
    public void ShowSettings()
    {
        _log.Debug("Opening settings dialog");
        
        var settingsWindow = new SettingsWindow(_viewModel!.Settings);
        settingsWindow.Owner = Window.GetWindow(_view);
        
        if (settingsWindow.ShowDialog() == true)
        {
            _viewModel.ApplySettings(settingsWindow.Settings);
        }
    }
    
    private void OnTimerTick(object? sender, EventArgs e)
    {
        _viewModel?.UpdateTime();
    }
}
```

---

## Error Handling with Host

```csharp
public void OnInitialize()
{
    try
    {
        DoInitialization();
    }
    catch (Exception ex)
    {
        _log.Fatal(ex, "Widget failed to initialize");
        
        // Create error UI instead of crashing
        _view = CreateErrorView(ex);
        
        // Don't re-throw - let host continue with other widgets
    }
}

private FrameworkElement CreateErrorView(Exception ex)
{
    return new Border
    {
        Background = new SolidColorBrush(Color.FromRgb(50, 0, 0)),
        Child = new TextBlock
        {
            Text = $"Widget failed to load:\n{ex.Message}",
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(10)
        }
    };
}
```

---

## Graceful Shutdown

```csharp
public void OnClose()
{
    // Set timeout for cleanup operations
    var shutdownTimeout = TimeSpan.FromSeconds(5);
    var cts = new CancellationTokenSource(shutdownTimeout);
    
    try
    {
        // Cancel ongoing work first
        _operationCts?.Cancel();
        
        // Wait for tasks with timeout
        var tasks = _runningTasks.ToArray();
        if (tasks.Length > 0)
        {
            Task.WaitAll(tasks, cts.Token);
        }
    }
    catch (OperationCanceledException)
    {
        _log.Warning("Shutdown timed out after {Timeout}s", shutdownTimeout.TotalSeconds);
    }
    catch (AggregateException ae)
    {
        _log.Warning(ae, "Errors during shutdown");
    }
    finally
    {
        // Always dispose resources
        DisposeResources();
        Log.CloseAndFlush();
    }
}

private void DisposeResources()
{
    _timer?.Dispose();
    _httpClient?.Dispose();
    _cts?.Dispose();
}
```

---

## Thread Safety

```csharp
public class ThreadSafeWidget : IExternalWidget
{
    private readonly object _lock = new();
    private bool _isInitialized;
    private bool _isClosing;
    
    public void OnInitialize()
    {
        lock (_lock)
        {
            if (_isInitialized) return;
            
            DoInitialization();
            _isInitialized = true;
        }
    }
    
    public void OnClose()
    {
        lock (_lock)
        {
            if (_isClosing) return;
            _isClosing = true;
            
            DoCleanup();
        }
    }
    
    // For UI updates from background threads
    private void UpdateUI(Action action)
    {
        if (_view?.Dispatcher.CheckAccess() == true)
        {
            action();
        }
        else
        {
            _view?.Dispatcher.BeginInvoke(action);
        }
    }
}
```

---

## Host Events (Future)

```csharp
// Potential future host event system
public interface IHostEvents
{
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    event EventHandler<SystemStateEventArgs>? SystemSuspending;
    event EventHandler<SystemStateEventArgs>? SystemResuming;
}

// Widget subscription
public void OnInitialize()
{
    if (HostServices.Events != null)
    {
        HostServices.Events.ThemeChanged += OnThemeChanged;
    }
}

private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
{
    _viewModel.UpdateTheme(e.NewTheme);
}
```

---

## Best Practices

1. **Fast initialization** - Keep OnInitialize quick, defer heavy work
2. **Complete cleanup** - OnClose must release all resources
3. **Handle resize gracefully** - Support various widget sizes
4. **Thread-safe operations** - UI updates on dispatcher thread
5. **Log lifecycle events** - Aid debugging and diagnostics

---

## Related Skills

- [widget-architecture.md](../core/widget-architecture.md) - Widget structure
- [contracts-interfaces.md](../core/contracts-interfaces.md) - Interface details

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added thread safety, graceful shutdown |
| 1.0.0 | 2025-06-01 | Initial version |
