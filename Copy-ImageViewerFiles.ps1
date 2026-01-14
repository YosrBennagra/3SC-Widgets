# Script to quickly migrate Image Viewer widget files
$ErrorActionPreference = "Stop"

$source3SC = "C:\Users\ALPHA\source\repos\3SC\3SC"
$widgetDest = "C:\Users\ALPHA\source\repos\widgets\3SC.Widgets.ImageViewer"

Write-Host "Copying Image Viewer widget files and adapting..." -ForegroundColor Cyan

# Read the full ImageWidget View XAML
$viewXaml = Get-Content "$source3SC\Widgets\Views\ImageWidgetView.xaml" -Raw

# Read the ImageWidget Window XAML
$windowXaml = Get-Content "$source3SC\Widgets\Windows\Media\ImageWidget.xaml" -Raw

# Read the ImageWidget code-behind
$windowCs = Get-Content "$source3SC\Widgets\Windows\Media\ImageWidget.xaml.cs" -Raw

Write-Host "Files read successfully" -ForegroundColor Green
Write-Host "Source View XAML length: $($viewXaml.Length) chars" -ForegroundColor Gray
Write-Host "Source Window XAML length: $($windowXaml.Length) chars" -ForegroundColor Gray  
Write-Host "Source CS length: $($windowCs.Length) chars" -ForegroundColor Gray
