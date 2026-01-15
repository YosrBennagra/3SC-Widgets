# Test Template

> **Category:** Templates | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Complete test templates using xUnit and NSubstitute for widget testing.

---

## ViewModel Test Template

```csharp
using Xunit;
using NSubstitute;
using Serilog;
using FluentAssertions;

namespace MyWidget.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly ILogger _mockLog;
    private readonly IDataService _mockDataService;
    private readonly MainViewModel _sut; // System Under Test
    
    public MainViewModelTests()
    {
        // Arrange - Common setup
        _mockLog = Substitute.For<ILogger>();
        _mockLog.ForContext<MainViewModel>().Returns(_mockLog);
        
        _mockDataService = Substitute.For<IDataService>();
        
        _sut = new MainViewModel(_mockLog, _mockDataService);
    }
    
    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        // Assert
        _sut.Title.Should().Be("Default Title");
        _sut.IsLoading.Should().BeFalse();
        _sut.Items.Should().BeEmpty();
    }
    
    [Fact]
    public async Task LoadDataCommand_WhenSuccessful_PopulatesItems()
    {
        // Arrange
        var testItems = new List<Item>
        {
            new() { Id = 1, Name = "Item 1" },
            new() { Id = 2, Name = "Item 2" }
        };
        _mockDataService.GetItemsAsync().Returns(testItems);
        
        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);
        
        // Assert
        _sut.Items.Should().HaveCount(2);
        _sut.IsLoading.Should().BeFalse();
    }
    
    [Fact]
    public async Task LoadDataCommand_WhenFails_SetsError()
    {
        // Arrange
        _mockDataService.GetItemsAsync()
            .ThrowsAsync(new Exception("Test error"));
        
        // Act
        await _sut.LoadDataCommand.ExecuteAsync(null);
        
        // Assert
        _sut.ErrorMessage.Should().NotBeNullOrEmpty();
        _sut.IsLoading.Should().BeFalse();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Title_WhenSetToInvalid_KeepsDefaultValue(string? invalidTitle)
    {
        // Act
        _sut.Title = invalidTitle!;
        
        // Assert - depends on implementation
        // This tests property validation if implemented
    }
}
```

---

## Service Test Template

```csharp
using Xunit;
using NSubstitute;
using Serilog;
using FluentAssertions;
using System.Text.Json;

namespace MyWidget.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly ILogger _mockLog;
    private readonly string _testSettingsPath;
    private readonly SettingsService<TestSettings> _sut;
    
    public SettingsServiceTests()
    {
        _mockLog = Substitute.For<ILogger>();
        _mockLog.ForContext<SettingsService<TestSettings>>().Returns(_mockLog);
        
        // Use temp file for tests
        _testSettingsPath = Path.Combine(Path.GetTempPath(), $"test-settings-{Guid.NewGuid()}.json");
        
        _sut = new SettingsService<TestSettings>(_mockLog, _testSettingsPath);
    }
    
    [Fact]
    public void Current_ReturnsDefaultSettings_WhenFileNotExists()
    {
        // Assert
        _sut.Current.Should().NotBeNull();
        _sut.Current.StringValue.Should().Be("default");
    }
    
    [Fact]
    public async Task SaveAsync_WritesSettingsToFile()
    {
        // Arrange
        _sut.Current.StringValue = "modified";
        _sut.Current.IntValue = 42;
        
        // Act
        await _sut.SaveAsync();
        
        // Assert
        File.Exists(_testSettingsPath).Should().BeTrue();
        var json = await File.ReadAllTextAsync(_testSettingsPath);
        var loaded = JsonSerializer.Deserialize<TestSettings>(json);
        loaded!.StringValue.Should().Be("modified");
        loaded.IntValue.Should().Be(42);
    }
    
    [Fact]
    public async Task LoadAsync_ReadsExistingSettings()
    {
        // Arrange
        var existing = new TestSettings { StringValue = "existing", IntValue = 100 };
        await File.WriteAllTextAsync(_testSettingsPath, JsonSerializer.Serialize(existing));
        
        // Act
        await _sut.LoadAsync();
        
        // Assert
        _sut.Current.StringValue.Should().Be("existing");
        _sut.Current.IntValue.Should().Be(100);
    }
    
    public void Dispose()
    {
        if (File.Exists(_testSettingsPath))
        {
            File.Delete(_testSettingsPath);
        }
    }
    
    private class TestSettings
    {
        public string StringValue { get; set; } = "default";
        public int IntValue { get; set; } = 0;
    }
}
```

---

## Model Test Template

```csharp
using Xunit;
using FluentAssertions;

namespace MyWidget.Tests.Models;

public class ItemTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var item = new Item();
        
        // Assert
        item.Id.Should().Be(0);
        item.Name.Should().BeEmpty();
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    [Theory]
    [InlineData("Test Name")]
    [InlineData("")]
    [InlineData("Name with special chars: !@#$%")]
    public void Name_CanBeSetToAnyValue(string name)
    {
        // Arrange
        var item = new Item();
        
        // Act
        item.Name = name;
        
        // Assert
        item.Name.Should().Be(name);
    }
    
    [Fact]
    public void Equals_ReturnsTrueForSameId()
    {
        // Arrange
        var item1 = new Item { Id = 1, Name = "A" };
        var item2 = new Item { Id = 1, Name = "B" };
        
        // Assert
        item1.Should().Be(item2); // If Equals is overridden
    }
}
```

---

## Async Test Helpers

```csharp
public static class TestHelpers
{
    /// <summary>
    /// Wait for a condition with timeout
    /// </summary>
    public static async Task WaitForAsync(
        Func<bool> condition,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        pollInterval ??= TimeSpan.FromMilliseconds(50);
        
        var deadline = DateTime.UtcNow + timeout.Value;
        
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return;
            
            await Task.Delay(pollInterval.Value);
        }
        
        throw new TimeoutException("Condition not met within timeout");
    }
    
    /// <summary>
    /// Create mock logger that captures messages
    /// </summary>
    public static (ILogger Logger, List<string> Messages) CreateCapturingLogger()
    {
        var messages = new List<string>();
        var logger = Substitute.For<ILogger>();
        
        logger.When(x => x.Information(Arg.Any<string>()))
            .Do(x => messages.Add(x.Arg<string>()));
        
        logger.When(x => x.Debug(Arg.Any<string>()))
            .Do(x => messages.Add(x.Arg<string>()));
        
        logger.ForContext<Arg.AnyType>().Returns(logger);
        
        return (logger, messages);
    }
}
```

---

## Test Project Setup (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyWidget\MyWidget.csproj" />
  </ItemGroup>
</Project>
```

---

## Running Tests

```powershell
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~MainViewModelTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Best Practices

1. **Arrange-Act-Assert** - Clear test structure
2. **One assertion per test** - Keep tests focused
3. **Use Theory for variations** - Test multiple inputs
4. **Mock external dependencies** - Isolate unit under test
5. **Clean up resources** - Implement IDisposable

---

## Related Skills

- [testing-strategies.md](../quality/testing-strategies.md) - Testing overview
- [mvvm-patterns.md](../ui/mvvm-patterns.md) - MVVM testability

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added async helpers |
| 1.0.0 | 2025-06-01 | Initial version |
