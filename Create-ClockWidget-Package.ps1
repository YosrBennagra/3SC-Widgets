# Build and Package Clock Widget Script
# Creates a complete .3scwidget package with DLL and dependencies

param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".\packages"
)

$ErrorActionPreference = "Stop"

# Configuration
$WidgetName = "Clock"
$ProjectName = "3SC.Widgets.Clock"
$PackageId = "com.3sc.clock"
$PackageDir = Join-Path $env:TEMP "$PackageId-temp"
$PackageFile = Join-Path $OutputPath "$WidgetName.3scwidget"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Building Clock Widget Package" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build the widget
Write-Host "[1/6] Building widget project..." -ForegroundColor Yellow
Push-Location "3SC.Widgets.Clock"
try {
    dotnet build "$ProjectName.csproj" -c Release --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[+] Build successful" -ForegroundColor Green
}
finally {
    Pop-Location
}
Write-Host ""

# Step 2: Create package directory
Write-Host "[2/6] Creating package directory..." -ForegroundColor Yellow
if (Test-Path $PackageDir) {
    Remove-Item -Path $PackageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PackageDir -Force | Out-Null
Write-Host "[+] Package directory created" -ForegroundColor Green
Write-Host ""

# Step 3: Copy DLL and dependencies
Write-Host "[3/6] Copying widget files..." -ForegroundColor Yellow
$BinPath = "3SC.Widgets.Clock\bin\Release\net8.0-windows"

# Required files
$RequiredFiles = @(
    "$ProjectName.dll",
    "manifest.json"
)

# Optional dependency files (copy if they exist)
$DependencyFiles = @(
    "3SC.Widgets.Contracts.dll",
    "3SC.Domain.dll",
    "CommunityToolkit.Mvvm.dll"
)

foreach ($file in $RequiredFiles) {
    $sourcePath = Join-Path $BinPath $file
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath -Destination $PackageDir
        Write-Host "  [+] Copied $file" -ForegroundColor Gray
    }
    else {
        throw "Required file not found: $file"
    }
}

foreach ($file in $DependencyFiles) {
    $sourcePath = Join-Path $BinPath $file
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath -Destination $PackageDir
        Write-Host "  [+] Copied $file" -ForegroundColor Gray
    }
}

Write-Host "[+] Files copied successfully" -ForegroundColor Green
Write-Host ""

# Step 4: Create README.md
Write-Host "[4/6] Generating README.md..." -ForegroundColor Yellow
$ReadmeContent = @"
# Digital Clock Widget

A simple, elegant digital clock widget for 3SC desktop application.

## Features

* Real-time clock display with automatic updates
* 12/24 hour format toggle
* Date display with full day and date
* Timezone support with customizable zones
* Clean, modern UI with theme support
* Lightweight and efficient

## Installation

1. Download the Clock.3scwidget file
2. Place in: ``%APPDATA%\3SC\Widgets\Community\$PackageId\``
3. Restart 3SC application
4. Add widget from the widget gallery

## Usage

1. Open 3SC application
2. Navigate to widget gallery
3. Find "Digital Clock" in General category
4. Click to add widget to your desktop
5. Right-click widget to access settings

## Settings

* **Time Format**: Toggle between 12-hour and 24-hour format
* **Show Seconds**: Display or hide seconds
* **Show Date**: Display or hide the date
* **Timezone**: Select from available timezones
* **Show Timezone Label**: Display or hide timezone name

## Requirements

* 3SC Desktop Application v1.0.0 or higher
* Windows 10/11
* .NET 8.0 Runtime

## Support

For issues or feature requests:
* Email: team@3sc.app
* GitHub: https://github.com/YosrBennagra/3SC-Widgets

## Version

1.0.0

## License

Copyright 2026 3SC Team. All rights reserved.
"@

$ReadmePath = Join-Path $PackageDir "README.md"
[System.IO.File]::WriteAllText($ReadmePath, $ReadmeContent, [System.Text.UTF8Encoding]::new($false))
Write-Host "[+] README.md created" -ForegroundColor Green
Write-Host ""

# Step 5: Create the package
Write-Host "[5/6] Creating .3scwidget package..." -ForegroundColor Yellow

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Remove existing package
if (Test-Path $PackageFile) {
    Remove-Item $PackageFile -Force
}

# Create ZIP and rename to .3scwidget
$TempZipFile = [System.IO.Path]::ChangeExtension($PackageFile, '.zip')
if (Test-Path $TempZipFile) {
    Remove-Item $TempZipFile -Force
}

Compress-Archive -Path "$PackageDir\*" -DestinationPath $TempZipFile -CompressionLevel Optimal -Force
Move-Item -Path $TempZipFile -Destination $PackageFile -Force

Write-Host "[+] Package created successfully" -ForegroundColor Green
Write-Host ""

# Step 6: Display package contents
Write-Host "[6/6] Package contents:" -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($PackageFile)
foreach ($entry in $zip.Entries | Sort-Object Name) {
    $size = if ($entry.Length -lt 1KB) { 
        "$($entry.Length) B" 
    }
    elseif ($entry.Length -lt 1MB) {
        "$([math]::Round($entry.Length/1KB, 2)) KB"
    }
    else {
        "$([math]::Round($entry.Length/1MB, 2)) MB"
    }
    Write-Host "  * $($entry.Name)" -ForegroundColor White -NoNewline
    Write-Host " ($size)" -ForegroundColor Gray
}
$zip.Dispose()

# Cleanup temp directory
Remove-Item -Path $PackageDir -Recurse -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   Package Created Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

$FinalSize = (Get-Item $PackageFile).Length
Write-Host "Package Location:" -ForegroundColor Cyan
Write-Host "  $PackageFile" -ForegroundColor White
Write-Host ""
Write-Host "Package Size:" -ForegroundColor Cyan
if ($FinalSize -lt 1MB) {
    Write-Host "  $([math]::Round($FinalSize/1KB, 2)) KB" -ForegroundColor White
}
else {
    Write-Host "  $([math]::Round($FinalSize/1MB, 2)) MB" -ForegroundColor White
}
Write-Host ""
Write-Host "Installation Path:" -ForegroundColor Cyan
Write-Host "  %APPDATA%\3SC\Widgets\Community\$PackageId\" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Install Widget:" -ForegroundColor Yellow
Write-Host "   Copy to: %APPDATA%\3SC\Widgets\Community\$PackageId\" -ForegroundColor White
Write-Host ""
Write-Host "2. Quick Install Command:" -ForegroundColor Yellow
Write-Host "   `$dest = `"`$env:APPDATA\3SC\Widgets\Community\$PackageId`"" -ForegroundColor Gray
Write-Host "   New-Item -ItemType Directory -Path `$dest -Force | Out-Null" -ForegroundColor Gray
Write-Host "   Expand-Archive -Path '$PackageFile' -DestinationPath `$dest -Force" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Test in 3SC:" -ForegroundColor Yellow
Write-Host "   - Restart 3SC application" -ForegroundColor White
Write-Host "   - Open widget gallery (General category)" -ForegroundColor White
Write-Host "   - Add Digital Clock widget" -ForegroundColor White
Write-Host ""
