# Build and Package Clock Widget
# Creates both packaged (.3scwidget) and unpackaged versions for testing

param(
    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Paths
$WidgetName = "Clock"
$WidgetProjectPath = ".\3SC.Widgets.$WidgetName"
$PackagesPath = ".\packages"
$UnpackagedPath = Join-Path $PackagesPath "clock-external"
$PackageFile = Join-Path $PackagesPath "clock-widget.3scwidget"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Clock Widget" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verify widget project exists
if (-not (Test-Path $WidgetProjectPath)) {
    Write-Error "Widget project not found: $WidgetProjectPath"
    exit 1
}

# Build widget
Write-Host "`nBuilding widget in $Configuration mode..." -ForegroundColor Yellow
dotnet build $WidgetProjectPath -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Find output DLL
$BinPath = Join-Path $WidgetProjectPath "bin\$Configuration\net8.0-windows"
$WidgetDll = Join-Path $BinPath "3SC.Widgets.$WidgetName.dll"

if (-not (Test-Path $WidgetDll)) {
    Write-Error "Widget DLL not found: $WidgetDll"
    exit 1
}

# Verify required files
Write-Host "`nVerifying package contents..." -ForegroundColor Yellow

$RequiredFiles = @(
    "3SC.Widgets.$WidgetName.dll",
    "manifest.json"
)

foreach ($file in $RequiredFiles) {
    $filePath = Join-Path $BinPath $file
    if (Test-Path $filePath) {
        Write-Host "  [OK] Found: $file" -ForegroundColor Green
    }
    else {
        Write-Error "Missing required file: $file"
        exit 1
    }
}

# Create packages directory
if (-not (Test-Path $PackagesPath)) {
    New-Item -ItemType Directory -Path $PackagesPath | Out-Null
    Write-Host "`nCreated packages directory" -ForegroundColor Green
}

# Files to include in package
$FilesToPackage = @(
    "*.dll",
    "manifest.json",
    "*.json"
)

# ============================================================================
# CREATE UNPACKAGED VERSION (for direct copying to community folder)
# ============================================================================

Write-Host "`nCreating unpackaged version for testing..." -ForegroundColor Yellow

# Remove existing unpackaged folder
if (Test-Path $UnpackagedPath) {
    Remove-Item $UnpackagedPath -Recurse -Force
}

# Create unpackaged folder
New-Item -ItemType Directory -Path $UnpackagedPath -Force | Out-Null

# Copy files to unpackaged folder
foreach ($pattern in $FilesToPackage) {
    Get-ChildItem -Path $BinPath -Filter $pattern -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $UnpackagedPath
        Write-Host "  Copied: $($_.Name)" -ForegroundColor Gray
    }
}

Write-Host "`nUnpackaged version created at: $UnpackagedPath" -ForegroundColor Green

# Show unpackaged contents
Write-Host "`nUnpackaged folder contents:" -ForegroundColor Cyan
Get-ChildItem $UnpackagedPath | ForEach-Object { 
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# ============================================================================
# CREATE PACKAGED VERSION (.3scwidget file)
# ============================================================================

Write-Host "`nCreating packaged version (.3scwidget)..." -ForegroundColor Yellow

# Remove existing package file
if (Test-Path $PackageFile) {
    Remove-Item $PackageFile -Force
}

# Create temp folder for packaging
$TempPackageFolder = Join-Path $env:TEMP "3SC_Widget_Package_Clock"
if (Test-Path $TempPackageFolder) {
    Remove-Item $TempPackageFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $TempPackageFolder | Out-Null

# Copy files to temp folder
foreach ($pattern in $FilesToPackage) {
    Get-ChildItem -Path $BinPath -Filter $pattern -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $TempPackageFolder
    }
}

# Create ZIP package
$TempZip = "$PackageFile.zip"
Compress-Archive -Path "$TempPackageFolder\*" -DestinationPath $TempZip -Force
Remove-Item $TempPackageFolder -Recurse -Force

# Rename to .3scwidget
Move-Item $TempZip $PackageFile -Force

$PackageSize = [math]::Round((Get-Item $PackageFile).Length / 1024, 2)
Write-Host "Package created successfully!" -ForegroundColor Green
Write-Host "Location: $PackageFile" -ForegroundColor Cyan
Write-Host "Size: $PackageSize KB" -ForegroundColor Cyan

# Show package contents
Write-Host "`nPackage contents:" -ForegroundColor Cyan
Add-Type -AssemblyName System.IO.Compression.FileSystem
$PackageFileFullPath = (Resolve-Path $PackageFile).Path
$zip = [System.IO.Compression.ZipFile]::OpenRead($PackageFileFullPath)
$zip.Entries | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
$zip.Dispose()

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Clock Widget Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[PACKAGED VERSION]:" -ForegroundColor White
Write-Host "   $PackageFile" -ForegroundColor Cyan
Write-Host "   (Ready for distribution)" -ForegroundColor Gray

Write-Host "`n[UNPACKAGED VERSION]:" -ForegroundColor White
Write-Host "   $UnpackagedPath" -ForegroundColor Cyan
Write-Host "   (Ready to copy to Community folder)" -ForegroundColor Gray

Write-Host "`n[TO INSTALL FOR TESTING]:" -ForegroundColor Yellow
Write-Host "   1. Copy the entire folder:" -ForegroundColor White
Write-Host "      From: $UnpackagedPath" -ForegroundColor Cyan
Write-Host "      To:   %APPDATA%\3SC\Widgets\Community\clock-external" -ForegroundColor Cyan
Write-Host "`n   2. OR run this command:" -ForegroundColor White
Write-Host "      xcopy /E /I /Y `"$UnpackagedPath`" `"%APPDATA%\3SC\Widgets\Community\clock-external`"" -ForegroundColor Cyan
Write-Host "`n   3. Then restart 3SC app to see the widget" -ForegroundColor White

Write-Host ""
