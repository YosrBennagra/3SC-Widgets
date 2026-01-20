#!/usr/bin/env pwsh
# Build and Package Script for This Day in History Widget
# This script builds the widget and packages it as a .3scwidget file

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\publish"
)

$ErrorActionPreference = "Stop"

Write-Host "Building This Day in History Widget..." -ForegroundColor Cyan

# Set paths
$ProjectDir = $PSScriptRoot
$ProjectFile = Join-Path $ProjectDir "3SC.Widgets.ThisDayInHistory.csproj"
$PublishDir = Join-Path $ProjectDir $OutputPath

# Clean previous build
if (Test-Path $PublishDir) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item $PublishDir -Recurse -Force
}

# Build the project
Write-Host "Building project..." -ForegroundColor Green
dotnet publish $ProjectFile `
    -c $Configuration `
    -o $PublishDir `
    --self-contained false `
    /p:PublishSingleFile=false `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Remove unnecessary files
Write-Host "Cleaning up unnecessary files..." -ForegroundColor Yellow
$FilesToRemove = @(
    "*.pdb",
    "*.xml",
    "3SC.Widgets.Contracts.dll"
)

foreach ($pattern in $FilesToRemove) {
    Get-ChildItem $PublishDir -Filter $pattern | Remove-Item -Force
}

# Create package
Write-Host "Creating widget package..." -ForegroundColor Green
$PackageName = "ThisDayInHistory.3scwidget"
$PackagePath = Join-Path $ProjectDir $PackageName
$TempZipPath = Join-Path $ProjectDir "ThisDayInHistory.zip"

if (Test-Path $TempZipPath) {
    Remove-Item $TempZipPath -Force
}
if (Test-Path $PackagePath) {
    Remove-Item $PackagePath -Force
}

# Create ZIP archive first, then rename to .3scwidget
Compress-Archive -Path "$PublishDir\*" -DestinationPath $TempZipPath -CompressionLevel Optimal
Rename-Item -Path $TempZipPath -NewName $PackageName

# Get package size
$PackageSize = (Get-Item $PackagePath).Length / 1KB
$PackageSizeRounded = [Math]::Round($PackageSize, 2)
Write-Host "Widget packaged successfully!" -ForegroundColor Green
Write-Host "Package: $PackagePath ($PackageSizeRounded KB)" -ForegroundColor Cyan

# Optional: Copy to 3SC Widgets folder for testing
$TestPath = Join-Path $env:APPDATA "3SC\Widgets\Community\this-day-in-history"
$CopyToTest = Read-Host "Copy to test folder ($TestPath)? (Y/N)"
if ($CopyToTest -eq 'Y' -or $CopyToTest -eq 'y') {
    if (!(Test-Path $TestPath)) {
        New-Item -ItemType Directory -Path $TestPath -Force | Out-Null
    }
    
    # Need to use .zip extension for Expand-Archive
    $TempExtractZip = Join-Path $ProjectDir "ThisDayInHistory_temp.zip"
    Copy-Item $PackagePath $TempExtractZip -Force
    
    # Extract package to test folder
    Expand-Archive -Path $TempExtractZip -DestinationPath $TestPath -Force
    Remove-Item $TempExtractZip -Force
    
    Write-Host "Copied to test folder. Restart 3SC to see changes." -ForegroundColor Green
}

Write-Host "Done!" -ForegroundColor Green
