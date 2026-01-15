# MVVM Patterns

> **Category:** UI | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers MVVM (Model-View-ViewModel) implementation using CommunityToolkit.MVVM for 3SC widget development.

## Prerequisites

- [widget-architecture.md](../core/widget-architecture.md)
- [project-setup.md](../core/project-setup.md)

---

## CommunityToolkit.MVVM

We use CommunityToolkit.MVVM 8.2.2 for:
- Source generators (less boilerplate)
- `ObservableObject` base class
- `RelayCommand` / `AsyncRelayCommand`
- `ObservableProperty` attribute

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

---

## Complete ViewModel Template

```csharp
using System.ComponentModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.MyWidget;

/// <summary>
/// ViewModel for MyWidget with settings, commands, and lifecycle management.
/// </summary>
public partial class MyWidgetViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly string _settingsPath;
    private CancellationTokenSource? _cts;
    private bool _isDisposed;
    
    #endregion

    #region Observable Properties
    
    /// <summary>
    /// Main display text. Updates UI automatically.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTextFormatted))]
    private string _displayText = "Hello Widget";
    
    /// <summary>
    /// Indicates if widget is currently loading.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool _isLoading;
    
    /// <summary>
    /// Error message to display, if any.
    /// </summary>
    [ObservableProperty]
    private string? _errorMessage;
    
    /// <summary>
    /// Settings: Whether to show seconds.
    /// </summary>
    [ObservableProperty]
    private bool _showDetails = true;
    
    #endregion

    #region Computed Properties
    
    /// <summary>
    /// Formatted display text (computed from DisplayText).
    /// </summary>
    public string DisplayTextFormatted => 
        string.IsNullOrEmpty(DisplayText) ? "(empty)" : DisplayText.ToUpperInvariant();
    
    /// <summary>
    /// Whether the refresh command can execute.
    /// </summary>
    public bool CanRefresh => !IsLoading;
    
    #endregion

    #region Constructor
    
    public MyWidgetViewModel()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Widgets", "my-widget", "settings.json");
    }
    
    #endregion

    #region Lifecycle Methods
    
    /// <summary>
    /// Initialize the ViewModel. Call from IWidget.OnInitialize().
    /// </summary>
    public void Initialize()
    {
        LoadSettings();
        _ = RefreshAsync(); // Fire and forget
    }
    
    /// <summary>
    /// Clean up resources. Call from IWidget.OnDispose().
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        _cts?.Cancel();
        _cts?.Dispose();
        
        SaveSettings();
        
        Log.Debug("MyWidgetViewModel disposed");
    }
    
    #endregion

    #region Commands
    
    /// <summary>
    /// Refresh data command.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (IsLoading) return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            // Simulate async work
            await Task.Delay(1000, _cts.Token);
            
            DisplayText = $"Refreshed at {DateTime.Now:HH:mm:ss}";
            
            Log.Information("Widget refreshed successfully");
        }
        catch (OperationCanceledException)
        {
            Log.Debug("Refresh cancelled");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Refresh failed");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Toggle details visibility command.
    /// </summary>
    [RelayCommand]
    private void ToggleDetails()
    {
        ShowDetails = !ShowDetails;
        SaveSettings();
    }
    
    /// <summary>
    /// Open settings command.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        // Navigate to settings or open dialog
        Log.Debug("Opening settings");
    }
    
    /// <summary>
    /// Close widget command.
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        // Request widget close
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
    #endregion

    #region Settings
    
    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                Log.Debug("No settings file found, using defaults");
                return;
            }
            
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<MyWidgetSettings>(json);
            
            if (settings != null)
            {
                ShowDetails = settings.ShowDetails;
                DisplayText = settings.LastDisplayText ?? DisplayText;
            }
            
            Log.Debug("Settings loaded from {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load settings");
        }
    }
    
    private void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var settings = new MyWidgetSettings
            {
                ShowDetails = ShowDetails,
                LastDisplayText = DisplayText
            };
            
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(_settingsPath, json);
            
            Log.Debug("Settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save settings");
        }
    }
    
    #endregion

    #region Events
    
    /// <summary>
    /// Raised when the widget requests to be closed.
    /// </summary>
    public event EventHandler? CloseRequested;
    
    #endregion
}

/// <summary>
/// Settings data class.
/// </summary>
public class MyWidgetSettings
{
    public bool ShowDetails { get; set; } = true;
    public string? LastDisplayText { get; set; }
}
```

---

## ObservableProperty Patterns

### Basic Property

```csharp
// Generates: public string Title { get; set; }
// With INotifyPropertyChanged
[ObservableProperty]
private string _title = "Default";
```

### Property with Dependent Notification

```csharp
// When Count changes, also notify TotalText changed
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(TotalText))]
private int _count;

public string TotalText => $"Total: {Count}";
```

### Property Affecting Command State

```csharp
// When IsBusy changes, re-evaluate SaveCommand.CanExecute
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private bool _isBusy;

[RelayCommand(CanExecute = nameof(CanSave))]
private void Save() { }

private bool CanSave => !IsBusy;
```

### Property with Custom Setter Logic

```csharp
[ObservableProperty]
private string _searchText = "";

// Called when SearchText changes
partial void OnSearchTextChanged(string value)
{
    // Filter results
    FilterResults(value);
}

// Called before SearchText changes (can cancel)
partial void OnSearchTextChanging(string value)
{
    Log.Debug("Search changing to: {Value}", value);
}
```

---

## RelayCommand Patterns

### Synchronous Command

```csharp
[RelayCommand]
private void DoSomething()
{
    // Synchronous action
}

// In XAML: Command="{Binding DoSomethingCommand}"
```

### Asynchronous Command

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    await Task.Delay(1000);
}

// Automatically handles:
// - IsRunning property
// - Exception handling
// - Cancellation support
```

### Command with Parameter

```csharp
[RelayCommand]
private void SelectItem(string itemId)
{
    SelectedId = itemId;
}

// In XAML: Command="{Binding SelectItemCommand}" 
//          CommandParameter="{Binding Id}"
```

### Command with CanExecute

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private void Save()
{
    // Save logic
}

private bool CanSave => !string.IsNullOrEmpty(Text) && !IsBusy;

// Remember to notify when CanSave might change:
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private string _text = "";

[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private bool _isBusy;
```

### Async Command with Cancellation

```csharp
[RelayCommand(IncludeCancelCommand = true)]
private async Task LongOperationAsync(CancellationToken token)
{
    for (int i = 0; i < 100; i++)
    {
        token.ThrowIfCancellationRequested();
        await Task.Delay(100, token);
        Progress = i;
    }
}

// In XAML:
// <Button Command="{Binding LongOperationCommand}" Content="Start"/>
// <Button Command="{Binding LongOperationCancelCommand}" Content="Cancel"/>
```

---

## Data Binding Patterns

### Basic Binding

```xml
<!-- One-way (default for most controls) -->
<TextBlock Text="{Binding DisplayText}" />

<!-- Two-way (for editable controls) -->
<TextBox Text="{Binding EditableText, Mode=TwoWay}" />

<!-- Two-way with immediate update -->
<TextBox Text="{Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

### Command Binding

```xml
<!-- Simple command -->
<Button Command="{Binding RefreshCommand}" Content="Refresh" />

<!-- Command with parameter -->
<Button Command="{Binding SelectCommand}" 
        CommandParameter="{Binding Id}" 
        Content="Select" />

<!-- Async command with loading indicator -->
<Button Command="{Binding LoadCommand}">
    <StackPanel Orientation="Horizontal">
        <ProgressRing IsActive="{Binding LoadCommand.IsRunning}" 
                      Visibility="{Binding LoadCommand.IsRunning, 
                                   Converter={StaticResource BoolToVisibility}}" />
        <TextBlock Text="Load" />
    </StackPanel>
</Button>
```

### Visibility Binding

```xml
<!-- With converter -->
<TextBlock Text="{Binding ErrorMessage}" 
           Visibility="{Binding HasError, 
                        Converter={StaticResource BoolToVisibilityConverter}}" />

<!-- Direct enum binding -->
<TextBlock Visibility="{Binding IsVisible, 
                        Converter={StaticResource BoolToVisibilityConverter},
                        ConverterParameter=Hidden}" />
```

### Collection Binding

```xml
<ItemsControl ItemsSource="{Binding Items}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Background="{StaticResource CardBackground}">
                <TextBlock Text="{Binding Name}" />
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

---

## View-ViewModel Connection

### Option 1: Set in Code-Behind (Recommended)

```csharp
// Window.xaml.cs
public partial class MyWidgetWindow : Window
{
    public MyWidgetWindow()
    {
        InitializeComponent();
    }
    
    public void SetViewModel(MyWidgetViewModel viewModel)
    {
        DataContext = viewModel;
    }
}

// Widget class
public Window? CreateWindow()
{
    _viewModel = new MyWidgetViewModel();
    _window = new MyWidgetWindow();
    _window.SetViewModel(_viewModel);
    return _window;
}
```

### Option 2: Create in XAML

```xml
<Window.DataContext>
    <local:MyWidgetViewModel />
</Window.DataContext>
```

### Option 3: Design-Time Data

```xml
<Window xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance local:MyWidgetViewModel, IsDesignTimeCreatable=True}">
```

---

## Observable Collections

### Basic Usage

```csharp
using System.Collections.ObjectModel;

public partial class MyWidgetViewModel : ObservableObject
{
    public ObservableCollection<ItemModel> Items { get; } = new();
    
    private void LoadItems()
    {
        Items.Clear();
        foreach (var item in GetItems())
        {
            Items.Add(item);
        }
    }
}
```

### Thread-Safe Updates

```csharp
// UI updates must be on UI thread
private async Task LoadItemsAsync()
{
    var items = await Task.Run(() => FetchItems());
    
    // Use dispatcher for UI thread
    Application.Current.Dispatcher.Invoke(() =>
    {
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    });
}
```

---

## Common Converters

### BoolToVisibilityConverter

```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var invert = parameter?.ToString() == "Invert";
        
        if (invert) boolValue = !boolValue;
        
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}
```

### NullToVisibilityConverter

```csharp
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isNull = value == null || (value is string s && string.IsNullOrEmpty(s));
        var hideWhenNull = parameter?.ToString() != "Invert";
        
        return (isNull && hideWhenNull) || (!isNull && !hideWhenNull) 
            ? Visibility.Collapsed 
            : Visibility.Visible;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

---

## Best Practices

### 1. Keep ViewModels Testable

```csharp
// Good - injectable dependencies
public class MyWidgetViewModel
{
    private readonly ISettingsService _settings;
    
    public MyWidgetViewModel(ISettingsService settings)
    {
        _settings = settings;
    }
}

// Bad - hard dependencies
public class MyWidgetViewModel
{
    public MyWidgetViewModel()
    {
        var settings = new SettingsService(); // Hard to test
    }
}
```

### 2. Use Async Commands for I/O

```csharp
// Good
[RelayCommand]
private async Task SaveAsync()
{
    await File.WriteAllTextAsync(path, content);
}

// Bad - blocks UI
[RelayCommand]
private void Save()
{
    File.WriteAllText(path, content); // Blocks!
}
```

### 3. Handle Disposal

```csharp
public partial class MyWidgetViewModel : ObservableObject, IDisposable
{
    private bool _isDisposed;
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        // Clean up subscriptions, timers, etc.
    }
}
```

---

## Common Mistakes

### ❌ Forgetting to Notify Property Changes

```csharp
// Bad - UI won't update
private string _text;
public string Text
{
    get => _text;
    set => _text = value; // Missing notification!
}

// Good - use ObservableProperty
[ObservableProperty]
private string _text;
```

### ❌ Blocking UI Thread

```csharp
// Bad
[RelayCommand]
private void Load()
{
    var data = httpClient.GetAsync(url).Result; // Blocks UI!
}

// Good
[RelayCommand]
private async Task LoadAsync()
{
    var data = await httpClient.GetAsync(url);
}
```

---

## Related Skills

- [xaml-styling.md](xaml-styling.md) - XAML patterns
- [settings-management.md](../data/settings-management.md) - Settings
- [async-patterns.md](../performance/async-patterns.md) - Async best practices

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Complete rewrite with CommunityToolkit.MVVM |
| 1.0.0 | 2025-06-01 | Initial version |
