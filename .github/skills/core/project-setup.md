# Project Setup

> **Category:** Core | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers the complete .csproj configuration, dependencies, and project structure required for 3SC widget development.

## Prerequisites

- [widget-architecture.md](widget-architecture.md)

---

## Complete Project Template

### .csproj File

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Target Framework - MUST be net8.0-windows -->
    <TargetFramework>net8.0-windows</TargetFramework>
    
    <!-- Namespace follows underscore convention for numeric prefix -->
    <RootNamespace>_3SC.Widgets.MyWidget</RootNamespace>
    <AssemblyName>3SC.Widgets.MyWidget</AssemblyName>
    
    <!-- WPF Support -->
    <UseWPF>true</UseWPF>
    
    <!-- Modern C# features -->
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    
    <!-- Dual-mode build: Executable for debug, Library for release -->
    <OutputType Condition="'$(Configuration)' == 'Debug'">WinExe</OutputType>
    <OutputType Condition="'$(Configuration)' == 'Release'">Library</OutputType>
    
    <!-- Entry point for debug mode testing -->
    <StartupObject Condition="'$(Configuration)' == 'Debug'">_3SC.Widgets.MyWidget.TestLauncher</StartupObject>
    
    <!-- Required for some WPF interop -->
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    
    <!-- Version info -->
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Company>Your Company</Company>
    <Description>Widget description</Description>
  </PropertyGroup>

  <!-- NuGet Packages - CRITICAL: Use exact versions! -->
  <ItemGroup>
    <!-- MVVM Framework - Must be 8.2.2 -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    
    <!-- Logging - Must be 3.1.1, NOT 4.x -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    
    <!-- JSON Serialization (included in .NET 8) -->
    <!-- No need for Newtonsoft.Json unless specifically required -->
  </ItemGroup>

  <!-- Widget Contracts Reference -->
  <ItemGroup>
    <!-- Option 1: Direct DLL reference (for external widgets) -->
    <Reference Include="3SC.Widgets.Contracts">
      <HintPath>..\packages\3SC.Widgets.Contracts.dll</HintPath>
      <Private>true</Private>
    </Reference>
    
    <!-- Option 2: Project reference (for development in same solution) -->
    <!--
    <ProjectReference Include="..\..\3SC\3SC.Widgets.Contracts\3SC.Widgets.Contracts.csproj" />
    -->
  </ItemGroup>

  <!-- Content Files -->
  <ItemGroup>
    <!-- Manifest must be copied to output -->
    <None Update="manifest.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    
    <!-- Icon and images -->
    <None Update="icon.png">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    
    <!-- Resources as embedded resources -->
    <Resource Include="Resources\**\*" />
  </ItemGroup>

</Project>
```

---

## Dependency Versions

### ⚠️ CRITICAL VERSION REQUIREMENTS

| Package | Required Version | ❌ Incompatible |
|---------|------------------|-----------------|
| CommunityToolkit.Mvvm | **8.2.2** | 8.3.x+ (API changes) |
| Serilog | **3.1.1** | 4.x (breaking changes) |
| Serilog.Sinks.File | **5.0.0** | 6.x (requires Serilog 4.x) |

### Why Version Lock?

The 3SC host application uses specific versions. Mismatched versions cause:
- Assembly binding failures
- Runtime type mismatches
- Serialization errors
- Logging failures

### Checking Host Versions

```powershell
# Check what versions 3SC uses
Get-ChildItem "$env:LOCALAPPDATA\3SC\app-*\*.dll" | 
    Select-Object Name, @{N='Version';E={$_.VersionInfo.FileVersion}}
```

---

## Project Structure

```
3SC.Widgets.MyWidget/
├── 3SC.Widgets.MyWidget.csproj     # Project file
├── manifest.json                    # Widget manifest
├── icon.png                         # Widget icon (optional)
│
├── MyWidgetWidgetFactory.cs         # IWidgetFactory + IWidget
├── MyWidgetViewModel.cs             # ViewModel (MVVM)
├── MyWidgetWindow.xaml              # Main window UI
├── MyWidgetWindow.xaml.cs           # Window code-behind
│
├── TestLauncher.cs                  # Debug mode entry point
│
├── ValueObjects/                    # Data classes
│   ├── MyWidgetSettings.cs          # Settings class
│   └── MyWidgetState.cs             # State persistence
│
├── Services/                        # Business logic
│   └── MyWidgetService.cs           # Service classes
│
├── Converters/                      # Value converters
│   └── BoolToVisibilityConverter.cs
│
├── Behaviors/                       # Attached behaviors
│   └── DragBehavior.cs
│
├── Resources/                       # Images, icons, assets
│   ├── Icons/
│   └── Images/
│
└── Themes/                          # Widget-specific styles
    └── Styles.xaml
```

---

## Namespace Convention

C# doesn't allow namespaces starting with numbers, so use underscore:

```csharp
// Correct
namespace _3SC.Widgets.Clock
{
    public class ClockWidgetFactory { }
}

// In XAML
xmlns:local="clr-namespace:_3SC.Widgets.Clock"
```

---

## Build Configurations

### Debug Mode

For standalone testing without 3SC:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <OutputType>WinExe</OutputType>
  <StartupObject>_3SC.Widgets.MyWidget.TestLauncher</StartupObject>
  <DefineConstants>DEBUG;TRACE</DefineConstants>
</PropertyGroup>
```

### Release Mode

For deployment to 3SC:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <OutputType>Library</OutputType>
  <Optimize>true</Optimize>
  <DefineConstants>RELEASE</DefineConstants>
</PropertyGroup>
```

---

## Test Launcher

```csharp
#if DEBUG
using System.IO;
using System.Windows;
using Serilog;

namespace _3SC.Widgets.MyWidget;

/// <summary>
/// Debug-mode entry point for standalone widget testing.
/// Only compiled in Debug configuration.
/// </summary>
public class TestLauncher
{
    [STAThread]
    public static void Main()
    {
        // Setup logging
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "Widgets", "my-widget", "logs", "debug-.log");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
        
        Log.Information("Starting MyWidget in test mode");
        
        try
        {
            // Create application
            var app = new Application();
            
            // Create widget via factory
            var factory = new MyWidgetWidgetFactory();
            var widget = factory.CreateWidget();
            
            // Create window
            var window = widget.CreateWindow();
            if (window == null)
            {
                Log.Error("Failed to create widget window");
                return;
            }
            
            // Initialize widget
            widget.OnInitialize();
            
            // Handle cleanup on close
            window.Closed += (s, e) =>
            {
                widget.OnDispose();
                Log.Information("Widget disposed");
                Log.CloseAndFlush();
            };
            
            // Run
            app.Run(window);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in test launcher");
            MessageBox.Show($"Error: {ex.Message}", "Widget Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
#endif
```

---

## Build Commands

### Local Development

```powershell
# Build for debugging
dotnet build -c Debug

# Run standalone
dotnet run -c Debug

# Build for release
dotnet build -c Release
```

### Publishing

```powershell
# Publish with all dependencies
dotnet publish -c Release -o bin\Release\net8.0-windows\publish

# The publish folder contains everything needed
```

### Quick Test Installation

```powershell
# Copy to 3SC Community folder
$source = ".\bin\Release\net8.0-windows\publish"
$dest = "$env:APPDATA\3SC\Widgets\Community\my-widget"

New-Item -ItemType Directory -Force -Path $dest
Copy-Item "$source\*" -Destination $dest -Force

Write-Host "Widget installed to $dest"
```

---

## Common Package Additions

### For HTTP Requests

```xml
<!-- Already included in .NET 8, just use System.Net.Http -->
```

### For Advanced JSON

```xml
<!-- Only if System.Text.Json is insufficient -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### For Image Processing

```xml
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.4" />
```

### For PDF

```xml
<PackageReference Include="PdfiumViewer" Version="2.13.0" />
```

### For SQLite

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
```

### For Web Views

```xml
<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.2478.35" />
```

---

## Assembly Attributes

```csharp
// AssemblyInfo.cs or in .csproj
using System.Runtime.Versioning;
using System.Windows;

[assembly: SupportedOSPlatform("windows")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
```

---

## .gitignore Additions

```gitignore
# Build outputs
bin/
obj/
publish/

# User files
*.user
*.suo

# IDE
.vs/
.idea/

# Package output
*.3scwidget

# Logs
logs/
*.log
```

---

## Solution Structure

If creating a solution for widget development:

```
widgets.sln
├── 3SC.Widgets.Clock/
├── 3SC.Widgets.Notes/
├── 3SC.Widgets.Calendar/
├── packages/
│   └── 3SC.Widgets.Contracts.dll
└── WidgetTestHost/                  # Shared test infrastructure
```

---

## Common Mistakes

### ❌ Wrong Target Framework

```xml
<!-- Wrong -->
<TargetFramework>net8.0</TargetFramework>

<!-- Correct - must include -windows for WPF -->
<TargetFramework>net8.0-windows</TargetFramework>
```

### ❌ Missing UseWPF

```xml
<!-- Wrong - WPF won't work -->
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
</PropertyGroup>

<!-- Correct -->
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
</PropertyGroup>
```

### ❌ Wrong Serilog Version

```xml
<!-- Wrong - will fail at runtime -->
<PackageReference Include="Serilog" Version="4.0.0" />

<!-- Correct -->
<PackageReference Include="Serilog" Version="3.1.1" />
```

### ❌ Missing manifest.json Copy

```xml
<!-- Wrong - manifest won't be in output -->
<None Include="manifest.json" />

<!-- Correct -->
<None Update="manifest.json">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

---

## Related Skills

- [widget-architecture.md](widget-architecture.md) - Widget structure
- [manifest-specification.md](manifest-specification.md) - Manifest format
- [build-configuration.md](../packaging/build-configuration.md) - Build settings
- [packaging-deployment.md](../packaging/packaging-deployment.md) - Deployment

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Complete rewrite with templates |
| 1.0.0 | 2025-06-01 | Initial version |
