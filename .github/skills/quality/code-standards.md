# Code Standards

> **Category:** Quality | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers coding standards and conventions for widget development, including naming, organization, documentation, and style guidelines.

---

## Project Structure

```
3SC.Widgets.MyWidget/
├── manifest.json              # Widget manifest (required)
├── 3SC.Widgets.MyWidget.csproj
├── MyWidget.cs                # IExternalWidget implementation
├── MyWidgetFactory.cs         # IWidgetFactory implementation
├── ViewModels/
│   ├── MainViewModel.cs       # Primary view model
│   └── SettingsViewModel.cs   # Settings view model
├── Views/
│   ├── MainView.xaml          # Main widget UI
│   ├── MainView.xaml.cs
│   ├── SettingsWindow.xaml    # Settings dialog
│   └── SettingsWindow.xaml.cs
├── Services/
│   ├── SettingsService.cs     # Settings persistence
│   └── DataService.cs         # Data operations
├── Models/
│   ├── WidgetSettings.cs      # Settings model
│   └── DataModels.cs          # Domain models
├── Converters/
│   └── ValueConverters.cs     # XAML converters
├── Helpers/
│   └── Extensions.cs          # Extension methods
├── Assets/
│   ├── icon.png               # Widget icon
│   └── preview.png            # Preview image
└── Styles/
    └── Styles.xaml            # Resource dictionary
```

---

## Naming Conventions

### Files & Classes

| Item | Convention | Example |
|------|------------|---------|
| Project | `3SC.Widgets.{Name}` | `3SC.Widgets.Clock` |
| Namespace | `_3SC.Widgets.{Name}` | `_3SC.Widgets.Clock` |
| Interface | `I{Name}` | `IDataService` |
| ViewModel | `{Feature}ViewModel` | `MainViewModel` |
| View | `{Feature}View` | `MainView` |
| Service | `{Purpose}Service` | `SettingsService` |
| Converter | `{Type}Converter` | `BoolToVisibilityConverter` |
| Exception | `{Type}Exception` | `SettingsException` |

### Members

```csharp
// Private fields: _camelCase
private readonly ILogger _log;
private string _title;

// Properties: PascalCase
public string Title { get; set; }
public bool IsLoading { get; private set; }

// Methods: PascalCase
public async Task LoadDataAsync()
public void HandleSettingsChanged()

// Parameters & locals: camelCase
public void SetTitle(string newTitle)
{
    var oldTitle = _title;
    _title = newTitle;
}

// Constants: PascalCase
public const int MaxRetries = 3;
public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
```

### Async Methods

```csharp
// Suffix with Async
public async Task<Data> LoadDataAsync()
public async Task SaveSettingsAsync(Settings settings)

// Cancellation token should be last parameter
public async Task<Data> FetchAsync(string url, CancellationToken cancellationToken = default)
```

---

## Code Organization

### Class Structure

```csharp
public partial class MainViewModel : ObservableObject
{
    // 1. Constants
    private const int RefreshIntervalSeconds = 60;
    
    // 2. Static fields/properties
    private static readonly JsonSerializerOptions JsonOptions = new();
    
    // 3. Instance fields (readonly first)
    private readonly ILogger _log;
    private readonly ISettingsService _settingsService;
    private string _title = "";
    
    // 4. Constructors
    public MainViewModel(ILogger log, ISettingsService settingsService)
    {
        _log = log;
        _settingsService = settingsService;
    }
    
    // 5. Properties
    [ObservableProperty]
    private string _title = "";
    
    public bool HasData => Data?.Any() == true;
    
    // 6. Commands
    [RelayCommand]
    private async Task RefreshAsync()
    {
        // ...
    }
    
    // 7. Public methods
    public void Initialize()
    {
        // ...
    }
    
    // 8. Private methods
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        // ...
    }
}
```

### Using Directives

```csharp
// System namespaces first
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Third-party namespaces
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

// Project namespaces
using _3SC.Widgets.MyWidget.Models;
using _3SC.Widgets.MyWidget.Services;
```

---

## XAML Standards

### Resource Naming

```xml
<!-- Colors: {Scope}{Purpose}Color -->
<Color x:Key="PrimaryBackgroundColor">#1A1A2E</Color>
<Color x:Key="AccentTextColor">#4A9EFF</Color>

<!-- Brushes: {Scope}{Purpose}Brush -->
<SolidColorBrush x:Key="PrimaryBackgroundBrush" Color="{StaticResource PrimaryBackgroundColor}"/>

<!-- Styles: {ControlType}Style or {Purpose}{ControlType}Style -->
<Style x:Key="TitleTextBlockStyle" TargetType="TextBlock">
<Style x:Key="IconButtonStyle" TargetType="Button">
```

### Element Order in XAML

```xml
<Button 
    x:Name="RefreshButton"
    x:Key="MyButton"
    Grid.Row="0"
    Grid.Column="1"
    Width="100"
    Height="32"
    Margin="5"
    Padding="10,5"
    HorizontalAlignment="Center"
    VerticalAlignment="Top"
    Background="{StaticResource AccentBrush}"
    Foreground="White"
    Content="Refresh"
    Command="{Binding RefreshCommand}"
    Style="{StaticResource PrimaryButtonStyle}"
    ToolTip="Click to refresh">
    <Button.Resources>
        <!-- Resources -->
    </Button.Resources>
</Button>
```

Order: Name → Key → Layout (Grid.*) → Size → Position → Appearance → Content → Behavior → Style

---

## Documentation

### XML Documentation

```csharp
/// <summary>
/// Manages widget settings persistence.
/// </summary>
/// <typeparam name="T">The settings type to manage.</typeparam>
public class SettingsService<T> where T : class, new()
{
    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    /// <returns>
    /// The loaded settings, or default values if file doesn't exist.
    /// </returns>
    /// <exception cref="SettingsException">
    /// Thrown when settings file is corrupted.
    /// </exception>
    public T Load()
    {
        // ...
    }
    
    /// <summary>
    /// Saves settings to disk with automatic backup.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="settings"/> is null.
    /// </exception>
    public void Save(T settings)
    {
        // ...
    }
}
```

### Inline Comments

```csharp
// Use sparingly - code should be self-documenting

// ✅ Good: Explains WHY
// Cache timeout is 5 minutes because API has rate limits
private static readonly TimeSpan CacheTimeout = TimeSpan.FromMinutes(5);

// ❌ Bad: Explains WHAT (obvious from code)
// Set cache timeout to 5 minutes
private static readonly TimeSpan CacheTimeout = TimeSpan.FromMinutes(5);
```

---

## MVVM Guidelines

### ViewModel Responsibilities

```csharp
// ✅ ViewModel SHOULD:
// - Expose data for binding
// - Handle commands
// - Coordinate services
// - Manage UI state (loading, errors)

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            Data = await _dataService.LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load data";
            _log.Error(ex, "LoadData failed");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### View Code-Behind

```csharp
// ✅ View code-behind SHOULD:
// - Handle view-specific events (Loaded, Closing)
// - Perform visual-only operations
// - Set DataContext if not using DI

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Focus first element, start animations, etc.
        SearchBox.Focus();
    }
}

// ❌ View code-behind SHOULD NOT:
// - Contain business logic
// - Access services directly
// - Manipulate data
```

---

## Error Prevention

### Null Safety

```csharp
// Use nullable reference types
public string? OptionalValue { get; set; }
public string RequiredValue { get; set; } = "";

// Guard clauses
public void Process(Data data)
{
    ArgumentNullException.ThrowIfNull(data);
    
    if (string.IsNullOrEmpty(data.Name))
        throw new ArgumentException("Name is required", nameof(data));
    
    // ... process data
}

// Null-conditional operators
var length = text?.Length ?? 0;
var name = person?.Address?.City ?? "Unknown";
```

### Disposal

```csharp
public class MyService : IDisposable
{
    private readonly Timer _timer;
    private bool _disposed;
    
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
            _timer?.Dispose();
        }
        
        _disposed = true;
    }
}
```

---

## Code Analysis

### .editorconfig

```ini
[*.cs]
# Naming rules
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = camel_case_underscore
dotnet_naming_rule.private_fields_should_be_camel_case.severity = warning

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private
dotnet_naming_style.camel_case_underscore.required_prefix = _
dotnet_naming_style.camel_case_underscore.capitalization = camel_case

# Code style
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_expression_bodied_methods = when_on_single_line:suggestion
csharp_prefer_braces = true:warning

# Formatting
indent_size = 4
end_of_line = crlf
insert_final_newline = true
```

---

## Best Practices Checklist

- [ ] Follow naming conventions consistently
- [ ] Organize code with clear structure
- [ ] Document public APIs with XML comments
- [ ] Use nullable reference types
- [ ] Implement IDisposable where needed
- [ ] Avoid magic numbers - use named constants
- [ ] Keep methods small and focused
- [ ] Prefer composition over inheritance

---

## Related Skills

- [mvvm-patterns.md](../ui/mvvm-patterns.md) - MVVM implementation
- [testing-strategies.md](testing-strategies.md) - Testing conventions

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added .editorconfig |
| 1.0.0 | 2025-06-01 | Initial version |
