<#
.SYNOPSIS
    Builds and packages a 3SC widget for distribution.

.DESCRIPTION
    This script builds a widget in Release mode, publishes it with all dependencies,
    and creates a .3scwidget package ready for installation.

.PARAMETER WidgetName
    The name of the widget project folder (e.g., "3SC.Widgets.Clock")

.PARAMETER All
    Build all widgets in the solution

.EXAMPLE
    .\Build-Widget.ps1 -WidgetName "3SC.Widgets.Clock"
    
.EXAMPLE
    .\Build-Widget.ps1 -All
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$WidgetName,
    
    [Parameter(Mandatory = $false)]
    [switch]$All
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PackagesDir = Join-Path $ScriptDir "packages"

# Ensure packages directory exists
if (-not (Test-Path $PackagesDir)) {
    New-Item -ItemType Directory -Path $PackagesDir -Force | Out-Null
}

function Build-SingleWidget {
    param([string]$ProjectPath)
    
    $projectName = Split-Path -Leaf $ProjectPath
    $widgetKey = $projectName -replace "3SC\.Widgets\.", "" | ForEach-Object { $_.ToLower() }
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Building: $projectName" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    # Clean previous publish
    $publishDir = Join-Path $ProjectPath "publish"
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }
    
    # Build and publish
    Write-Host "`n[1/4] Publishing..." -ForegroundColor Yellow
    $result = dotnet publish $ProjectPath -c Release -o $publishDir 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        Write-Host $result
        return $false
    }
    Write-Host "Published successfully" -ForegroundColor Green
    
    # Verify manifest exists
    $manifestPath = Join-Path $publishDir "manifest.json"
    if (-not (Test-Path $manifestPath)) {
        # Copy from project root if not in publish
        $srcManifest = Join-Path $ProjectPath "manifest.json"
        if (Test-Path $srcManifest) {
            Copy-Item $srcManifest $publishDir -Force
        }
        else {
            Write-Host "Warning: No manifest.json found!" -ForegroundColor Yellow
        }
    }
    
    # Assets are automatically copied by dotnet publish (see .csproj)
    # No need to copy manually - doing so creates nested Assets/Assets/
    Write-Host "[2/4] Assets copied by build" -ForegroundColor Gray
    
    # Remove files that shouldn't be packaged
    Write-Host "[3/4] Cleaning package..." -ForegroundColor Yellow
    $filesToRemove = @(
        "3SC.Widgets.Contracts.dll",
        "3SC.Widgets.Contracts.pdb",
        "*.pdb"
    )
    foreach ($pattern in $filesToRemove) {
        Get-ChildItem $publishDir -Filter $pattern -ErrorAction SilentlyContinue | Remove-Item -Force
    }
    
    # Create .3scwidget package (zip)
    Write-Host "[4/4] Creating package..." -ForegroundColor Yellow
    $packageName = "$widgetKey-widget.3scwidget"
    $packagePath = Join-Path $PackagesDir $packageName
    $tempZipPath = Join-Path $PackagesDir "$widgetKey-widget.zip"
    
    if (Test-Path $packagePath) {
        Remove-Item $packagePath -Force
    }
    if (Test-Path $tempZipPath) {
        Remove-Item $tempZipPath -Force
    }
    
    # Create as .zip first, then rename to .3scwidget
    Compress-Archive -Path "$publishDir\*" -DestinationPath $tempZipPath -Force
    Rename-Item $tempZipPath $packageName -Force
    
    $packageSize = (Get-Item $packagePath).Length / 1KB
    Write-Host "`n✅ Package created: $packageName ($([math]::Round($packageSize, 1)) KB)" -ForegroundColor Green
    
    # List package contents
    Write-Host "`nPackage contents:" -ForegroundColor Gray
    $zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    $zip.Entries | Select-Object -First 15 | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
    if ($zip.Entries.Count -gt 15) {
        Write-Host "  ... and $($zip.Entries.Count - 15) more files" -ForegroundColor Gray
    }
    $zip.Dispose()
    
    return $true
}

# Main execution
Add-Type -AssemblyName System.IO.Compression.FileSystem

if ($All) {
    # Build all widgets
    $widgets = Get-ChildItem $ScriptDir -Directory -Filter "3SC.Widgets.*" | 
    Where-Object { $_.Name -ne "3SC.Widgets.Contracts" }
    
    Write-Host "`nBuilding all widgets..." -ForegroundColor Cyan
    Write-Host "Found $($widgets.Count) widgets" -ForegroundColor Gray
    
    $successful = 0
    $failed = 0
    
    foreach ($widget in $widgets) {
        if (Build-SingleWidget -ProjectPath $widget.FullName) {
            $successful++
        }
        else {
            $failed++
        }
    }
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Build Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Successful: $successful" -ForegroundColor Green
    if ($failed -gt 0) {
        Write-Host "Failed: $failed" -ForegroundColor Red
    }
    Write-Host "`nPackages saved to: $PackagesDir" -ForegroundColor Gray
    
}
elseif ($WidgetName) {
    # Build single widget
    $projectPath = Join-Path $ScriptDir $WidgetName
    if (-not (Test-Path $projectPath)) {
        Write-Host "Widget not found: $WidgetName" -ForegroundColor Red
        Write-Host "`nAvailable widgets:" -ForegroundColor Gray
        Get-ChildItem $ScriptDir -Directory -Filter "3SC.Widgets.*" | 
        Where-Object { $_.Name -ne "3SC.Widgets.Contracts" } |
        ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
        exit 1
    }
    
    Build-SingleWidget -ProjectPath $projectPath
    
}
else {
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\Build-Widget.ps1 -WidgetName '3SC.Widgets.Clock'" -ForegroundColor Gray
    Write-Host "  .\Build-Widget.ps1 -All" -ForegroundColor Gray
    Write-Host "`nAvailable widgets:" -ForegroundColor Yellow
    Get-ChildItem $ScriptDir -Directory -Filter "3SC.Widgets.*" | 
    Where-Object { $_.Name -ne "3SC.Widgets.Contracts" } |
    ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
}
