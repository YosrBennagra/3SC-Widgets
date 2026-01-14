# Widget Packaging and Deployment

## Package Structure

A widget package consists of:
```
{widget-key}/
  ├── {WidgetName}.dll          (Main widget assembly)
  ├── 3SC.Widgets.Contracts.dll (Contracts interface)
  ├── manifest.json              (Widget metadata)
  └── {WidgetName}.deps.json    (Dependencies)
```

## Build Script

Create a PowerShell script to automate building and packaging:

```powershell
# Build-And-Package-Widget.ps1

Write-Host "========================================"
Write-Host "Building Widget"
Write-Host "========================================"

# Clean previous builds
Remove-Item -Path "packages/*" -Recurse -Force -ErrorAction SilentlyContinue

# Build in Release mode
Write-Host "Building widget in Release mode..."
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host "Build successful!"

# Create package directories
$packagesDir = "packages"
$unpackagedDir = "$packagesDir\{widget-key}"
New-Item -ItemType Directory -Force -Path $unpackagedDir | Out-Null

# Copy files
$buildDir = "bin\Release\net8.0-windows"
Copy-Item "$buildDir\*.dll" -Destination $unpackagedDir
Copy-Item "$buildDir\manifest.json" -Destination $unpackagedDir
Copy-Item "$buildDir\*.deps.json" -Destination $unpackagedDir

Write-Host "Package created in: $unpackagedDir"
```

## Manifest.json

Required fields:

```json
{
  "packageId": "com.3sc.widget-name",
  "widgetKey": "widget-name",
  "displayName": "Widget Display Name",
  "version": "1.0.0",
  "author": {
    "name": "Author Name",
    "email": "author@example.com"
  },
  "description": "Widget description",
  "category": "general",
  "icon": "icon.png",
  "entry": "WidgetName.dll",
  "minAppVersion": "1.0.0",
  "hasSettings": true,
  "permissions": [],
  "tags": ["tag1", "tag2"],
  "defaultSize": {
    "width": 300,
    "height": 200
  }
}
```

### Field Descriptions

- **packageId**: Unique identifier (reverse domain notation)
- **widgetKey**: Short key used in code and file paths (no spaces)
- **displayName**: Human-readable name shown in UI
- **entry**: DLL filename (e.g., `"MyWidget.dll"`) - NOT a type name
- **category**: Widget category (`"general"`, `"productivity"`, `"utilities"`, etc.)
- **defaultSize**: Initial window dimensions

## Installation Paths

### Development Testing
```
%APPDATA%\3SC\Widgets\Community\{widget-key}\
```

Example:
```
C:\Users\{Username}\AppData\Roaming\3SC\Widgets\Community\clock\
```

### Production Distribution
Package as `.3scwidget` file (ZIP archive):

```powershell
# Create packaged version
$zipPath = "$packagesDir\widget-name.3scwidget"
Compress-Archive -Path "$unpackagedDir\*" -DestinationPath $zipPath -Force
```

## Deployment Commands

### Manual Copy
```powershell
xcopy /E /I /Y ".\packages\{widget-key}" "%APPDATA%\3SC\Widgets\Community\{widget-key}"
```

### Automated Installation Script
```powershell
$communityPath = "$env:APPDATA\3SC\Widgets\Community"
$widgetKey = "my-widget"
$targetPath = "$communityPath\$widgetKey"

# Ensure directory exists
New-Item -ItemType Directory -Force -Path $targetPath | Out-Null

# Copy widget files
Copy-Item "packages\$widgetKey\*" -Destination $targetPath -Recurse -Force

Write-Host "Widget installed to: $targetPath"
```

## Project Configuration

### .csproj Settings
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Dual mode: WinExe for Debug, Library for Release -->
    <OutputType Condition="'$(Configuration)' == 'Debug'">WinExe</OutputType>
    <OutputType Condition="'$(Configuration)' != 'Debug'">Library</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference the contracts project -->
    <ProjectReference Include="..\..\3SC\3SC.Widgets.Contracts\3SC.Widgets.Contracts.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Copy manifest.json to output -->
    <None Update="manifest.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

## Verification Checklist

Before deploying a widget:

- [ ] Manifest.json `entry` field is DLL filename, not type name
- [ ] `widgetKey` in manifest matches `WidgetKey` property in code
- [ ] All required DLLs copied (widget DLL + Contracts DLL)
- [ ] manifest.json copied to output directory
- [ ] Widget builds without errors in Release mode
- [ ] Assembly contains a class implementing `IWidgetFactory`

## Testing Workflow

1. **Build the widget**
   ```powershell
   dotnet build -c Release
   ```

2. **Copy to Community folder**
   ```powershell
   xcopy /E /I /Y "bin\Release\net8.0-windows" "%APPDATA%\3SC\Widgets\Community\{widget-key}"
   ```

3. **Restart 3SC app**
   - Widget appears in Widget Library
   - Click "Add Widget" to test

4. **Debug loading issues**
   - Check 3SC logs for errors
   - Verify manifest.json syntax
   - Confirm DLL files present
   - Validate IWidgetFactory implementation

## Common Deployment Errors

### "Widget DLL not found"
- Check `entry` field in manifest.json matches actual DLL filename
- Verify DLL was copied to Community folder

### "No IWidgetFactory implementation found"
- Ensure widget class implements `IWidgetFactory`
- Check namespace and class visibility (must be public)

### Widget doesn't appear in library
- Restart 3SC app after copying files
- Check manifest.json for syntax errors
- Verify folder name matches `widgetKey`
