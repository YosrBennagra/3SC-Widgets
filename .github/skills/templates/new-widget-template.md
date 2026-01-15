# New Widget Template

> **Category:** Templates | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This template provides a complete, production-ready starting point for creating a new widget.

---

## Quick Start

1. Copy the template files below
2. Replace `MyWidget` with your widget name
3. Update `manifest.json` with your widget details
4. Build and test

---

## Project Structure

```
3SC.Widgets.MyWidget/
├── manifest.json
├── 3SC.Widgets.MyWidget.csproj
├── MyWidget.cs
├── MyWidgetFactory.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── MainView.xaml
│   ├── MainView.xaml.cs
│   ├── SettingsWindow.xaml
│   └── SettingsWindow.xaml.cs
├── Models/
│   └── WidgetSettings.cs
├── Services/
│   └── SettingsService.cs
└── Assets/
    └── icon.png
```

---

## manifest.json

```json
{
  "widgetKey": "my-widget",
  "displayName": "My Widget",
  "description": "A brief description of what the widget does",
  "version": "1.0.0",
  "author": {
    "name": "Your Name",
    "email": "your.email@example.com"
  },
  "entryPoint": "3SC.Widgets.MyWidget.dll",
  "factoryType": "_3SC.Widgets.MyWidget.MyWidgetFactory",
  "hasSettings": true,
  "category": "General",
  "minHostVersion": "3.0.0",
  "defaultSize": {
    "width": 300,
    "height": 200
  },
  "minSize": {
    "width": 150,
    "height": 100
  },
  "maxSize": {
    "width": 600,
    "height": 400
  }
}
```

---

## 3SC.Widgets.MyWidget.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <OutputType>Library</OutputType>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>_3SC.Widgets.MyWidget</RootNamespace>
    <AssemblyName>3SC.Widgets.MyWidget</AssemblyName>
    
    <!-- Version -->
    <Version>1.0.0</Version>
    <FileVersion>1.0.0.0</FileVersion>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    
    <!-- Metadata -->
    <Product>My Widget</Product>
    <Company>3SC</Company>
    <Authors>Your Name</Authors>
    <Description>A brief description</Description>
  </PropertyGroup>

  <!-- ⚠️ CRITICAL: Use exact versions -->
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\packages\3SC.Widgets.Contracts\3SC.Widgets.Contracts.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Include="manifest.json" CopyToOutputDirectory="PreserveNewest" />
    <None Include="Assets\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

---

## MyWidgetFactory.cs

```csharp
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.MyWidget;

/// <summary>
/// Factory for creating MyWidget instances.
/// </summary>
public class MyWidgetFactory : IWidgetFactory
{
    public IWidget Create()
    {
        return new MyWidget();
    }
}
```

---

## MyWidget.cs

```csharp
using System.Windows;
using _3SC.Widgets.Contracts;
using _3SC.Widgets.MyWidget.ViewModels;
using _3SC.Widgets.MyWidget.Views;
using Serilog;

namespace _3SC.Widgets.MyWidget;

/// <summary>
/// Main widget implementation.
/// </summary>
public class MyWidget : IExternalWidget
{
    private readonly ILogger _log;
    private MainView? _view;
    private MainViewModel? _viewModel;
    
    public string Key => "my-widget";
    public string Name => "My Widget";
    public bool HasSettings => true;
    public FrameworkElement Content => _view!;
    
    public MyWidget()
    {
        _log = CreateLogger();
    }
    
    public void OnInitialize()
    {
        _log.Information("Initializing widget");
        
        try
        {
            _viewModel = new MainViewModel(_log);
            _view = new MainView { DataContext = _viewModel };
            
            _viewModel.LoadSettings();
            
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
    }
    
    public void OnClose()
    {
        _log.Information("Closing widget");
        
        try
        {
            _viewModel?.SaveSettings();
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to save settings on close");
        }
        
        Log.CloseAndFlush();
    }
    
    public void ShowSettings()
    {
        if (_viewModel == null) return;
        
        var settingsWindow = new SettingsWindow(_viewModel.Settings.Clone());
        settingsWindow.Owner = Window.GetWindow(_view);
        
        if (settingsWindow.ShowDialog() == true)
        {
            _viewModel.ApplySettings(settingsWindow.Settings);
        }
    }
    
    private static ILogger CreateLogger()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", "my-widget", "logs", "widget-.log");
        
        return new LoggerConfiguration()
            .MinimumLevel.Information()
#if DEBUG
            .MinimumLevel.Debug()
            .WriteTo.Debug()
#endif
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();
    }
}
```

---

## ViewModels/MainViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using _3SC.Widgets.MyWidget.Models;
using _3SC.Widgets.MyWidget.Services;
using Serilog;

namespace _3SC.Widgets.MyWidget.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILogger _log;
    private readonly SettingsService _settingsService;
    
    [ObservableProperty]
    private string _title = "My Widget";
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public WidgetSettings Settings { get; private set; } = new();
    
    public MainViewModel(ILogger log)
    {
        _log = log.ForContext<MainViewModel>();
        _settingsService = new SettingsService("my-widget", log);
    }
    
    public void LoadSettings()
    {
        try
        {
            Settings = _settingsService.Load();
            ApplySettings(Settings);
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to load settings, using defaults");
            Settings = new WidgetSettings();
        }
    }
    
    public void SaveSettings()
    {
        try
        {
            _settingsService.Save(Settings);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save settings");
        }
    }
    
    public void ApplySettings(WidgetSettings settings)
    {
        Settings = settings;
        // Apply settings to UI
        OnPropertyChanged(nameof(Settings));
    }
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        
        try
        {
            // TODO: Implement refresh logic
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Refresh failed");
            ErrorMessage = "Failed to refresh";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## Models/WidgetSettings.cs

```csharp
using System.Text.Json.Serialization;

namespace _3SC.Widgets.MyWidget.Models;

public class WidgetSettings
{
    [JsonPropertyName("refreshInterval")]
    public int RefreshIntervalSeconds { get; set; } = 60;
    
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";
    
    [JsonPropertyName("showTitle")]
    public bool ShowTitle { get; set; } = true;
    
    public WidgetSettings Clone()
    {
        return new WidgetSettings
        {
            RefreshIntervalSeconds = RefreshIntervalSeconds,
            Theme = Theme,
            ShowTitle = ShowTitle
        };
    }
}
```

---

## Views/MainView.xaml

```xml
<UserControl x:Class="_3SC.Widgets.MyWidget.Views.MainView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:_3SC.Widgets.MyWidget.ViewModels"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
             d:DesignWidth="300" d:DesignHeight="200">
    
    <UserControl.Resources>
        <!-- Theme Colors (StaticResource for external widgets!) -->
        <Color x:Key="BackgroundColor">#1A1A2E</Color>
        <Color x:Key="SurfaceColor">#25253A</Color>
        <Color x:Key="TextColor">#E8E8E8</Color>
        <Color x:Key="TextSecondaryColor">#A0A0A0</Color>
        <Color x:Key="AccentColor">#4A9EFF</Color>
        
        <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
        <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
        <SolidColorBrush x:Key="TextBrush" Color="{StaticResource TextColor}"/>
        <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
        <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
        
        <BooleanToVisibilityConverter x:Key="BoolToVis"/>
    </UserControl.Resources>
    
    <Border Background="{StaticResource BackgroundBrush}" 
            CornerRadius="8"
            Padding="15">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>
            
            <!-- Header -->
            <TextBlock Grid.Row="0"
                       Text="{Binding Title}"
                       FontSize="18"
                       FontWeight="SemiBold"
                       Foreground="{StaticResource TextBrush}"
                       Margin="0,0,0,10"/>
            
            <!-- Content -->
            <Border Grid.Row="1"
                    Background="{StaticResource SurfaceBrush}"
                    CornerRadius="6"
                    Padding="10">
                
                <!-- Loading indicator -->
                <ProgressBar IsIndeterminate="True"
                             Visibility="{Binding IsLoading, Converter={StaticResource BoolToVis}}"
                             Height="4"
                             VerticalAlignment="Top"/>
                
                <!-- Main content goes here -->
                <TextBlock Text="Widget content"
                           Foreground="{StaticResource TextSecondaryBrush}"
                           VerticalAlignment="Center"
                           HorizontalAlignment="Center"/>
            </Border>
            
            <!-- Error display -->
            <Border Grid.Row="1"
                    Background="#40FF4444"
                    CornerRadius="4"
                    Padding="8"
                    VerticalAlignment="Bottom"
                    Visibility="{Binding ErrorMessage, Converter={StaticResource BoolToVis}}">
                <TextBlock Text="{Binding ErrorMessage}"
                           Foreground="#FFCCCC"
                           TextWrapping="Wrap"/>
            </Border>
        </Grid>
    </Border>
</UserControl>
```

---

## Views/MainView.xaml.cs

```csharp
using System.Windows.Controls;

namespace _3SC.Widgets.MyWidget.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
}
```

---

## Services/SettingsService.cs

```csharp
using System.Text.Json;
using _3SC.Widgets.MyWidget.Models;
using Serilog;

namespace _3SC.Widgets.MyWidget.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private readonly ILogger _log;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public SettingsService(string widgetKey, ILogger log)
    {
        _log = log.ForContext<SettingsService>();
        
        var dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", widgetKey);
        
        Directory.CreateDirectory(dataPath);
        _settingsPath = Path.Combine(dataPath, "settings.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public WidgetSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            _log.Debug("Settings file not found, returning defaults");
            return new WidgetSettings();
        }
        
        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<WidgetSettings>(json, _jsonOptions) 
                   ?? new WidgetSettings();
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to load settings from {Path}", _settingsPath);
            return new WidgetSettings();
        }
    }
    
    public void Save(WidgetSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(_settingsPath, json);
            _log.Debug("Settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save settings to {Path}", _settingsPath);
            throw;
        }
    }
}
```

---

## Usage

1. Create a new folder: `3SC.Widgets.MyWidget`
2. Copy all the files above
3. Replace `MyWidget` / `my-widget` with your widget name
4. Update manifest.json with your details
5. Add your widget logic to MainViewModel
6. Design your UI in MainView.xaml
7. Build and test!

---

## Related Skills

- [project-setup.md](../core/project-setup.md) - Project configuration
- [widget-architecture.md](../core/widget-architecture.md) - Architecture details

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Updated for best practices |
| 1.0.0 | 2025-06-01 | Initial version |
