# Build and Package Moon Phase Widget
# This script builds the widget and packages it for distribution

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Building and Packaging Moon Phase Widget ===" -ForegroundColor Cyan
Write-Host ""

# Project paths
$ProjectPath = "3SC.Widgets.MoonPhase\3SC.Widgets.MoonPhase.csproj"
$OutputPath = "3SC.Widgets.MoonPhase\bin\$Configuration\net8.0-windows"
$PublishPath = "3SC.Widgets.MoonPhase\publish"
$PackagePath = "3SC.Widgets.MoonPhase\package"

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $PublishPath) {
    Remove-Item -Path $PublishPath -Recurse -Force
}
if (Test-Path $PackagePath) {
    Remove-Item -Path $PackagePath -Recurse -Force
}

# Build the project
Write-Host "Building project in $Configuration mode..." -ForegroundColor Yellow
dotnet build $ProjectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create publish directory
Write-Host "Creating publish directory..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $PublishPath | Out-Null
New-Item -ItemType Directory -Force -Path $PackagePath | Out-Null

# Copy widget files
Write-Host "Copying widget files..." -ForegroundColor Yellow
Copy-Item "$OutputPath\3SC.Widgets.MoonPhase.dll" -Destination $PublishPath
Copy-Item "$OutputPath\manifest.json" -Destination $PublishPath

# Copy dependencies (exclude system assemblies)
Write-Host "Copying dependencies..." -ForegroundColor Yellow
$Dependencies = @(
    "CommunityToolkit.Mvvm.dll",
    "Serilog.dll"
)

foreach ($dep in $Dependencies) {
    $sourcePath = Join-Path $OutputPath $dep
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath -Destination $PublishPath
        Write-Host "  Copied: $dep" -ForegroundColor Gray
    } else {
        Write-Host "  Warning: $dep not found" -ForegroundColor DarkYellow
    }
}

# Create package (ZIP)
Write-Host "Creating package..." -ForegroundColor Yellow
$PackageFile = Join-Path $PackagePath "MoonPhase.3scwidget"
Compress-Archive -Path "$PublishPath\*" -DestinationPath $PackageFile -Force

Write-Host ""
Write-Host "=== Package Complete ===" -ForegroundColor Green
Write-Host "Package location: $PackageFile" -ForegroundColor Green
Write-Host ""

# Display package contents
Write-Host "Package contents:" -ForegroundColor Cyan
Get-ChildItem $PublishPath | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "To test locally, copy files to:" -ForegroundColor Yellow
Write-Host "  %APPDATA%\3SC\Widgets\Community\moon-phase\" -ForegroundColor White
Write-Host ""
