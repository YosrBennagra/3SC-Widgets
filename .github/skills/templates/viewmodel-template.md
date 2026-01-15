# ViewModel Template

> **Category:** Templates | **Priority:** 🔴 Essential
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Complete ViewModel templates using CommunityToolkit.MVVM 8.2.2 patterns.

---

## Basic ViewModel Template

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace MyWidget.ViewModels;

/// <summary>
/// ViewModel for [Description]
/// </summary>
public partial class MyViewModel : ObservableObject
{
    private readonly ILogger _log;
    
    #region Observable Properties
    
    [ObservableProperty]
    private string _title = "Default Title";
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    #endregion
    
    #region Constructor
    
    public MyViewModel(ILogger log)
    {
        _log = log.ForContext<MyViewModel>();
        _log.Debug("MyViewModel created");
    }
    
    #endregion
    
    #region Commands
    
    [RelayCommand]
    private void DoSomething()
    {
        _log.Debug("DoSomething executed");
        // Implementation
    }
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsLoading) return;
        
        IsLoading = true;
        ErrorMessage = null;
        
        try
        {
            _log.Debug("Loading data...");
            // await _dataService.LoadAsync();
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
    
    #endregion
}
```

---

## ViewModel with Settings Template

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace MyWidget.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger _log;
    private readonly SettingsService<MySettings> _settings;
    
    #region Settings Properties
    
    [ObservableProperty]
    private string _displayName = "";
    
    [ObservableProperty]
    private int _refreshInterval = 30;
    
    [ObservableProperty]
    private bool _autoStart;
    
    #endregion
    
    #region UI State
    
    [ObservableProperty]
    private bool _hasChanges;
    
    [ObservableProperty]
    private bool _isSaving;
    
    #endregion
    
    public SettingsViewModel(ILogger log, SettingsService<MySettings> settings)
    {
        _log = log.ForContext<SettingsViewModel>();
        _settings = settings;
        
        LoadFromSettings();
    }
    
    private void LoadFromSettings()
    {
        var s = _settings.Current;
        DisplayName = s.DisplayName;
        RefreshInterval = s.RefreshInterval;
        AutoStart = s.AutoStart;
        HasChanges = false;
    }
    
    partial void OnDisplayNameChanged(string value) => HasChanges = true;
    partial void OnRefreshIntervalChanged(int value) => HasChanges = true;
    partial void OnAutoStartChanged(bool value) => HasChanges = true;
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        IsSaving = true;
        
        try
        {
            _settings.Current.DisplayName = DisplayName;
            _settings.Current.RefreshInterval = RefreshInterval;
            _settings.Current.AutoStart = AutoStart;
            
            await _settings.SaveAsync();
            HasChanges = false;
            
            _log.Information("Settings saved");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save settings");
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    [RelayCommand]
    private void Reset()
    {
        LoadFromSettings();
    }
}
```

---

## ViewModel with Collection Template

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace MyWidget.ViewModels;

public partial class ListViewModel : ObservableObject
{
    private readonly ILogger _log;
    private readonly IDataService _dataService;
    
    #region Collections
    
    public ObservableCollection<ItemViewModel> Items { get; } = new();
    
    [ObservableProperty]
    private ItemViewModel? _selectedItem;
    
    #endregion
    
    #region Filtering
    
    [ObservableProperty]
    private string _searchText = "";
    
    partial void OnSearchTextChanged(string value)
    {
        FilterItems();
    }
    
    #endregion
    
    #region State
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isEmpty;
    
    #endregion
    
    public ListViewModel(ILogger log, IDataService dataService)
    {
        _log = log.ForContext<ListViewModel>();
        _dataService = dataService;
    }
    
    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        IsLoading = true;
        
        try
        {
            Items.Clear();
            
            var data = await _dataService.GetItemsAsync();
            foreach (var item in data)
            {
                Items.Add(new ItemViewModel(item));
            }
            
            IsEmpty = Items.Count == 0;
            _log.Debug("Loaded {Count} items", Items.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load items");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void AddItem()
    {
        var newItem = new ItemViewModel(new Item { Name = "New Item" });
        Items.Add(newItem);
        SelectedItem = newItem;
        IsEmpty = false;
    }
    
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedItem == null) return;
        
        try
        {
            await _dataService.DeleteAsync(SelectedItem.Model);
            Items.Remove(SelectedItem);
            SelectedItem = null;
            IsEmpty = Items.Count == 0;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to delete item");
        }
    }
    
    private void FilterItems()
    {
        // Implement filtering logic
    }
}

public partial class ItemViewModel : ObservableObject
{
    public Item Model { get; }
    
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private bool _isSelected;
    
    public ItemViewModel(Item model)
    {
        Model = model;
        _name = model.Name;
    }
}
```

---

## ViewModel with Async Initialization

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace MyWidget.ViewModels;

public partial class AsyncInitViewModel : ObservableObject
{
    private readonly ILogger _log;
    private bool _isInitialized;
    
    [ObservableProperty]
    private bool _isInitializing = true;
    
    [ObservableProperty]
    private string? _initError;
    
    public AsyncInitViewModel(ILogger log)
    {
        _log = log;
    }
    
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_isInitialized) return;
        
        IsInitializing = true;
        InitError = null;
        
        try
        {
            _log.Debug("Initializing...");
            
            // Load initial data
            await Task.WhenAll(
                LoadConfigAsync(ct),
                LoadDataAsync(ct)
            );
            
            _isInitialized = true;
            _log.Debug("Initialization complete");
        }
        catch (OperationCanceledException)
        {
            _log.Debug("Initialization cancelled");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Initialization failed");
            InitError = "Failed to initialize";
        }
        finally
        {
            IsInitializing = false;
        }
    }
    
    private async Task LoadConfigAsync(CancellationToken ct)
    {
        await Task.Delay(100, ct); // Simulated
    }
    
    private async Task LoadDataAsync(CancellationToken ct)
    {
        await Task.Delay(100, ct); // Simulated
    }
}
```

---

## ViewModel with Disposable Pattern

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace MyWidget.ViewModels;

public partial class DisposableViewModel : ObservableObject, IDisposable
{
    private readonly ILogger _log;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<IDisposable> _subscriptions = new();
    private bool _disposed;
    
    public DisposableViewModel(ILogger log)
    {
        _log = log;
    }
    
    protected void AddSubscription(IDisposable subscription)
    {
        _subscriptions.Add(subscription);
    }
    
    protected CancellationToken CancellationToken => _cts.Token;
    
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
            _log.Debug("Disposing {Type}", GetType().Name);
            
            // Cancel async operations
            _cts.Cancel();
            _cts.Dispose();
            
            // Dispose subscriptions
            foreach (var sub in _subscriptions)
            {
                sub.Dispose();
            }
            _subscriptions.Clear();
        }
        
        _disposed = true;
    }
}
```

---

## ViewModel Naming Conventions

| ViewModel | Purpose |
|-----------|---------|
| `MainViewModel` | Primary widget view |
| `SettingsViewModel` | Settings window |
| `ItemViewModel` | Individual list item |
| `{Feature}ViewModel` | Feature-specific view |
| `{Dialog}DialogViewModel` | Dialog windows |

---

## Related Skills

- [mvvm-patterns.md](../ui/mvvm-patterns.md) - MVVM fundamentals
- [settings-management.md](../data/settings-management.md) - Settings service

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added disposable pattern |
| 1.0.0 | 2025-06-01 | Initial version |
