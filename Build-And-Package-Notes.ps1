# Build and package Notes widget
Write-Host "Building Notes Widget..." -ForegroundColor Cyan

# Clean first
Remove-Item "3SC.Widgets.Notes\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "3SC.Widgets.Notes\bin" -Recurse -Force -ErrorAction SilentlyContinue

# Build in Release mode using publish (to include all dependencies)
dotnet publish 3SC.Widgets.Notes/3SC.Widgets.Notes.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create widget package directory
$widgetDir = "$env:APPDATA\3SC\Widgets\Community\notes"
if (Test-Path $widgetDir) {
    Remove-Item $widgetDir -Recurse -Force
}
New-Item -ItemType Directory -Path $widgetDir -Force | Out-Null

Write-Host "Creating widget package..." -ForegroundColor Cyan

# Copy all files from publish folder
$publishDir = "3SC.Widgets.Notes\bin\Release\net8.0-windows\publish"
Get-ChildItem $publishDir -File | ForEach-Object {
    Copy-Item $_.FullName -Destination $widgetDir
    Write-Host "  Copied: $($_.Name)" -ForegroundColor Gray
}

Write-Host "`nNotes Widget packaged successfully!" -ForegroundColor Green
Write-Host "Location: $widgetDir" -ForegroundColor Yellow
