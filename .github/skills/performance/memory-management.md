# Memory Management

> **Category:** Performance | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Proper memory management prevents leaks and ensures widgets remain responsive. This skill covers disposable patterns, event cleanup, and memory profiling.

---

## IDisposable Pattern

```csharp
public class MyWidget : UserControl, IWidget, IDisposable
{
    private readonly ILogger _log;
    private readonly DispatcherTimer _timer;
    private readonly HttpClient _http;
    private bool _disposed;
    
    public MyWidget()
    {
        _log = ServiceLocator.Resolve<ILogger>();
        _timer = new DispatcherTimer();
        _http = new HttpClient();
        
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += Timer_Tick;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            // Stop and unhook timer
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            
            // Dispose HTTP client
            _http.Dispose();
            
            // Dispose ViewModel if disposable
            if (DataContext is IDisposable vm)
            {
                vm.Dispose();
            }
            
            _log.Information("Widget disposed");
        }
        
        _disposed = true;
    }
    
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_disposed) return;
        // Timer logic
    }
}
```

---

## ViewModel Disposal

```csharp
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<IDisposable> _subscriptions = new();
    private bool _disposed;
    
    public MainViewModel()
    {
        // Track subscriptions for cleanup
        _subscriptions.Add(
            Observable.Interval(TimeSpan.FromMinutes(1))
                .Subscribe(_ => RefreshData()));
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        // Cancel all async operations
        _cts.Cancel();
        _cts.Dispose();
        
        // Dispose all subscriptions
        foreach (var sub in _subscriptions)
        {
            sub.Dispose();
        }
        _subscriptions.Clear();
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            await SomeApiCall(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal during disposal
        }
    }
}
```

---

## Event Subscription Cleanup

```csharp
public partial class MyWidget : UserControl, IWidget
{
    private readonly CompositeDisposable _events = new();
    
    public void OnInitialize()
    {
        // Use weak event pattern or track subscriptions
        var handler = new PropertyChangedEventHandler(OnSettingsChanged);
        _settings.PropertyChanged += handler;
        
        // Track for cleanup
        _events.Add(Disposable.Create(() => 
            _settings.PropertyChanged -= handler));
    }
    
    public void OnClose()
    {
        _events.Dispose();
    }
}

// Simple disposable helper
public class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    
    public void Add(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }
    
    public void Dispose()
    {
        foreach (var d in _disposables)
        {
            d.Dispose();
        }
        _disposables.Clear();
    }
}

public static class Disposable
{
    public static IDisposable Create(Action dispose) => new ActionDisposable(dispose);
    
    private class ActionDisposable : IDisposable
    {
        private Action? _dispose;
        public ActionDisposable(Action dispose) => _dispose = dispose;
        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
```

---

## Image Memory Management

```csharp
public class ImageCache : IDisposable
{
    private readonly Dictionary<string, WeakReference<BitmapImage>> _cache = new();
    private readonly object _lock = new();
    
    public BitmapImage? GetOrLoad(string path)
    {
        lock (_lock)
        {
            // Check weak reference
            if (_cache.TryGetValue(path, out var weakRef) && 
                weakRef.TryGetTarget(out var cached))
            {
                return cached;
            }
            
            // Load new image
            var image = LoadImage(path);
            if (image != null)
            {
                _cache[path] = new WeakReference<BitmapImage>(image);
            }
            return image;
        }
    }
    
    private BitmapImage? LoadImage(string path)
    {
        if (!File.Exists(path)) return null;
        
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad; // Release file handle
        image.UriSource = new Uri(path);
        image.EndInit();
        image.Freeze(); // Make cross-thread accessible, reduces memory
        return image;
    }
    
    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
    }
    
    public void Dispose()
    {
        Clear();
    }
}
```

---

## Collection Memory

```csharp
public partial class ListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemViewModel> _items = new();
    
    // Clear properly to release item references
    public void ClearItems()
    {
        foreach (var item in Items)
        {
            if (item is IDisposable d)
            {
                d.Dispose();
            }
        }
        Items.Clear();
    }
    
    // Use virtualization for large lists
    // In XAML:
    // <ListBox VirtualizingPanel.IsVirtualizing="True"
    //          VirtualizingPanel.VirtualizationMode="Recycling"/>
}
```

---

## Memory Profiling Helpers

```csharp
public static class MemoryDiagnostics
{
    public static void LogMemoryUsage(ILogger log, string context)
    {
        var process = Process.GetCurrentProcess();
        
        log.Debug("Memory [{Context}]: Working={WorkingMB:F1}MB, Private={PrivateMB:F1}MB, GC={GcMB:F1}MB",
            context,
            process.WorkingSet64 / 1024.0 / 1024.0,
            process.PrivateMemorySize64 / 1024.0 / 1024.0,
            GC.GetTotalMemory(false) / 1024.0 / 1024.0);
    }
    
    public static void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

// Usage
public void OnClose()
{
    MemoryDiagnostics.LogMemoryUsage(_log, "Before cleanup");
    
    // Cleanup
    _imageCache.Dispose();
    _dataCache.Clear();
    
    MemoryDiagnostics.LogMemoryUsage(_log, "After cleanup");
}
```

---

## Avoiding Common Leaks

```csharp
// ❌ BAD: Static event handler holds reference
public class LeakyWidget
{
    public LeakyWidget()
    {
        Application.Current.Activated += OnActivated; // LEAK!
    }
}

// ✅ GOOD: Unsubscribe on close
public class SafeWidget : IWidget
{
    public void OnInitialize()
    {
        Application.Current.Activated += OnActivated;
    }
    
    public void OnClose()
    {
        Application.Current.Activated -= OnActivated;
    }
}

// ❌ BAD: Lambda captures 'this' forever
someService.DataChanged += (s, e) => UpdateUI(); // LEAK!

// ✅ GOOD: Store handler for removal
private EventHandler _dataChangedHandler;

public void OnInitialize()
{
    _dataChangedHandler = (s, e) => UpdateUI();
    someService.DataChanged += _dataChangedHandler;
}

public void OnClose()
{
    someService.DataChanged -= _dataChangedHandler;
}
```

---

## Best Practices

1. **Implement IDisposable** - Always for classes with resources
2. **Unsubscribe events** - Track and remove all handlers
3. **Cancel async ops** - Use CancellationToken in OnClose
4. **Freeze images** - Reduces memory and enables cross-thread
5. **Use weak references** - For caches that can be reclaimed
6. **Virtualize lists** - For collections over 100 items

---

## Related Skills

- [optimization.md](optimization.md) - Performance optimization
- [host-communication.md](../integration/host-communication.md) - Widget lifecycle

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added profiling helpers |
| 1.0.0 | 2025-06-01 | Initial version |
