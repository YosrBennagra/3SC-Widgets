# Build Configuration

> **Category:** Packaging | **Priority:** 🔴 Critical
> **Version:** 3.0.0 | **Last Updated:** 2026-01-20

## Overview

This skill covers build configuration for widget projects, including Debug/Release modes, dual-mode builds, and MSBuild customization.

**NOTE:** For packaging, use the centralized `Build-Widget.ps1` script in the repository root. See [packaging-deployment.md](packaging-deployment.md).

## Prerequisites

- [project-setup.md](../core/project-setup.md)
- [packaging-deployment.md](packaging-deployment.md) - For packaging with Build-Widget.ps1

---

## Build Modes

### Debug Mode

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <!-- Debug symbols -->
  <DebugType>full</DebugType>
  <DebugSymbols>true</DebugSymbols>
  <Optimize>false</Optimize>
  
  <!-- Constants -->
  <DefineConstants>DEBUG;TRACE</DefineConstants>
  
  <!-- Output -->
  <OutputPath>bin\Debug\net8.0-windows\</OutputPath>
</PropertyGroup>
```

### Release Mode

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <!-- Optimization -->
  <DebugType>pdbonly</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <Optimize>true</Optimize>
  
  <!-- Constants -->
  <DefineConstants>RELEASE</DefineConstants>
  
  <!-- Output -->
  <OutputPath>bin\Release\net8.0-windows\</OutputPath>
</PropertyGroup>
```

---

## Dual-Mode Build Configuration

Widgets support two build modes for flexibility:

### Mode 1: External Widget (Default)

Builds as a library that integrates with the 3SC host:

```xml
<PropertyGroup Condition="'$(BuildMode)' != 'Standalone'">
  <OutputType>Library</OutputType>
  <StartAction>None</StartAction>
  
  <!-- External widget packaging -->
  <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
</PropertyGroup>
```

### Mode 2: Standalone Application

Builds as a runnable application for testing:

```xml
<PropertyGroup Condition="'$(BuildMode)' == 'Standalone'">
  <OutputType>WinExe</OutputType>
  <StartAction>Project</StartAction>
  
  <!-- Allow running directly -->
  <UseWPF>true</UseWPF>
</PropertyGroup>
```

### Building with Mode Switch

```powershell
# Build as external widget (default)
dotnet build -c Release

# Build as standalone for testing
dotnet build -c Debug -p:BuildMode=Standalone
```

---

## Complete .csproj Template

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Framework -->
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- Identity -->
    <RootNamespace>_3SC.Widgets.MyWidget</RootNamespace>
    <AssemblyName>3SC.Widgets.MyWidget</AssemblyName>
    
    <!-- Version -->
    <Version>1.0.0</Version>
    <FileVersion>1.0.0.0</FileVersion>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    
    <!-- Metadata -->
    <Product>My Widget</Product>
    <Company>3SC</Company>
    <Authors>3SC Team</Authors>
    <Description>Description of widget</Description>
    <Copyright>Copyright © 3SC 2025</Copyright>
  </PropertyGroup>

  <!-- Build mode configuration -->
  <PropertyGroup Condition="'$(BuildMode)' != 'Standalone'">
    <OutputType>Library</OutputType>
  </PropertyGroup>
  
  <PropertyGroup Condition="'$(BuildMode)' == 'Standalone'">
    <OutputType>WinExe</OutputType>
  </PropertyGroup>

  <!-- Debug configuration -->
  <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <DebugType>full</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <Optimize>false</Optimize>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
  </PropertyGroup>

  <!-- Release configuration -->
  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <DefineConstants>RELEASE</DefineConstants>
  </PropertyGroup>

  <!-- NuGet packages - CRITICAL VERSIONS -->
  <ItemGroup>
    <!-- MUST use 8.2.2, NOT 8.3.x+ -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    
    <!-- MUST use 3.1.1, NOT 4.x -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  </ItemGroup>

  <!-- Contracts reference -->
  <ItemGroup>
    <ProjectReference Include="..\3SC.Widgets.Contracts\3SC.Widgets.Contracts.csproj" />
  </ItemGroup>

  <!-- Include manifest in output -->
  <ItemGroup>
    <None Include="manifest.json" CopyToOutputDirectory="PreserveNewest" />
    <None Include="Assets\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

---

## MSBuild Customization

### Custom Targets

```xml
<!-- Pre-build: Validate manifest -->
<Target Name="ValidateManifest" BeforeTargets="Build">
  <Error Condition="!Exists('manifest.json')" 
         Text="manifest.json is required" />
  
  <Message Importance="high" 
           Text="Building widget: $(AssemblyName) v$(Version)" />
</Target>

<!-- Post-build: Copy to 3SC widgets folder -->
<Target Name="CopyToWidgetsFolder" AfterTargets="Build" 
        Condition="'$(Configuration)' == 'Debug'">
  <PropertyGroup>
    <WidgetsPath>$(APPDATA)\3SC\Widgets\Community\$(WidgetKey)</WidgetsPath>
  </PropertyGroup>
  
  <MakeDir Directories="$(WidgetsPath)" />
  
  <Copy SourceFiles="@(OutputFiles)" 
        DestinationFolder="$(WidgetsPath)" />
  
  <Message Importance="high" 
           Text="Copied to: $(WidgetsPath)" />
</Target>

<!-- Clean custom outputs -->
<Target Name="CleanWidgetOutput" AfterTargets="Clean">
  <RemoveDir Directories="$(OutputPath)" />
</Target>
```

### Version Sync

```xml
<!-- Automatically sync version from manifest -->
<Target Name="SyncVersionFromManifest" BeforeTargets="GetAssemblyVersion">
  <PropertyGroup>
    <ManifestPath>$(MSBuildProjectDirectory)\manifest.json</ManifestPath>
  </PropertyGroup>
  
  <Exec Command="powershell -Command &quot;(Get-Content '$(ManifestPath)' | ConvertFrom-Json).version&quot;"
        ConsoleToMSBuild="true"
        Condition="Exists('$(ManifestPath)')">
    <Output TaskParameter="ConsoleOutput" PropertyName="ManifestVersion" />
  </Exec>
  
  <PropertyGroup Condition="'$(ManifestVersion)' != ''">
    <Version>$(ManifestVersion)</Version>
    <FileVersion>$(ManifestVersion).0</FileVersion>
    <AssemblyVersion>$(ManifestVersion).0</AssemblyVersion>
  </PropertyGroup>
</Target>
```

---

## Build Scripts

### build.ps1

```powershell
#!/usr/bin/env pwsh
#Requires -Version 7.0

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [ValidateSet('External', 'Standalone')]
    [string]$BuildMode = 'External',
    
    [switch]$Clean,
    [switch]$Package
)

$ErrorActionPreference = 'Stop'

$WidgetKey = (Get-Content manifest.json | ConvertFrom-Json).widgetKey
$Version = (Get-Content manifest.json | ConvertFrom-Json).version

Write-Host "Building $WidgetKey v$Version" -ForegroundColor Cyan

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning..." -ForegroundColor Yellow
    dotnet clean -c $Configuration
    Remove-Item -Path "bin", "obj" -Recurse -Force -ErrorAction SilentlyContinue
}

# Build
$buildModeArg = if ($BuildMode -eq 'Standalone') { '-p:BuildMode=Standalone' } else { '' }
dotnet build -c $Configuration $buildModeArg

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build succeeded!" -ForegroundColor Green

# Package if requested
if ($Package) {
    # Use centralized Build-Widget.ps1 script
    & "$PSScriptRoot\..\..\Build-Widget.ps1" -WidgetName (Split-Path -Leaf $PSScriptRoot)
}
```

### watch.ps1

```powershell
#!/usr/bin/env pwsh
# Hot-reload development mode

param(
    [string]$WidgetsPath = "$env:APPDATA\3SC\Widgets\Community"
)

$manifest = Get-Content manifest.json | ConvertFrom-Json
$widgetKey = $manifest.widgetKey
$targetPath = Join-Path $WidgetsPath $widgetKey

Write-Host "Watching for changes..." -ForegroundColor Cyan
Write-Host "Target: $targetPath" -ForegroundColor Gray

# Watch and rebuild on changes
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $PSScriptRoot
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true

$action = {
    $path = $Event.SourceEventArgs.FullPath
    $changeType = $Event.SourceEventArgs.ChangeType
    
    # Ignore bin/obj folders
    if ($path -match '\\(bin|obj)\\') { return }
    
    Write-Host "[$changeType] $path" -ForegroundColor Yellow
    
    # Rebuild
    Push-Location $EventSubscriber.SourceObject.Path
    dotnet build -c Debug --no-restore 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Rebuilt successfully" -ForegroundColor Green
    } else {
        Write-Host "Build failed" -ForegroundColor Red
    }
    Pop-Location
}

Register-ObjectEvent $watcher "Changed" -Action $action
Register-ObjectEvent $watcher "Created" -Action $action
Register-ObjectEvent $watcher "Deleted" -Action $action

Write-Host "Press Ctrl+C to stop watching" -ForegroundColor Gray
while ($true) { Start-Sleep -Seconds 1 }
```

---

## Environment Variables

```powershell
# Set widget development environment
$env:3SC_ENVIRONMENT = "Development"
$env:3SC_LOG_LEVEL = "Debug"
$env:3SC_WIDGET_DATA_PATH = "C:\Dev\WidgetData"

# Build with environment
dotnet build -c Debug
```

---

## CI/CD Integration

### GitHub Actions

```yaml
name: Build Widget

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release --no-restore
    
    - name: Test
      run: dotnet test -c Release --no-build
    
    - name: Package
      run: ./package.ps1 -Configuration Release
    
    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: widget-package
        path: "*.3scwidget"
```

---

## Troubleshooting

### Common Build Issues

| Issue | Solution |
|-------|----------|
| Missing UseWPF | Add `<UseWPF>true</UseWPF>` to .csproj |
| NuGet restore fails | Clear cache: `dotnet nuget locals all --clear` |
| Version mismatch | Run `SyncVersionFromManifest` target |
| Missing manifest | Create manifest.json with required fields |

---

## Related Skills

- [project-setup.md](../core/project-setup.md) - Project structure
- [packaging-deployment.md](packaging-deployment.md) - Creating packages
- [versioning.md](versioning.md) - Version management

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added CI/CD, watch script |
| 1.0.0 | 2025-06-01 | Initial version |
