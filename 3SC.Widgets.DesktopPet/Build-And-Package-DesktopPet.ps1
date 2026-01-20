# Desktop Pet Widget - Build and Package Script
# This script builds and packages the widget for distribution

param(
    [switch]$Debug,
    [switch]$InstallLocal
)

$ErrorActionPreference = "Stop"
$WidgetName = "DesktopPet"
$WidgetKey = "desktop-pet"
$ProjectDir = $PSScriptRoot
$OutputDir = Join-Path $ProjectDir "publish"

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Desktop Pet Widget - Build & Package               ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Clean previous build
if (Test-Path $OutputDir) {
    Write-Host "🧹 Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item $OutputDir -Recurse -Force
}

# Determine configuration
$Configuration = if ($Debug) { "Debug" } else { "Release" }
Write-Host "📦 Building in $Configuration mode..." -ForegroundColor Green

# Build the project
Write-Host ""
Write-Host "🔨 Building 3SC.Widgets.$WidgetName..." -ForegroundColor Blue
dotnet build "$ProjectDir\3SC.Widgets.$WidgetName.csproj" -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Publish if Release mode
if (-not $Debug) {
    Write-Host ""
    Write-Host "📤 Publishing..." -ForegroundColor Blue
    dotnet publish "$ProjectDir\3SC.Widgets.$WidgetName.csproj" -c Release -o $OutputDir --no-build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Publish failed!" -ForegroundColor Red
        exit 1
    }
    
    # Remove unnecessary files
    Write-Host ""
    Write-Host "🗑️  Cleaning up unnecessary files..." -ForegroundColor Yellow
    $filesToRemove = @(
        "3SC.Widgets.Contracts.dll",
        "3SC.Widgets.Contracts.pdb",
        "*.pdb",
        "*.deps.json"
    )
    foreach ($pattern in $filesToRemove) {
        Get-ChildItem -Path $OutputDir -Filter $pattern -ErrorAction SilentlyContinue | Remove-Item -Force
    }
    
    # Create package (ZIP)
    $packageName = "3SC.Widgets.$WidgetName.3scwidget"
    $packagePath = Join-Path $ProjectDir $packageName
    
    if (Test-Path $packagePath) {
        Remove-Item $packagePath -Force
    }
    
    Write-Host ""
    Write-Host "📦 Creating package: $packageName" -ForegroundColor Magenta
    Compress-Archive -Path "$OutputDir\*" -DestinationPath $packagePath -Force
    
    Write-Host ""
    Write-Host "✅ Package created: $packagePath" -ForegroundColor Green
    
    # Show package contents
    Write-Host ""
    Write-Host "📋 Package contents:" -ForegroundColor Cyan
    Get-ChildItem $OutputDir | ForEach-Object { Write-Host "   - $($_.Name)" }
}

# Install locally for testing
if ($InstallLocal) {
    Write-Host ""
    Write-Host "📥 Installing locally for testing..." -ForegroundColor Blue
    
    $sourceDir = if ($Debug) { 
        Join-Path $ProjectDir "bin\Debug\net8.0-windows" 
    } else { 
        $OutputDir 
    }
    
    $destDir = Join-Path $env:APPDATA "3SC\Widgets\Community\$WidgetKey"
    
    if (Test-Path $destDir) {
        Remove-Item $destDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    
    # Copy required files
    Copy-Item "$sourceDir\3SC.Widgets.$WidgetName.dll" $destDir
    Copy-Item "$sourceDir\manifest.json" $destDir
    
    # Copy dependencies (excluding Contracts)
    Get-ChildItem $sourceDir -Filter "*.dll" | Where-Object {
        $_.Name -notlike "3SC.Widgets.Contracts*" -and
        $_.Name -ne "3SC.Widgets.$WidgetName.dll"
    } | ForEach-Object {
        Copy-Item $_.FullName $destDir
    }
    
    Write-Host "✅ Installed to: $destDir" -ForegroundColor Green
    Write-Host ""
    Write-Host "⚠️  Restart 3SC to load the widget!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Build complete! 🎉" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
