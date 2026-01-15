# Build and Package Image Viewer Widget
param(
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

# Paths
$projectPath = "C:\Users\ALPHA\source\repos\widgets\3SC.Widgets.ImageViewer"
$buildOutput = Join-Path $projectPath "bin\Release\net8.0-windows\publish"
$widgetKey = "image-viewer"
$communityFolder = Join-Path $env:APPDATA "3SC\Widgets\Community\$widgetKey"

Write-Host "Building Image Viewer Widget..." -ForegroundColor Cyan

# Build and publish the project
if (-not $SkipBuild) {
    Push-Location $projectPath
    try {
        dotnet publish -c Release -o "bin\Release\net8.0-windows\publish"
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed"
        }
    }
    finally {
        Pop-Location
    }
}

Write-Host "Creating widget package..." -ForegroundColor Cyan

# Create community folder
if (-not (Test-Path $communityFolder)) {
    New-Item -ItemType Directory -Path $communityFolder -Force | Out-Null
}

# Copy all files from publish folder
if (Test-Path $buildOutput) {
    Get-ChildItem -Path $buildOutput -File | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $communityFolder $_.Name) -Force
        Write-Host "  Copied: $($_.Name)" -ForegroundColor Green
    }
}
else {
    Write-Host "  Error: Build output not found at $buildOutput" -ForegroundColor Red
    exit 1
}

Write-Host "`nImage Viewer Widget packaged successfully!" -ForegroundColor Green
Write-Host "Location: $communityFolder" -ForegroundColor Gray
