param()

$project = Join-Path $PSScriptRoot "3SC.Widgets.Folders.csproj"
$publishDir = Join-Path $PSScriptRoot "publish"

Write-Host "Cleaning..."
dotnet clean $project -c Release

Write-Host "Publishing..."
dotnet publish $project -c Release -o $publishDir

$dest = Join-Path $env:APPDATA "3SC\Widgets\Community\folders"
Write-Host "Copying to $dest"
if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
New-Item -ItemType Directory -Path $dest -Force | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $dest -Recurse -Force

Write-Host "Done."
