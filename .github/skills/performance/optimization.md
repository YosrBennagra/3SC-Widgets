# Performance Optimization

> **Category:** Performance | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers performance optimization strategies for widgets, including startup time, memory usage, rendering efficiency, and battery-conscious design.

---

## Performance Goals

```
┌─────────────────────────────────────────────────────────────┐
│                    Performance Targets                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   Startup Time:    < 500ms to first render                   │
│   Memory Usage:    < 50MB per widget                         │
│   CPU Idle:        < 1% when not updating                    │
│   Render Time:     < 16ms per frame (60 FPS)                 │
│   Battery Impact:  Minimal (support power modes)             │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Startup Optimization

### Lazy Initialization

```csharp
public class OptimizedWidget : IExternalWidget
{
    private ClockView? _view;
    private ClockViewModel? _viewModel;
    private SettingsService? _settingsService;
    
    // Defer heavy initialization
    private ClockViewModel ViewModel => _viewModel ??= CreateViewModel();
    private SettingsService Settings => _settingsService ??= new SettingsService(Key);
    
    public void OnInitialize()
    {
        // Create minimal UI immediately
        _view = new ClockView();
        
        // Defer data loading
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        // Load data in background
        await Task.Run(() =>
        {
            var settings = Settings.Load();
            _view.Dispatcher.Invoke(() =>
            {
                _viewModel = CreateViewModel();
                _viewModel.ApplySettings(settings);
                _view.DataContext = _viewModel;
            });
        });
    }
}
```

### Precompiled Views

```csharp
// Avoid runtime XAML parsing in hot paths
static MyWidget()
{
    // Pre-initialize type metadata
    var type = typeof(MainView);
    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
}
```

---

## Memory Management

### Object Pooling

```csharp
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentBag<T> _objects = new();
    private readonly int _maxSize;
    
    public ObjectPool(int maxSize = 100)
    {
        _maxSize = maxSize;
    }
    
    public T Rent()
    {
        return _objects.TryTake(out var item) ? item : new T();
    }
    
    public void Return(T item)
    {
        if (_objects.Count < _maxSize)
        {
            _objects.Add(item);
        }
    }
}

// Usage for frequently created objects
private static readonly ObjectPool<StringBuilder> StringBuilderPool = new();

public string FormatTime(DateTime time)
{
    var sb = StringBuilderPool.Rent();
    try
    {
        sb.Clear();
        sb.Append(time.Hour.ToString("00"));
        sb.Append(':');
        sb.Append(time.Minute.ToString("00"));
        return sb.ToString();
    }
    finally
    {
        StringBuilderPool.Return(sb);
    }
}
```

### Avoid Memory Leaks

```csharp
public class LeakFreeWidget : IExternalWidget, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private WeakReference<SettingsWindow>? _settingsRef;
    
    public void OnInitialize()
    {
        // Use weak events to avoid leaks
        WeakEventManager<DispatcherTimer, EventArgs>.AddHandler(
            _timer, "Tick", OnTimerTick);
        
        // Track disposables
        _disposables.Add(_timer);
        _disposables.Add(_settingsService);
    }
    
    public void ShowSettings()
    {
        // Don't hold strong reference to dialogs
        var settings = new SettingsWindow();
        _settingsRef = new WeakReference<SettingsWindow>(settings);
        settings.ShowDialog();
    }
    
    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }
        _disposables.Clear();
        
        // Unsubscribe events
        WeakEventManager<DispatcherTimer, EventArgs>.RemoveHandler(
            _timer, "Tick", OnTimerTick);
    }
}
```

---

## Rendering Optimization

### UI Virtualization

```xml
<!-- For large lists, use VirtualizingStackPanel -->
<ListBox ItemsSource="{Binding Items}">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel 
                VirtualizationMode="Recycling"
                IsVirtualizing="True"/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

### Reduce Visual Tree Complexity

```xml
<!-- ❌ Deep nesting impacts performance -->
<Border>
    <Grid>
        <StackPanel>
            <Border>
                <Grid>
                    <TextBlock Text="Hello"/>
                </Grid>
            </Border>
        </StackPanel>
    </Grid>
</Border>

<!-- ✅ Flattened structure -->
<Grid>
    <TextBlock Text="Hello" Margin="10"/>
</Grid>
```

### Use Freezable Resources

```csharp
// Freeze brushes and geometries to improve performance
private static readonly SolidColorBrush BackgroundBrush;

static MyWidget()
{
    BackgroundBrush = new SolidColorBrush(Color.FromRgb(26, 26, 46));
    BackgroundBrush.Freeze(); // Thread-safe, faster rendering
}
```

### Cached Compositions

```xml
<!-- Cache complex visuals -->
<Border CacheMode="BitmapCache">
    <Path Data="{StaticResource ComplexGeometry}" 
          Fill="{StaticResource AccentBrush}"/>
</Border>
```

---

## Update Throttling

```csharp
public class ThrottledUpdater
{
    private readonly TimeSpan _interval;
    private DateTime _lastUpdate;
    private readonly object _lock = new();
    
    public ThrottledUpdater(TimeSpan interval)
    {
        _interval = interval;
    }
    
    public bool ShouldUpdate()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            if (now - _lastUpdate >= _interval)
            {
                _lastUpdate = now;
                return true;
            }
            return false;
        }
    }
}

// Usage
private readonly ThrottledUpdater _uiThrottle = new(TimeSpan.FromMilliseconds(50));

private void UpdateDisplay(Data data)
{
    if (_uiThrottle.ShouldUpdate())
    {
        _viewModel.UpdateDisplay(data);
    }
}
```

### Debounced Property Updates

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly Timer _updateTimer;
    private string? _pendingSearch;
    
    [ObservableProperty]
    private string? _searchText;
    
    partial void OnSearchTextChanged(string? value)
    {
        _pendingSearch = value;
        _updateTimer.Change(300, Timeout.Infinite); // Debounce 300ms
    }
    
    private void OnDebounceElapsed(object? state)
    {
        if (_pendingSearch != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PerformSearch(_pendingSearch);
            });
        }
    }
}
```

---

## Timer Optimization

```csharp
public class EfficientTimerManager
{
    private DispatcherTimer? _timer;
    private readonly TimeSpan _activeInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _idleInterval = TimeSpan.FromMinutes(1);
    private bool _isIdle;
    
    public void Start()
    {
        _timer = new DispatcherTimer { Interval = _activeInterval };
        _timer.Tick += OnTick;
        _timer.Start();
        
        // Listen for system idle
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }
    
    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                _timer?.Stop();
                break;
            case PowerModes.Resume:
                _timer?.Start();
                break;
        }
    }
    
    public void SetIdleMode(bool idle)
    {
        if (_timer == null || _isIdle == idle) return;
        
        _isIdle = idle;
        _timer.Interval = idle ? _idleInterval : _activeInterval;
    }
}
```

---

## Async Best Practices

```csharp
public class AsyncOptimizations
{
    // Use ValueTask for frequently called methods that often complete synchronously
    public ValueTask<Data> GetCachedDataAsync()
    {
        if (_cache.TryGet(out var data))
        {
            return ValueTask.FromResult(data); // No allocation
        }
        return new ValueTask<Data>(LoadDataAsync());
    }
    
    // Use ConfigureAwait(false) in library code
    public async Task<string> FetchDataAsync()
    {
        var response = await _http.GetAsync(url).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
    
    // Avoid async void (except event handlers)
    // ❌ Wrong
    public async void LoadData() { }
    
    // ✅ Correct
    public async Task LoadDataAsync() { }
}
```

---

## Profiling Tools

### Built-in Diagnostics

```csharp
public class PerformanceMonitor
{
    public static void TraceMemory(string label)
    {
        var mem = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        Debug.WriteLine($"[MEM] {label}: {mem:F2} MB");
    }
    
    public static IDisposable TimedSection(string name)
    {
        return new TimedOperation(name);
    }
    
    private class TimedOperation : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _sw;
        
        public TimedOperation(string name)
        {
            _name = name;
            _sw = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            _sw.Stop();
            Debug.WriteLine($"[PERF] {_name}: {_sw.ElapsedMilliseconds}ms");
        }
    }
}

// Usage
using (PerformanceMonitor.TimedSection("LoadSettings"))
{
    await LoadSettingsAsync();
}
```

---

## Performance Checklist

- [ ] Startup under 500ms
- [ ] Use lazy initialization for heavy objects
- [ ] Freeze brushes and resources
- [ ] Virtualize large lists
- [ ] Throttle frequent updates
- [ ] Use ConfigureAwait(false)
- [ ] Stop timers when idle/suspended
- [ ] Dispose all resources
- [ ] Profile memory in release mode

---

## Related Skills

- [async-patterns.md](async-patterns.md) - Async optimization
- [memory-management.md](memory-management.md) - Memory details

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added profiling tools |
| 1.0.0 | 2025-06-01 | Initial version |
