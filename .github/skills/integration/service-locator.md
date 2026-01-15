# Service Locator Pattern

> **Category:** Integration | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

The Service Locator pattern provides a central registry for services, enabling dependency injection without constructor injection in scenarios where it's not practical (like XAML-instantiated objects).

---

## Basic Service Locator

```csharp
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();
    private static readonly Dictionary<Type, Func<object>> _factories = new();
    private static readonly object _lock = new();
    
    /// <summary>
    /// Register a singleton service instance.
    /// </summary>
    public static void Register<T>(T instance) where T : class
    {
        lock (_lock)
        {
            _services[typeof(T)] = instance;
        }
    }
    
    /// <summary>
    /// Register a factory for lazy instantiation.
    /// </summary>
    public static void RegisterFactory<T>(Func<T> factory) where T : class
    {
        lock (_lock)
        {
            _factories[typeof(T)] = () => factory();
        }
    }
    
    /// <summary>
    /// Resolve a service instance.
    /// </summary>
    public static T Resolve<T>() where T : class
    {
        lock (_lock)
        {
            // Check for existing instance
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            // Check for factory
            if (_factories.TryGetValue(typeof(T), out var factory))
            {
                var instance = (T)factory();
                _services[typeof(T)] = instance; // Cache for singleton behavior
                return instance;
            }
            
            throw new InvalidOperationException(
                $"Service {typeof(T).Name} is not registered");
        }
    }
    
    /// <summary>
    /// Try to resolve a service, returning null if not found.
    /// </summary>
    public static T? TryResolve<T>() where T : class
    {
        lock (_lock)
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            if (_factories.TryGetValue(typeof(T), out var factory))
            {
                var instance = (T)factory();
                _services[typeof(T)] = instance;
                return instance;
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// Clear all registrations (for testing).
    /// </summary>
    public static void Clear()
    {
        lock (_lock)
        {
            _services.Clear();
            _factories.Clear();
        }
    }
}
```

---

## Widget Bootstrap Registration

```csharp
public class WidgetFactory : IWidgetFactory
{
    public IWidget Create()
    {
        // Register services on widget creation
        ConfigureServices();
        return new MyWidget();
    }
    
    private static void ConfigureServices()
    {
        // Logger (always register first)
        var log = CreateLogger();
        ServiceLocator.Register<ILogger>(log);
        
        // Settings service
        ServiceLocator.RegisterFactory(() => 
            new SettingsService<MySettings>(log));
        
        // Data services
        ServiceLocator.RegisterFactory(() => 
            new DataService(log));
        
        // UI services
        ServiceLocator.RegisterFactory(() => 
            new DialogService());
    }
    
    private static ILogger CreateLogger()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Widgets", "Community", "my-widget", "logs", "widget-.log");
        
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
```

---

## Usage in ViewModels

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger _log;
    private readonly SettingsService<MySettings> _settings;
    
    public MainViewModel()
    {
        // Resolve from service locator
        _log = ServiceLocator.Resolve<ILogger>();
        _settings = ServiceLocator.Resolve<SettingsService<MySettings>>();
        
        _log.Information("MainViewModel initialized");
    }
}
```

---

## Usage in XAML Code-Behind

```csharp
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        
        // ViewModel created via service locator
        DataContext = new SettingsViewModel(
            ServiceLocator.Resolve<ILogger>(),
            ServiceLocator.Resolve<SettingsService<MySettings>>());
    }
}
```

---

## Interface-Based Registration

```csharp
// Define interfaces for testability
public interface IDataService
{
    Task<List<Item>> GetItemsAsync();
    Task SaveItemAsync(Item item);
}

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string message);
    Task ShowErrorAsync(string message);
}

// Register with interfaces
ServiceLocator.Register<IDataService>(new DataService(log));
ServiceLocator.Register<IDialogService>(new DialogService());

// Resolve by interface
var data = ServiceLocator.Resolve<IDataService>();
var dialogs = ServiceLocator.Resolve<IDialogService>();
```

---

## Scoped Services (Advanced)

```csharp
public static class ServiceLocator
{
    private static readonly AsyncLocal<Dictionary<Type, object>> _scopedServices = new();
    
    public static IDisposable CreateScope()
    {
        _scopedServices.Value = new Dictionary<Type, object>();
        return new ServiceScope();
    }
    
    public static void RegisterScoped<T>(T instance) where T : class
    {
        if (_scopedServices.Value == null)
            throw new InvalidOperationException("No active scope");
        
        _scopedServices.Value[typeof(T)] = instance;
    }
    
    public static T ResolveScoped<T>() where T : class
    {
        if (_scopedServices.Value?.TryGetValue(typeof(T), out var service) == true)
        {
            return (T)service;
        }
        
        // Fall back to singleton
        return Resolve<T>();
    }
    
    private class ServiceScope : IDisposable
    {
        public void Dispose()
        {
            _scopedServices.Value = null!;
        }
    }
}

// Usage
using (ServiceLocator.CreateScope())
{
    ServiceLocator.RegisterScoped(new RequestContext());
    // Services resolved in this scope get this context
}
```

---

## Testing with Service Locator

```csharp
public class ViewModelTests : IDisposable
{
    public ViewModelTests()
    {
        // Clear before each test
        ServiceLocator.Clear();
        
        // Register mocks
        ServiceLocator.Register<ILogger>(new LoggerConfiguration().CreateLogger());
        ServiceLocator.Register<IDataService>(new MockDataService());
    }
    
    [Fact]
    public void ViewModel_LoadsData()
    {
        var vm = new MainViewModel();
        // Test with mock services
    }
    
    public void Dispose()
    {
        ServiceLocator.Clear();
    }
}
```

---

## Best Practices

1. **Register early** - Configure services during widget creation
2. **Use interfaces** - Enable testing with mocks
3. **Single responsibility** - Each service does one thing
4. **Lazy factories** - Use factories for expensive services
5. **Clear in tests** - Always clean up between tests

---

## Related Skills

- [contracts-interfaces.md](../core/contracts-interfaces.md) - Widget interfaces
- [testing-strategies.md](../quality/testing-strategies.md) - Testing patterns

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added scoped services |
| 1.0.0 | 2025-06-01 | Initial version |
