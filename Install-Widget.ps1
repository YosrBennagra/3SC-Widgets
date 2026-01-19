<#
.SYNOPSIS
    Installs a 3SC widget package to the local 3SC application.

.DESCRIPTION
    Extracts a .3scwidget package to the 3SC Community widgets folder.
    The widget will be available after restarting 3SC.

.PARAMETER PackagePath
    Path to the .3scwidget file to install

.PARAMETER WidgetKey
    Widget key to install from the packages folder (e.g., "clock")

.PARAMETER List
    List all installed widgets

.PARAMETER Uninstall
    Uninstall a widget by key

.EXAMPLE
    .\Install-Widget.ps1 -PackagePath ".\packages\clock-widget.3scwidget"
    
.EXAMPLE
    .\Install-Widget.ps1 -WidgetKey clock
    
.EXAMPLE
    .\Install-Widget.ps1 -List
    
.EXAMPLE
    .\Install-Widget.ps1 -Uninstall -WidgetKey clock
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$PackagePath,
    
    [Parameter(Mandatory = $false)]
    [string]$WidgetKey,
    
    [Parameter(Mandatory = $false)]
    [switch]$List,
    
    [Parameter(Mandatory = $false)]
    [switch]$Uninstall
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PackagesDir = Join-Path $ScriptDir "packages"
$WidgetsInstallDir = Join-Path $env:APPDATA "3SC\Widgets\Community"

# Ensure install directory exists
if (-not (Test-Path $WidgetsInstallDir)) {
    New-Item -ItemType Directory -Path $WidgetsInstallDir -Force | Out-Null
}

function Get-InstalledWidgets {
    $widgets = @()
    if (Test-Path $WidgetsInstallDir) {
        Get-ChildItem $WidgetsInstallDir -Directory | ForEach-Object {
            $manifestPath = Join-Path $_.FullName "manifest.json"
            if (Test-Path $manifestPath) {
                $manifest = Get-Content $manifestPath | ConvertFrom-Json
                $widgets += [PSCustomObject]@{
                    Key = $_.Name
                    Name = $manifest.name
                    Version = $manifest.version
                    Path = $_.FullName
                }
            } else {
                $widgets += [PSCustomObject]@{
                    Key = $_.Name
                    Name = "(unknown)"
                    Version = "?"
                    Path = $_.FullName
                }
            }
        }
    }
    return $widgets
}

function Install-WidgetPackage {
    param([string]$Package)
    
    if (-not (Test-Path $Package)) {
        Write-Host "Package not found: $Package" -ForegroundColor Red
        return $false
    }
    
    # Extract to temp to read manifest
    $tempDir = Join-Path $env:TEMP "3sc-widget-install-$(Get-Random)"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    
    # Rename .3scwidget to .zip temporarily for extraction
    $tempZip = Join-Path $env:TEMP "widget-temp-$(Get-Random).zip"
    Copy-Item $Package $tempZip -Force
    
    try {
        Write-Host "Extracting package..." -ForegroundColor Yellow
        Expand-Archive -Path $tempZip -DestinationPath $tempDir -Force
        Remove-Item $tempZip -Force
        
        # Read manifest to get widget key
        $manifestPath = Join-Path $tempDir "manifest.json"
        if (-not (Test-Path $manifestPath)) {
            Write-Host "Invalid package: No manifest.json found" -ForegroundColor Red
            return $false
        }
        
        $manifest = Get-Content $manifestPath | ConvertFrom-Json
        $widgetKey = $manifest.widgetKey
        $widgetName = $manifest.name
        $widgetVersion = $manifest.version
        
        if (-not $widgetKey) {
            Write-Host "Invalid manifest: Missing widgetKey" -ForegroundColor Red
            return $false
        }
        
        # Install to destination
        $destDir = Join-Path $WidgetsInstallDir $widgetKey
        
        if (Test-Path $destDir) {
            Write-Host "Updating existing widget: $widgetKey" -ForegroundColor Yellow
            Remove-Item $destDir -Recurse -Force
        }
        
        # Move from temp to final location
        Move-Item $tempDir $destDir -Force
        
        Write-Host "`n✅ Widget installed successfully!" -ForegroundColor Green
        Write-Host "   Name: $widgetName" -ForegroundColor Gray
        Write-Host "   Version: $widgetVersion" -ForegroundColor Gray
        Write-Host "   Key: $widgetKey" -ForegroundColor Gray
        Write-Host "   Path: $destDir" -ForegroundColor Gray
        Write-Host "`nRestart 3SC to use the widget." -ForegroundColor Cyan
        
        return $true
    }
    catch {
        Write-Host "Installation failed: $_" -ForegroundColor Red
        return $false
    }
    finally {
        # Clean up temp if still exists
        if (Test-Path $tempDir) {
            Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

function Uninstall-Widget {
    param([string]$Key)
    
    $widgetDir = Join-Path $WidgetsInstallDir $Key
    
    if (-not (Test-Path $widgetDir)) {
        Write-Host "Widget not installed: $Key" -ForegroundColor Red
        return $false
    }
    
    Remove-Item $widgetDir -Recurse -Force
    Write-Host "✅ Widget uninstalled: $Key" -ForegroundColor Green
    return $true
}

# Main execution
if ($List) {
    $widgets = Get-InstalledWidgets
    
    if ($widgets.Count -eq 0) {
        Write-Host "No widgets installed." -ForegroundColor Gray
        Write-Host "`nInstall widgets with:" -ForegroundColor Yellow
        Write-Host "  .\Install-Widget.ps1 -WidgetKey clock" -ForegroundColor Gray
    } else {
        Write-Host "`nInstalled Widgets:" -ForegroundColor Cyan
        Write-Host "==================" -ForegroundColor Cyan
        $widgets | ForEach-Object {
            Write-Host "`n  $($_.Name)" -ForegroundColor White
            Write-Host "    Key: $($_.Key)" -ForegroundColor Gray
            Write-Host "    Version: $($_.Version)" -ForegroundColor Gray
        }
        Write-Host ""
    }
    
} elseif ($Uninstall -and $WidgetKey) {
    Uninstall-Widget -Key $WidgetKey
    
} elseif ($PackagePath) {
    Install-WidgetPackage -Package $PackagePath
    
} elseif ($WidgetKey) {
    # Install from packages folder
    $packagePath = Join-Path $PackagesDir "$WidgetKey-widget.3scwidget"
    if (-not (Test-Path $packagePath)) {
        Write-Host "Package not found: $packagePath" -ForegroundColor Red
        Write-Host "`nAvailable packages:" -ForegroundColor Yellow
        Get-ChildItem $PackagesDir -Filter "*.3scwidget" | ForEach-Object {
            $key = $_.Name -replace "-widget\.3scwidget$", ""
            Write-Host "  - $key" -ForegroundColor Gray
        }
        exit 1
    }
    Install-WidgetPackage -Package $packagePath
    
} else {
    Write-Host "3SC Widget Installer" -ForegroundColor Cyan
    Write-Host "====================" -ForegroundColor Cyan
    Write-Host "`nUsage:" -ForegroundColor Yellow
    Write-Host "  .\Install-Widget.ps1 -WidgetKey clock           # Install from packages folder" -ForegroundColor Gray
    Write-Host "  .\Install-Widget.ps1 -PackagePath .\my.3scwidget # Install from file" -ForegroundColor Gray
    Write-Host "  .\Install-Widget.ps1 -List                       # List installed widgets" -ForegroundColor Gray
    Write-Host "  .\Install-Widget.ps1 -Uninstall -WidgetKey clock # Uninstall widget" -ForegroundColor Gray
    Write-Host "`nAvailable packages:" -ForegroundColor Yellow
    Get-ChildItem $PackagesDir -Filter "*.3scwidget" -ErrorAction SilentlyContinue | ForEach-Object {
        $key = $_.Name -replace "-widget\.3scwidget$", ""
        Write-Host "  - $key" -ForegroundColor Gray
    }
}
