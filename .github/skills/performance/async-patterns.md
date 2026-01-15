# Async Patterns

> **Category:** Performance | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers async/await patterns for widgets, including proper Task handling, cancellation, progress reporting, and common pitfalls.

---

## Async Fundamentals

### Basic Async Pattern

```csharp
// ✅ Correct async method signature
public async Task<Data> LoadDataAsync()
{
    var response = await _httpClient.GetAsync("https://api.example.com/data");
    var content = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<Data>(content)!;
}

// ❌ Avoid async void (except event handlers)
public async void LoadData() { } // NO!

// ✅ Event handlers are the exception
private async void OnButtonClick(object sender, RoutedEventArgs e)
{
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Button click handler failed");
    }
}
```

---

## Cancellation

### CancellationToken Pattern

```csharp
public class DataService : IDisposable
{
    private CancellationTokenSource? _cts;
    
    public async Task<Data> LoadDataAsync(CancellationToken cancellationToken = default)
    {
        // Create linked token for widget shutdown + caller cancellation
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _cts?.Token ?? CancellationToken.None);
        
        try
        {
            var response = await _httpClient.GetAsync(
                "https://api.example.com/data",
                linkedCts.Token);
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            return JsonSerializer.Deserialize<Data>(content)!;
        }
        catch (OperationCanceledException)
        {
            _log.Debug("Data load cancelled");
            throw;
        }
    }
    
    public void CancelPendingOperations()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
    }
    
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### Cancellation in ViewModel

```csharp
public partial class MainViewModel : ObservableObject, IDisposable
{
    private CancellationTokenSource? _loadCts;
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Cancel any existing load operation
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        
        IsLoading = true;
        
        try
        {
            Data = await _dataService.LoadDataAsync(_loadCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load data");
            ErrorMessage = "Failed to load data";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    public void Dispose()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
    }
}
```

---

## Progress Reporting

### IProgress<T> Pattern

```csharp
public async Task ProcessItemsAsync(
    IEnumerable<Item> items,
    IProgress<ProgressInfo>? progress = null,
    CancellationToken cancellationToken = default)
{
    var itemList = items.ToList();
    var total = itemList.Count;
    
    for (int i = 0; i < total; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await ProcessItemAsync(itemList[i], cancellationToken);
        
        progress?.Report(new ProgressInfo
        {
            Current = i + 1,
            Total = total,
            Message = $"Processing {itemList[i].Name}..."
        });
    }
}

public class ProgressInfo
{
    public int Current { get; init; }
    public int Total { get; init; }
    public string Message { get; init; } = "";
    public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
}
```

### Using in ViewModel

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private double _progress;
    
    [ObservableProperty]
    private string _progressMessage = "";
    
    [RelayCommand]
    private async Task ImportItemsAsync(IEnumerable<Item> items)
    {
        var progressHandler = new Progress<ProgressInfo>(info =>
        {
            Progress = info.Percentage;
            ProgressMessage = info.Message;
        });
        
        await _service.ProcessItemsAsync(items, progressHandler, _cts.Token);
    }
}
```

---

## Parallel Operations

### Controlled Parallelism

```csharp
public async Task<IEnumerable<Result>> ProcessBatchAsync(
    IEnumerable<Item> items,
    CancellationToken cancellationToken = default)
{
    // Limit concurrent operations
    var semaphore = new SemaphoreSlim(4); // Max 4 concurrent
    var results = new ConcurrentBag<Result>();
    
    var tasks = items.Select(async item =>
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var result = await ProcessItemAsync(item, cancellationToken);
            results.Add(result);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
    return results;
}
```

### Parallel.ForEachAsync (.NET 6+)

```csharp
public async Task ProcessAllAsync(
    IEnumerable<Item> items,
    CancellationToken cancellationToken = default)
{
    await Parallel.ForEachAsync(
        items,
        new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cancellationToken
        },
        async (item, token) =>
        {
            await ProcessItemAsync(item, token);
        });
}
```

---

## Timeout Handling

```csharp
public async Task<Data> FetchWithTimeoutAsync(TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);
    
    try
    {
        return await _dataService.LoadDataAsync(cts.Token);
    }
    catch (OperationCanceledException) when (cts.IsCancellationRequested)
    {
        throw new TimeoutException($"Operation timed out after {timeout}");
    }
}

// Alternative with Task.WhenAny
public async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout)
{
    var delayTask = Task.Delay(timeout);
    var completedTask = await Task.WhenAny(task, delayTask);
    
    if (completedTask == delayTask)
    {
        throw new TimeoutException();
    }
    
    return await task;
}
```

---

## Fire-and-Forget Pattern

```csharp
public static class TaskExtensions
{
    /// <summary>
    /// Executes a task without waiting, with proper exception handling.
    /// </summary>
    public static void FireAndForget(
        this Task task,
        ILogger? log = null,
        Action<Exception>? errorHandler = null)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                log?.Error(t.Exception, "Fire-and-forget task failed");
                errorHandler?.Invoke(t.Exception);
            }
        }, TaskScheduler.Default);
    }
}

// Usage
public void OnInitialize()
{
    // Start background task, don't block initialization
    PreloadDataAsync().FireAndForget(_log);
}
```

---

## Async Initialization

```csharp
public class AsyncInitializedService
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private Task? _initTask;
    private bool _isInitialized;
    
    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;
        
        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;
            
            _initTask ??= InitializeAsync();
            await _initTask;
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
    
    private async Task InitializeAsync()
    {
        // Heavy initialization
        await Task.Delay(100);
    }
}

// Usage
public async Task<Data> GetDataAsync()
{
    await _service.EnsureInitializedAsync();
    return await _service.LoadDataAsync();
}
```

---

## UI Thread Synchronization

```csharp
public class UiSafeService
{
    private readonly Dispatcher _dispatcher;
    
    public UiSafeService()
    {
        _dispatcher = Application.Current.Dispatcher;
    }
    
    public async Task UpdateUiAsync(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            await _dispatcher.InvokeAsync(action);
        }
    }
    
    public void UpdateUi(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}
```

---

## Common Pitfalls

### ❌ Deadlock with .Result

```csharp
// ❌ WRONG - Can cause deadlock on UI thread
public void LoadDataSync()
{
    var data = LoadDataAsync().Result; // DEADLOCK!
}

// ✅ CORRECT - Use async all the way
public async Task LoadDataAsync()
{
    var data = await LoadDataAsync();
}
```

### ❌ Missing ConfigureAwait

```csharp
// In library/service code, use ConfigureAwait(false)
public async Task<Data> FetchAsync()
{
    var response = await _http.GetAsync(url).ConfigureAwait(false);
    return await ParseAsync(response).ConfigureAwait(false);
}

// In ViewModel/UI code, don't use ConfigureAwait (need UI context)
public async Task LoadAsync()
{
    Data = await _service.FetchAsync(); // No ConfigureAwait
    IsLoading = false; // Must run on UI thread
}
```

---

## Best Practices

1. **Async all the way** - Don't mix sync and async
2. **Always use cancellation** - Support graceful shutdown
3. **ConfigureAwait(false)** - In library code
4. **Handle exceptions** - async void must try/catch
5. **Avoid .Result/.Wait()** - Causes deadlocks

---

## Related Skills

- [optimization.md](optimization.md) - Performance patterns
- [error-handling.md](../quality/error-handling.md) - Exception handling

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added parallel patterns |
| 1.0.0 | 2025-06-01 | Initial version |
