# Build and package Video Viewer widget
Write-Host "Building Video Viewer Widget..." -ForegroundColor Cyan

# Clean first
Remove-Item "3SC.Widgets.VideoViewer\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "3SC.Widgets.VideoViewer\bin" -Recurse -Force -ErrorAction SilentlyContinue

# Build in Release mode using publish (to include all dependencies)
dotnet publish 3SC.Widgets.VideoViewer/3SC.Widgets.VideoViewer.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create widget package directory
$widgetDir = "$env:APPDATA\3SC\Widgets\Community\video-viewer"
if (Test-Path $widgetDir) {
    Remove-Item $widgetDir -Recurse -Force
}
New-Item -ItemType Directory -Path $widgetDir -Force | Out-Null

Write-Host "Creating widget package..." -ForegroundColor Cyan

# Copy all files from publish folder
$publishDir = "3SC.Widgets.VideoViewer\bin\Release\net8.0-windows\publish"
Get-ChildItem $publishDir -File | ForEach-Object {
    Copy-Item $_.FullName -Destination $widgetDir
    Write-Host "  Copied: $($_.Name)" -ForegroundColor Gray
}

Write-Host "`nVideo Viewer Widget packaged successfully!" -ForegroundColor Green
Write-Host "Location: $widgetDir" -ForegroundColor Yellow
