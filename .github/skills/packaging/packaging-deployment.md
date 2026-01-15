# Packaging & Deployment

> **Category:** Packaging | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers creating distributable widget packages (.3scwidget) and deploying them to the 3SC host application.

## Prerequisites

- [build-configuration.md](build-configuration.md)
- [manifest-specification.md](../core/manifest-specification.md)

---

## Package Format

The `.3scwidget` format is a ZIP archive containing:

```
my-widget.3scwidget
├── manifest.json           # Widget metadata (REQUIRED)
├── 3SC.Widgets.MyWidget.dll  # Widget assembly (REQUIRED)
├── 3SC.Widgets.MyWidget.pdb  # Debug symbols (optional)
├── Dependencies/           # Third-party dependencies
│   └── *.dll
└── Assets/                 # Static resources
    ├── icon.png           # Widget icon (128x128)
    ├── preview.png        # Preview image (optional)
    └── ...
```

---

## Installation Paths

```
%APPDATA%\3SC\Widgets\
├── Bundled\              # Pre-installed widgets (read-only)
│   └── clock\
└── Community\            # User-installed widgets
    └── my-widget\
        ├── manifest.json
        ├── 3SC.Widgets.MyWidget.dll
        └── Assets\
```

---

## Packaging Script

### package.ps1

```powershell
#!/usr/bin/env pwsh
#Requires -Version 7.0

<#
.SYNOPSIS
    Packages a widget for distribution.

.DESCRIPTION
    Creates a .3scwidget package from the build output.

.PARAMETER Configuration
    Build configuration (Debug/Release). Default: Release

.PARAMETER OutputPath
    Where to create the package. Default: current directory

.EXAMPLE
    ./package.ps1 -Configuration Release
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [string]$OutputPath = '.'
)

$ErrorActionPreference = 'Stop'

# Read manifest
$manifest = Get-Content 'manifest.json' | ConvertFrom-Json
$widgetKey = $manifest.widgetKey
$version = $manifest.version
$displayName = $manifest.displayName

Write-Host "Packaging: $displayName v$version" -ForegroundColor Cyan

# Paths
$buildOutput = "bin\$Configuration\net8.0-windows"
$packageName = "$widgetKey-$version.3scwidget"
$stagingDir = "staging\$widgetKey"

# Validate build output
if (-not (Test-Path $buildOutput)) {
    Write-Host "Build output not found. Running build..." -ForegroundColor Yellow
    dotnet build -c $Configuration
}

# Clean staging
if (Test-Path $stagingDir) {
    Remove-Item $stagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingDir | Out-Null

# Copy required files
Write-Host "Staging files..." -ForegroundColor Gray

# Manifest
Copy-Item 'manifest.json' $stagingDir

# Main assembly
$mainAssembly = Get-ChildItem "$buildOutput\3SC.Widgets.*.dll" -Exclude "*Contracts*" | Select-Object -First 1
if (-not $mainAssembly) {
    throw "Main assembly not found in $buildOutput"
}
Copy-Item $mainAssembly.FullName $stagingDir

# Optional: Include PDB for Release (smaller)
if ($Configuration -eq 'Debug') {
    $pdb = $mainAssembly.FullName -replace '\.dll$', '.pdb'
    if (Test-Path $pdb) {
        Copy-Item $pdb $stagingDir
    }
}

# Dependencies (exclude contracts & framework)
$excludePatterns = @(
    '*Contracts*',
    'Microsoft.*',
    'System.*',
    'WindowsBase*',
    'PresentationCore*',
    'PresentationFramework*'
)

$deps = Get-ChildItem "$buildOutput\*.dll" | Where-Object {
    $name = $_.Name
    $exclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($name -like $pattern) {
            $exclude = $true
            break
        }
    }
    -not $exclude -and $_.Name -ne $mainAssembly.Name
}

if ($deps) {
    $depsDir = Join-Path $stagingDir 'Dependencies'
    New-Item -ItemType Directory -Path $depsDir | Out-Null
    
    foreach ($dep in $deps) {
        Copy-Item $dep.FullName $depsDir
        Write-Host "  + $($dep.Name)" -ForegroundColor Gray
    }
}

# Assets
if (Test-Path 'Assets') {
    Copy-Item 'Assets' $stagingDir -Recurse
}

# Create package
$packagePath = Join-Path $OutputPath $packageName
if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force
}

Write-Host "Creating package: $packageName" -ForegroundColor Gray

Compress-Archive -Path "$stagingDir\*" -DestinationPath $packagePath -CompressionLevel Optimal

# Cleanup
Remove-Item 'staging' -Recurse -Force

# Summary
$packageSize = (Get-Item $packagePath).Length / 1KB
Write-Host ""
Write-Host "Package created successfully!" -ForegroundColor Green
Write-Host "  Path: $packagePath" -ForegroundColor Gray
Write-Host "  Size: $([math]::Round($packageSize, 1)) KB" -ForegroundColor Gray

# Return path for scripts
return $packagePath
```

---

## Installation Script

### install.ps1

```powershell
#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Installs a widget package.

.PARAMETER PackagePath
    Path to the .3scwidget file

.PARAMETER Force
    Overwrite if already installed
#>

param(
    [Parameter(Mandatory)]
    [string]$PackagePath,
    
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $PackagePath)) {
    throw "Package not found: $PackagePath"
}

# Extract to temp
$tempDir = Join-Path $env:TEMP "3sc-install-$(Get-Random)"
Expand-Archive -Path $PackagePath -DestinationPath $tempDir

try {
    # Read manifest
    $manifestPath = Join-Path $tempDir 'manifest.json'
    if (-not (Test-Path $manifestPath)) {
        throw "Invalid package: missing manifest.json"
    }
    
    $manifest = Get-Content $manifestPath | ConvertFrom-Json
    $widgetKey = $manifest.widgetKey
    $version = $manifest.version
    
    Write-Host "Installing: $($manifest.displayName) v$version" -ForegroundColor Cyan
    
    # Target path
    $installPath = Join-Path $env:APPDATA "3SC\Widgets\Community\$widgetKey"
    
    # Check existing
    if ((Test-Path $installPath) -and -not $Force) {
        $existingManifest = Get-Content (Join-Path $installPath 'manifest.json') | ConvertFrom-Json
        throw "Widget already installed (v$($existingManifest.version)). Use -Force to overwrite."
    }
    
    # Create/clean target
    if (Test-Path $installPath) {
        Remove-Item $installPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $installPath | Out-Null
    
    # Copy files
    Copy-Item "$tempDir\*" $installPath -Recurse
    
    Write-Host "Installed to: $installPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Restart 3SC to load the widget." -ForegroundColor Yellow
}
finally {
    # Cleanup temp
    Remove-Item $tempDir -Recurse -Force
}
```

---

## Uninstallation

### uninstall.ps1

```powershell
#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$WidgetKey
)

$installPath = Join-Path $env:APPDATA "3SC\Widgets\Community\$WidgetKey"

if (-not (Test-Path $installPath)) {
    throw "Widget not installed: $WidgetKey"
}

$manifest = Get-Content (Join-Path $installPath 'manifest.json') | ConvertFrom-Json
Write-Host "Uninstalling: $($manifest.displayName)" -ForegroundColor Cyan

# Remove files
Remove-Item $installPath -Recurse -Force

# Optionally remove data
$dataPath = Join-Path $env:APPDATA "3SC\WidgetData\$WidgetKey"
if (Test-Path $dataPath) {
    $response = Read-Host "Remove widget data? (y/N)"
    if ($response -eq 'y') {
        Remove-Item $dataPath -Recurse -Force
        Write-Host "Data removed." -ForegroundColor Gray
    }
}

Write-Host "Uninstalled successfully!" -ForegroundColor Green
```

---

## Validation

### validate-package.ps1

```powershell
#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$PackagePath
)

$errors = @()
$warnings = @()

Write-Host "Validating: $PackagePath" -ForegroundColor Cyan

$tempDir = Join-Path $env:TEMP "3sc-validate-$(Get-Random)"
Expand-Archive -Path $PackagePath -DestinationPath $tempDir

try {
    # Check manifest
    $manifestPath = Join-Path $tempDir 'manifest.json'
    if (-not (Test-Path $manifestPath)) {
        $errors += "Missing manifest.json"
    }
    else {
        $manifest = Get-Content $manifestPath | ConvertFrom-Json
        
        # Required fields
        @('widgetKey', 'displayName', 'version', 'entryPoint') | ForEach-Object {
            if (-not $manifest.$_) {
                $errors += "Missing required field: $_"
            }
        }
        
        # Widget key format
        if ($manifest.widgetKey -and $manifest.widgetKey -notmatch '^[a-z][a-z0-9-]*$') {
            $errors += "Invalid widgetKey format (must be lowercase alphanumeric with dashes)"
        }
        
        # Version format
        if ($manifest.version -and $manifest.version -notmatch '^\d+\.\d+\.\d+') {
            $errors += "Invalid version format (must be semver)"
        }
        
        # Entry point exists
        if ($manifest.entryPoint) {
            $entryPath = Join-Path $tempDir $manifest.entryPoint
            if (-not (Test-Path $entryPath)) {
                $errors += "Entry point not found: $($manifest.entryPoint)"
            }
        }
        
        # Icon
        if (-not (Test-Path (Join-Path $tempDir 'Assets\icon.png'))) {
            $warnings += "Missing icon.png (128x128 recommended)"
        }
    }
    
    # Check assemblies
    $assemblies = Get-ChildItem $tempDir -Filter '*.dll' -Recurse
    if (-not $assemblies) {
        $errors += "No assemblies found"
    }
    
    # Check for forbidden dependencies
    $forbidden = @('3SC.Widgets.Contracts.dll')
    foreach ($asm in $assemblies) {
        if ($asm.Name -in $forbidden) {
            $errors += "Forbidden dependency included: $($asm.Name)"
        }
    }
}
finally {
    Remove-Item $tempDir -Recurse -Force
}

# Report
Write-Host ""
if ($warnings) {
    Write-Host "Warnings:" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
}

if ($errors) {
    Write-Host "Errors:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "Validation FAILED" -ForegroundColor Red
    exit 1
}
else {
    Write-Host "Validation PASSED" -ForegroundColor Green
}
```

---

## Package Contents Manifest

For transparency, you can include a CONTENTS.md:

```markdown
# Package Contents

## Widget: Clock Widget v1.2.0

### Files

| File | Size | Purpose |
|------|------|---------|
| manifest.json | 1.2 KB | Widget metadata |
| 3SC.Widgets.Clock.dll | 45 KB | Main widget assembly |
| Assets/icon.png | 4.5 KB | Widget icon (128x128) |
| Assets/preview.png | 12 KB | Preview image |

### Dependencies

| Package | Version | License |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.2.2 | MIT |
| Serilog | 3.1.1 | Apache-2.0 |

### Checksums (SHA256)

```
3SC.Widgets.Clock.dll: a1b2c3d4e5f6...
manifest.json: f6e5d4c3b2a1...
```
```

---

## Best Practices

1. **Exclude Contracts DLL** - Never include 3SC.Widgets.Contracts.dll
2. **Minimize size** - Only include necessary dependencies
3. **Include icon** - 128x128 PNG with transparency
4. **Validate before publish** - Run validate-package.ps1
5. **Sign packages** - For production, consider code signing

---

## Related Skills

- [build-configuration.md](build-configuration.md) - Build setup
- [versioning.md](versioning.md) - Version management
- [distribution.md](distribution.md) - Publishing widgets

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added validation, checksums |
| 1.0.0 | 2025-06-01 | Initial version |
