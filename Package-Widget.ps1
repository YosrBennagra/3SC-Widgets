param(
    [Parameter(Mandatory=$true)]
    [string]$WidgetName,
    
    [Parameter(Mandatory=$false)]
    [switch]$Install,
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\packages"
)

$ErrorActionPreference = "Stop"

# Paths
$WidgetProjectPath = ".\3SC.Widgets.$WidgetName"
$CommunityPath = "$env:APPDATA\3SC\Widgets\Community\$($WidgetName.ToLower())-external"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building External Widget: $WidgetName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verify widget project exists
if (-not (Test-Path $WidgetProjectPath)) {
    Write-Error "Widget project not found: $WidgetProjectPath"
    Write-Host "`nAvailable widgets:" -ForegroundColor Yellow
    Get-ChildItem -Directory "3SC.Widgets.*" | ForEach-Object { Write-Host "  - $($_.Name)" }
    exit 1
}

# Build widget in Release mode (creates DLL)
Write-Host "`nBuilding widget..." -ForegroundColor Yellow
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
    "manifest.json",
    "3SC.Widgets.Contracts.dll",
    "3SC.Domain.dll"
)

foreach ($file in $RequiredFiles) {
    $filePath = Join-Path $BinPath $file
    if (Test-Path $filePath) {
        Write-Host "  Found: $file" -ForegroundColor Green
    } else {
        Write-Error "Missing required file: $file"
        exit 1
    }
}

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Package name
$PackageName = "$($WidgetName.ToLower())-widget"
$PackageFile = Join-Path (Resolve-Path $OutputPath).Path "$PackageName.3scwidget"

# Create ZIP package
Write-Host "`nPackaging widget..." -ForegroundColor Yellow
if (Test-Path $PackageFile) {
    Remove-Item $PackageFile -Force
}

# Only package necessary files (exclude Debug symbols, etc.)
$FilesToPackage = @(
    "*.dll",
    "manifest.json",
    "*.json",
    "*.xaml"
)

$TempPackageFolder = Join-Path $env:TEMP "3SC_Widget_Package_$WidgetName"
if (Test-Path $TempPackageFolder) {
    Remove-Item $TempPackageFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $TempPackageFolder | Out-Null

# Copy only necessary files
foreach ($pattern in $FilesToPackage) {
    Get-ChildItem -Path $BinPath -Filter $pattern -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $TempPackageFolder
    }
}

# Create the package
$TempZip = "$PackageFile.zip"
Compress-Archive -Path "$TempPackageFolder\*" -DestinationPath $TempZip -Force
Remove-Item $TempPackageFolder -Recurse -Force

# Rename to .3scwidget
if (Test-Path $PackageFile) {
    Remove-Item $PackageFile -Force
}
Move-Item $TempZip $PackageFile -Force

$PackageSize = (Get-Item $PackageFile).Length / 1KB
Write-Host "Package created: $PackageFile ($([math]::Round($PackageSize, 2)) KB)" -ForegroundColor Green

# Show package contents
Write-Host "`nPackage contents:" -ForegroundColor Cyan
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($PackageFile)
$zip.Entries | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
$zip.Dispose()

# Install to Community folder if requested
if ($Install) {
    Write-Host "`nInstalling to Community folder..." -ForegroundColor Yellow
    
    # Create Community folder if it doesn't exist
    $CommunityRoot = "$env:APPDATA\3SC\Widgets\Community"
    if (-not (Test-Path $CommunityRoot)) {
        New-Item -ItemType Directory -Path $CommunityRoot -Force | Out-Null
    }
    
    # Remove existing installation
    if (Test-Path $CommunityPath) {
        Write-Host "  Removing existing installation..." -ForegroundColor Gray
        Remove-Item $CommunityPath -Recurse -Force
    }
    
    # Create widget folder
    New-Item -ItemType Directory -Path $CommunityPath -Force | Out-Null
    
    # Extract package to Community folder
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($PackageFile, $CommunityPath)
    
    Write-Host "Widget installed to: $CommunityPath" -ForegroundColor Green
    
    # Show installed files
    Write-Host "`nInstalled files:" -ForegroundColor Cyan
    Get-ChildItem $CommunityPath | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Widget Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nPackage: " -NoNewline -ForegroundColor White
Write-Host "$PackageFile" -ForegroundColor Cyan

if (-not $Install) {
    Write-Host "`nTo install, run:" -ForegroundColor Yellow
    Write-Host "  .\Package-Widget.ps1 -WidgetName $WidgetName -Install" -ForegroundColor White
}

Write-Host "`nTo test in main app:" -ForegroundColor Yellow
Write-Host "  1. Run the main 3SC app" -ForegroundColor White
Write-Host "  2. Widget will be discovered automatically from Community folder" -ForegroundColor White
Write-Host '  3. Or use test mode: 3SC.UI.exe --test-external' -ForegroundColor White

Write-Host ""
