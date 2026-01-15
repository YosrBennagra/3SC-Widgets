# Testing Strategies

> **Category:** Quality | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers testing strategies for widgets, including unit tests, integration tests, UI testing, and the TestLauncher pattern.

---

## Testing Pyramid

```
                    ┌───────────┐
                    │   E2E     │  ← Few, slow, high confidence
                    │   Tests   │
                ┌───┴───────────┴───┐
                │   Integration     │  ← Widget + Host interaction
                │      Tests        │
            ┌───┴───────────────────┴───┐
            │       Unit Tests          │  ← Many, fast, focused
            │   ViewModels, Services    │
            └───────────────────────────┘
```

---

## Project Setup

### Test Project (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.1">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\3SC.Widgets.MyWidget\3SC.Widgets.MyWidget.csproj" />
  </ItemGroup>

</Project>
```

---

## Unit Testing ViewModels

### Basic ViewModel Test

```csharp
using FluentAssertions;
using Xunit;

namespace _3SC.Widgets.MyWidget.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var viewModel = new MainViewModel();
        
        // Assert
        viewModel.Title.Should().NotBeNullOrEmpty();
        viewModel.IsLoading.Should().BeFalse();
    }
    
    [Fact]
    public void RefreshCommand_ShouldUpdateData()
    {
        // Arrange
        var viewModel = new MainViewModel();
        
        // Act
        viewModel.RefreshCommand.Execute(null);
        
        // Assert
        viewModel.LastUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void SetTitle_WithInvalidValue_ShouldNotChange(string? invalidTitle)
    {
        // Arrange
        var viewModel = new MainViewModel { Title = "Original" };
        
        // Act
        viewModel.Title = invalidTitle!;
        
        // Assert
        viewModel.Title.Should().Be("Original");
    }
}
```

### Testing Async Commands

```csharp
public class AsyncViewModelTests
{
    [Fact]
    public async Task LoadDataCommand_ShouldSetLoadingState()
    {
        // Arrange
        var viewModel = new MainViewModel();
        
        // Act
        var loadTask = viewModel.LoadDataCommand.ExecuteAsync(null);
        
        // Assert - During execution
        viewModel.IsLoading.Should().BeTrue();
        
        await loadTask;
        
        // Assert - After completion
        viewModel.IsLoading.Should().BeFalse();
        viewModel.Data.Should().NotBeNull();
    }
    
    [Fact]
    public async Task LoadDataCommand_WhenFails_ShouldSetError()
    {
        // Arrange
        var mockService = Substitute.For<IDataService>();
        mockService.LoadAsync().ThrowsAsync(new Exception("Network error"));
        
        var viewModel = new MainViewModel(mockService);
        
        // Act
        await viewModel.LoadDataCommand.ExecuteAsync(null);
        
        // Assert
        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Contain("Network error");
    }
}
```

---

## Testing with Mocks

### Using NSubstitute

```csharp
public class ServiceMockingTests
{
    private readonly ISettingsService _mockSettings;
    private readonly ILogger _mockLogger;
    
    public ServiceMockingTests()
    {
        _mockSettings = Substitute.For<ISettingsService>();
        _mockLogger = Substitute.For<ILogger>();
    }
    
    [Fact]
    public void SaveSettings_ShouldCallService()
    {
        // Arrange
        var settings = new WidgetSettings { RefreshInterval = 30 };
        var viewModel = new SettingsViewModel(_mockSettings, _mockLogger);
        viewModel.Settings = settings;
        
        // Act
        viewModel.SaveCommand.Execute(null);
        
        // Assert
        _mockSettings.Received(1).Save(Arg.Is<WidgetSettings>(s => s.RefreshInterval == 30));
    }
    
    [Fact]
    public void LoadSettings_WhenServiceThrows_ShouldUseDefaults()
    {
        // Arrange
        _mockSettings.Load().Returns(x => throw new FileNotFoundException());
        var viewModel = new SettingsViewModel(_mockSettings, _mockLogger);
        
        // Act
        viewModel.LoadSettings();
        
        // Assert
        viewModel.Settings.Should().NotBeNull();
        viewModel.Settings.RefreshInterval.Should().Be(60); // Default
        _mockLogger.Received().Warning(Arg.Any<Exception>(), Arg.Any<string>());
    }
}
```

---

## Testing Settings

```csharp
public class SettingsServiceTests
{
    private readonly string _testDataPath;
    
    public SettingsServiceTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
    }
    
    [Fact]
    public void Save_ShouldPersistSettings()
    {
        // Arrange
        var service = new SettingsService<TestSettings>(_testDataPath);
        var settings = new TestSettings { Value = "test" };
        
        // Act
        service.Save(settings);
        
        // Assert
        var loaded = service.Load();
        loaded.Value.Should().Be("test");
    }
    
    [Fact]
    public void Load_WhenFileNotExists_ShouldReturnDefaults()
    {
        // Arrange
        var service = new SettingsService<TestSettings>(_testDataPath);
        
        // Act
        var settings = service.Load();
        
        // Assert
        settings.Should().NotBeNull();
        settings.Value.Should().BeNull();
    }
    
    [Fact]
    public void Save_ShouldCreateBackup()
    {
        // Arrange
        var service = new SettingsService<TestSettings>(_testDataPath);
        service.Save(new TestSettings { Value = "v1" });
        
        // Act
        service.Save(new TestSettings { Value = "v2" });
        
        // Assert
        var backupFiles = Directory.GetFiles(_testDataPath, "*.bak");
        backupFiles.Should().NotBeEmpty();
    }
    
    public void Dispose()
    {
        Directory.Delete(_testDataPath, recursive: true);
    }
}

public class TestSettings
{
    public string? Value { get; set; }
}
```

---

## TestLauncher Pattern

For UI testing without the host application:

### TestLauncher Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <RootNamespace>_3SC.Widgets.MyWidget.TestLauncher</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\3SC.Widgets.MyWidget\3SC.Widgets.MyWidget.csproj" />
  </ItemGroup>
</Project>
```

### TestLauncher App.xaml.cs

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .CreateLogger();
        
        // Create and show widget
        var factory = new MyWidgetFactory();
        var widget = factory.Create();
        
        widget.OnInitialize();
        
        // Host window simulates 3SC environment
        var host = new TestHostWindow
        {
            Content = widget.Content,
            Title = $"Widget Test: {widget.Name}",
            Width = 400,
            Height = 300
        };
        
        host.Closed += (s, e) =>
        {
            widget.OnClose();
            Shutdown();
        };
        
        host.Show();
    }
}
```

### TestHostWindow

```csharp
public class TestHostWindow : Window
{
    public TestHostWindow()
    {
        // Simulate 3SC host environment
        Background = new SolidColorBrush(Color.FromRgb(26, 26, 46));
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        
        // Add test controls
        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem 
        { 
            Header = "Open Settings",
            Command = new RelayCommand(OpenSettings)
        });
        contextMenu.Items.Add(new MenuItem 
        { 
            Header = "Reset Size",
            Command = new RelayCommand(() => { Width = 400; Height = 300; })
        });
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(new MenuItem 
        { 
            Header = "Close",
            Command = new RelayCommand(Close)
        });
        
        ContextMenu = contextMenu;
    }
    
    private void OpenSettings()
    {
        if (Content is FrameworkElement fe && fe.DataContext is IHasSettings vm)
        {
            vm.OpenSettingsCommand.Execute(null);
        }
    }
}
```

---

## Integration Tests

### Widget Lifecycle Tests

```csharp
public class WidgetLifecycleTests
{
    [Fact]
    public void Widget_ShouldInitializeCorrectly()
    {
        // Arrange
        var factory = new MyWidgetFactory();
        
        // Act
        var widget = factory.Create();
        widget.OnInitialize();
        
        // Assert
        widget.Content.Should().NotBeNull();
        widget.Name.Should().NotBeNullOrEmpty();
        widget.HasSettings.Should().BeTrue();
    }
    
    [Fact]
    public void Widget_ShouldCleanupOnClose()
    {
        // Arrange
        var factory = new MyWidgetFactory();
        var widget = factory.Create();
        widget.OnInitialize();
        
        // Act
        widget.OnClose();
        
        // Assert - No exceptions, resources released
        // Verify timers stopped, subscriptions disposed, etc.
    }
    
    [Fact]
    public void Widget_ShouldHandleResize()
    {
        // Arrange
        var factory = new MyWidgetFactory();
        var widget = factory.Create();
        widget.OnInitialize();
        
        // Act
        widget.OnResize(new Size(500, 400));
        
        // Assert
        if (widget.Content is FrameworkElement fe)
        {
            fe.Width.Should().Be(500);
            fe.Height.Should().Be(400);
        }
    }
}
```

---

## Snapshot Testing

For UI consistency:

```csharp
public class SnapshotTests
{
    [Fact]
    public void MainView_ShouldMatchSnapshot()
    {
        // Arrange
        var viewModel = new MainViewModel
        {
            Title = "Test Widget",
            Data = new List<string> { "Item 1", "Item 2" }
        };
        
        var view = new MainView { DataContext = viewModel };
        
        // Act
        var xaml = XamlWriter.Save(view);
        
        // Assert
        xaml.Should().MatchSnapshot();
    }
}
```

---

## Test Data Builders

```csharp
public class TestDataBuilder
{
    public static WidgetSettings CreateDefaultSettings()
    {
        return new WidgetSettings
        {
            RefreshInterval = 60,
            Theme = "dark",
            Opacity = 1.0,
            ShowTitle = true
        };
    }
    
    public static WidgetSettings CreateSettings(Action<WidgetSettings>? configure = null)
    {
        var settings = CreateDefaultSettings();
        configure?.Invoke(settings);
        return settings;
    }
    
    public static MainViewModel CreateViewModel(
        ISettingsService? settingsService = null,
        IDataService? dataService = null)
    {
        return new MainViewModel(
            settingsService ?? Substitute.For<ISettingsService>(),
            dataService ?? Substitute.For<IDataService>()
        );
    }
}

// Usage
[Fact]
public void Test_WithCustomSettings()
{
    var settings = TestDataBuilder.CreateSettings(s => s.RefreshInterval = 30);
    var viewModel = TestDataBuilder.CreateViewModel();
    // ...
}
```

---

## Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~MainViewModelTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Generate coverage report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

---

## Best Practices

1. **Test behavior, not implementation** - Focus on what, not how
2. **Use meaningful names** - `Method_Scenario_Expected` format
3. **Keep tests independent** - No shared state between tests
4. **Mock external dependencies** - Database, network, filesystem
5. **Test edge cases** - Null, empty, boundary values

---

## Related Skills

- [error-handling.md](error-handling.md) - Error scenarios
- [logging.md](logging.md) - Test logging

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added TestLauncher pattern |
| 1.0.0 | 2025-06-01 | Initial version |
