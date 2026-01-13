# Create 3SC Widget Package Script
# Generates a complete .3scwidget package with manifest and documentation

param(
    [Parameter(Mandatory=$false)]
    [string]$WidgetName = "my-widget",
    
    [Parameter(Mandatory=$false)]
    [string]$PackageId = "com.company.my-widget",
    
    [Parameter(Mandatory=$false)]
    [string]$DisplayName = "My Widget",
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "A custom widget for 3SC desktop application.",
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",
    
    [Parameter(Mandatory=$false)]
    [string]$AuthorName = "Widget Author",
    
    [Parameter(Mandatory=$false)]
    [string]$AuthorEmail = "author@example.com",
    
    [Parameter(Mandatory=$false)]
    [string]$AuthorUrl = "https://github.com/author",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("General", "Productivity", "System", "Media", "Utilities")]
    [string]$Category = "General",
    
    [Parameter(Mandatory=$false)]
    [string]$Icon = "widget",
    
    [Parameter(Mandatory=$false)]
    [int]$DefaultWidth = 400,
    
    [Parameter(Mandatory=$false)]
    [int]$DefaultHeight = 300,
    
    [Parameter(Mandatory=$false)]
    [string]$MinAppVersion = "1.0.0",
    
    [Parameter(Mandatory=$false)]
    [string[]]$Tags = @("widget", "custom"),
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "C:\temp"
)

$ErrorActionPreference = "Stop"

# ============================================================================
# VALIDATION FUNCTIONS
# ============================================================================

function Test-PackageId {
    param([string]$Id)
    
    if ([string]::IsNullOrWhiteSpace($Id)) {
        throw "PackageId cannot be empty"
    }
    
    if ($Id -ne $Id.ToLower()) {
        throw "PackageId must be lowercase"
    }
    
    if ($Id -notmatch '\.') {
        throw "PackageId must contain '.' (reverse domain format, e.g., com.company.widget)"
    }
    
    if ($Id -match '\s') {
        throw "PackageId cannot contain spaces"
    }
    
    return $true
}

function Test-WidgetKey {
    param([string]$Key)
    
    if ([string]::IsNullOrWhiteSpace($Key)) {
        throw "WidgetKey cannot be empty"
    }
    
    if ($Key -ne $Key.ToLower()) {
        throw "WidgetKey must be lowercase"
    }
    
    if ($Key -match '\s') {
        throw "WidgetKey cannot contain spaces"
    }
    
    return $true
}

function Test-SemanticVersion {
    param([string]$Ver)
    
    if ($Ver -notmatch '^\d+\.\d+\.\d+$') {
        throw "Version must follow semantic versioning (e.g., 1.0.0)"
    }
    
    return $true
}

# ============================================================================
# BANNER
# ============================================================================

Clear-Host
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   3SC Widget Package Creator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# VALIDATE INPUT
# ============================================================================

Write-Host "[1/8] Validating input parameters..." -ForegroundColor Yellow

try {
    Test-PackageId -Id $PackageId
    Test-WidgetKey -Key $WidgetName
    Test-SemanticVersion -Ver $Version
    
    if ([string]::IsNullOrWhiteSpace($DisplayName)) {
        throw "DisplayName is required"
    }
    
    if ([string]::IsNullOrWhiteSpace($Description)) {
        throw "Description is required"
    }
    
    if ([string]::IsNullOrWhiteSpace($AuthorName)) {
        throw "AuthorName is required"
    }
    
    Write-Host "[+] All validation checks passed" -ForegroundColor Green
}
catch {
    Write-Host "[X] Validation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ============================================================================
# DISPLAY PACKAGE INFO
# ============================================================================

Write-Host "Package Information:" -ForegroundColor Cyan
Write-Host "  Package ID:    $PackageId" -ForegroundColor White
Write-Host "  Widget Key:    $WidgetName" -ForegroundColor White
Write-Host "  Display Name:  $DisplayName" -ForegroundColor White
Write-Host "  Version:       $Version" -ForegroundColor White
Write-Host "  Category:      $Category" -ForegroundColor White
Write-Host "  Author:        $AuthorName ($AuthorEmail)" -ForegroundColor White
Write-Host ""

# ============================================================================
# CREATE TEMP FOLDER STRUCTURE
# ============================================================================

Write-Host "[2/8] Creating temporary folder structure..." -ForegroundColor Yellow

$TempFolder = Join-Path $OutputPath $WidgetName
$PackageFile = Join-Path $OutputPath "$WidgetName.3scwidget"

if (Test-Path $TempFolder) {
    Write-Host "  Removing existing temp folder..." -ForegroundColor Gray
    Remove-Item $TempFolder -Recurse -Force
}

New-Item -ItemType Directory -Path $TempFolder -Force | Out-Null
Write-Host "[+] Temp folder created: $TempFolder" -ForegroundColor Green
Write-Host ""

# ============================================================================
# GENERATE MANIFEST.JSON
# ============================================================================

Write-Host "[3/8] Generating manifest.json..." -ForegroundColor Yellow

$Manifest = @{
    packageId = $PackageId
    widgetKey = $WidgetName
    displayName = $DisplayName
    description = $Description
    version = $Version
    author = @{
        name = $AuthorName
        email = $AuthorEmail
        url = $AuthorUrl
    }
    category = $Category.ToLower()
    icon = $Icon
    defaultSize = @{
        width = $DefaultWidth
        height = $DefaultHeight
    }
    minAppVersion = $MinAppVersion
    tags = $Tags
    permissions = @()
    settings = @{
        hasSettings = $false
    }
}

$ManifestPath = Join-Path $TempFolder "manifest.json"
$ManifestJson = $Manifest | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText($ManifestPath, $ManifestJson, [System.Text.UTF8Encoding]::new($false))

Write-Host "[+] manifest.json created" -ForegroundColor Green
Write-Host ""

# ============================================================================
# GENERATE README.MD
# ============================================================================

Write-Host "[4/8] Generating README.md..." -ForegroundColor Yellow

$ReadmeContent = "# $DisplayName`n`n"
$ReadmeContent += "$Description`n`n"
$ReadmeContent += "## Features`n`n"
$ReadmeContent += "* Custom widget functionality`n"
$ReadmeContent += "* Easy to configure`n"
$ReadmeContent += "* Lightweight and efficient`n"
$ReadmeContent += "* Integrates seamlessly with 3SC`n`n"
$ReadmeContent += "## Installation`n`n"
$ReadmeContent += "1. Download the ${WidgetName}.3scwidget package`n"
$ReadmeContent += "2. Double-click the file to install (if 3SC supports automatic installation)`n"
$ReadmeContent += "3. Or manually extract to: %APPDATA%\3SC\Widgets\Community\${PackageId}\`n"
$ReadmeContent += "4. Restart 3SC application`n"
$ReadmeContent += "5. The widget will appear in the widget gallery`n`n"
$ReadmeContent += "## Usage`n`n"
$ReadmeContent += "1. Open 3SC application`n"
$ReadmeContent += "2. Navigate to the widget gallery`n"
$ReadmeContent += "3. Find '$DisplayName' in the $Category category`n"
$ReadmeContent += "4. Click to add the widget to your desktop`n"
$ReadmeContent += "5. Customize settings as needed`n`n"
$ReadmeContent += "## Configuration`n`n"
$ReadmeContent += "This widget currently has no configuration options. Settings may be added in future versions.`n`n"
$ReadmeContent += "## Requirements`n`n"
$ReadmeContent += "* 3SC Desktop Application v$MinAppVersion or higher`n"
$ReadmeContent += "* Windows 10/11`n`n"
$ReadmeContent += "## Support`n`n"
$ReadmeContent += "For issues, questions, or feature requests:`n"
$ReadmeContent += "* Email: $AuthorEmail`n"
$ReadmeContent += "* Website: $AuthorUrl`n`n"
$ReadmeContent += "## License`n`n"
$ReadmeContent += "Copyright $([DateTime]::Now.Year) $AuthorName. All rights reserved.`n`n"
$ReadmeContent += "## Version`n`n"
$ReadmeContent += "$Version`n"

$ReadmePath = Join-Path $TempFolder "README.md"
[System.IO.File]::WriteAllText($ReadmePath, $ReadmeContent, [System.Text.UTF8Encoding]::new($false))

Write-Host "[+] README.md created" -ForegroundColor Green
Write-Host ""

# ============================================================================
# GENERATE CHANGELOG.MD
# ============================================================================

Write-Host "[5/8] Generating CHANGELOG.md..." -ForegroundColor Yellow

$ChangelogContent = "# Changelog`n`n"
$ChangelogContent += "All notable changes to $DisplayName will be documented in this file.`n`n"
$ChangelogContent += "The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),`n"
$ChangelogContent += "and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).`n`n"
$ChangelogContent += "## [$Version] - $([DateTime]::Now.ToString('yyyy-MM-dd'))`n`n"
$ChangelogContent += "### Added`n"
$ChangelogContent += "* Initial release`n"
$ChangelogContent += "* Core widget functionality`n"
$ChangelogContent += "* Basic configuration options`n"
$ChangelogContent += "* Integration with 3SC desktop application`n`n"
$ChangelogContent += "### Changed`n"
$ChangelogContent += "* N/A (initial release)`n`n"
$ChangelogContent += "### Deprecated`n"
$ChangelogContent += "* N/A`n`n"
$ChangelogContent += "### Removed`n"
$ChangelogContent += "* N/A`n`n"
$ChangelogContent += "### Fixed`n"
$ChangelogContent += "* N/A`n`n"
$ChangelogContent += "### Security`n"
$ChangelogContent += "* N/A`n`n"
$ChangelogContent += "---`n`n"
$ChangelogContent += "## Version History`n`n"
$ChangelogContent += "### [$Version] - $([DateTime]::Now.ToString('yyyy-MM-dd'))`n"
$ChangelogContent += "First public release of $DisplayName.`n"

$ChangelogPath = Join-Path $TempFolder "CHANGELOG.md"
[System.IO.File]::WriteAllText($ChangelogPath, $ChangelogContent, [System.Text.UTF8Encoding]::new($false))

Write-Host "[+] CHANGELOG.md created" -ForegroundColor Green
Write-Host ""

# ============================================================================
# VALIDATE PACKAGE
# ============================================================================

Write-Host "[6/8] Validating package contents..." -ForegroundColor Yellow

$RequiredFiles = @("manifest.json")
$MissingFiles = @()

foreach ($file in $RequiredFiles) {
    $filePath = Join-Path $TempFolder $file
    if (-not (Test-Path $filePath)) {
        $MissingFiles += $file
    }
}

if ($MissingFiles.Count -gt 0) {
    Write-Host "[X] Missing required files: $($MissingFiles -join ', ')" -ForegroundColor Red
    exit 1
}

# Check package size
$TotalSize = (Get-ChildItem $TempFolder -Recurse | Measure-Object -Property Length -Sum).Sum
$MaxSize = 50MB

if ($TotalSize -gt $MaxSize) {
    $sizeMB = [math]::Round($TotalSize/1MB, 2)
    Write-Host "[X] Package size ($sizeMB MB) exceeds maximum (50 MB)" -ForegroundColor Red
    exit 1
}

Write-Host "[+] Package validation passed" -ForegroundColor Green
Write-Host "  Total size: $([math]::Round($TotalSize/1KB, 2)) KB" -ForegroundColor Gray
Write-Host ""

# ============================================================================
# CREATE .3SCWIDGET PACKAGE
# ============================================================================

Write-Host "[7/8] Creating .3scwidget package..." -ForegroundColor Yellow

$TempZipFile = Join-Path $OutputPath "$WidgetName.zip"

if (Test-Path $TempZipFile) {
    Write-Host "  Removing existing temp zip..." -ForegroundColor Gray
    Remove-Item $TempZipFile -Force
}

if (Test-Path $PackageFile) {
    Write-Host "  Removing existing package..." -ForegroundColor Gray
    Remove-Item $PackageFile -Force
}

try {
    # Create ZIP file first
    Compress-Archive -Path "$TempFolder\*" -DestinationPath $TempZipFile -CompressionLevel Optimal -Force
    
    # Rename .zip to .3scwidget
    Move-Item -Path $TempZipFile -Destination $PackageFile -Force
    
    Write-Host "[+] Package created successfully" -ForegroundColor Green
}
catch {
    Write-Host "[X] Failed to create package: $($_.Exception.Message)" -ForegroundColor Red
    
    # Cleanup temp zip if it exists
    if (Test-Path $TempZipFile) {
        Remove-Item $TempZipFile -Force
    }
    
    exit 1
}

Write-Host ""

# ============================================================================
# DISPLAY PACKAGE CONTENTS
# ============================================================================

Write-Host "Package Contents:" -ForegroundColor Cyan
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($PackageFile)
foreach ($entry in $zip.Entries) {
    $size = if ($entry.Length -lt 1KB) { "$($entry.Length) B" } else { "$([math]::Round($entry.Length/1KB, 2)) KB" }
    Write-Host "  * $($entry.Name)" -ForegroundColor White -NoNewline
    Write-Host " ($size)" -ForegroundColor Gray
}
$zip.Dispose()

Write-Host ""

# ============================================================================
# CLEANUP
# ============================================================================

Write-Host "[8/8] Cleaning up temporary files..." -ForegroundColor Yellow

Remove-Item $TempFolder -Recurse -Force
Write-Host "[+] Temp files removed" -ForegroundColor Green
Write-Host ""

# ============================================================================
# SUCCESS SUMMARY
# ============================================================================

$FinalSize = (Get-Item $PackageFile).Length

Write-Host "========================================" -ForegroundColor Green
Write-Host "   Package Created Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Package Location:" -ForegroundColor Cyan
Write-Host "  $PackageFile" -ForegroundColor White
Write-Host ""
Write-Host "Package Size:" -ForegroundColor Cyan
Write-Host "  $([math]::Round($FinalSize/1KB, 2)) KB" -ForegroundColor White
Write-Host ""
Write-Host "Installation Path:" -ForegroundColor Cyan
Write-Host "  %APPDATA%\3SC\Widgets\Community\$PackageId\" -ForegroundColor White
Write-Host ""

# ============================================================================
# NEXT STEPS
# ============================================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Test the Package:" -ForegroundColor Yellow
Write-Host "   - Extract and verify contents:" -ForegroundColor White
Write-Host "     Expand-Archive -Path '$PackageFile' -DestinationPath 'C:\temp\test-$WidgetName'" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Install Manually:" -ForegroundColor Yellow
    Write-Host "   - Copy to installation path:" -ForegroundColor White
Write-Host "     `$installPath = `"`$env:APPDATA\3SC\Widgets\Community\$PackageId`"" -ForegroundColor Gray
Write-Host "     New-Item -ItemType Directory -Path `$installPath -Force" -ForegroundColor Gray
Write-Host "     Expand-Archive -Path '$PackageFile' -DestinationPath `$installPath" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Test in 3SC:" -ForegroundColor Yellow
Write-Host "   - Restart 3SC application" -ForegroundColor White
   Write-Host "   - Open widget gallery" -ForegroundColor White
   Write-Host "   - Look for '$DisplayName' in $Category category" -ForegroundColor White
Write-Host ""
Write-Host "4. Add Custom Assets (Optional):" -ForegroundColor Yellow
Write-Host "   * icon.png (256x256 recommended)" -ForegroundColor White
Write-Host "   * Additional .dll files for functionality" -ForegroundColor White
Write-Host "   * Custom .html/.css for UI" -ForegroundColor White
Write-Host "   * Then re-run this script to repackage" -ForegroundColor White
Write-Host ""
Write-Host "5. Distribute:" -ForegroundColor Yellow
Write-Host "   * Share the .3scwidget file" -ForegroundColor White
Write-Host "   * Upload to a widget repository" -ForegroundColor White
Write-Host "   * Include README.md for documentation" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Happy widget development!" -ForegroundColor Green
Write-Host ""
