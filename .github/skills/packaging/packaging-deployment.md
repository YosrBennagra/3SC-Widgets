# Packaging & Deployment

> **Category:** Packaging | **Priority:** 🔴 Critical
> **Version:** 3.0.0 | **Last Updated:** 2026-01-20

## Overview

This skill covers creating distributable widget packages (.3scwidget) and deploying them to the 3SC host application using the **centralized Build-Widget.ps1 and Install-Widget.ps1 scripts** located in the repository root.

## Prerequisites

- [build-configuration.md](build-configuration.md)
- [manifest-specification.md](../core/manifest-specification.md)

---

## ⚠️ IMPORTANT: Use Centralized Scripts Only

**DO NOT** create individual build/package scripts inside widget projects. Use the centralized scripts:
- `Build-Widget.ps1` - Builds and packages widgets
- `Install-Widget.ps1` - Installs, uninstalls, and lists widgets

These scripts are located in the **repository root** (`widgets/` folder).

---

## Package Format

The `.3scwidget` format is a ZIP archive containing:

```
my-widget.3scwidget
├── manifest.json           # Widget metadata (REQUIRED)
├── 3SC.Widgets.MyWidget.dll  # Widget assembly (REQUIRED)
├── 3SC.Widgets.MyWidget.pdb  # Debug symbols (optional)
├── Dependencies/           # Third-party dependencies
│   └── *.dll
└── Assets/                 # Static resources
    ├── icon.png           # Widget icon (128x128)
    ├── preview.png        # Preview image (optional)
    └── ...
```

---

## Installation Paths

```
%APPDATA%\3SC\Widgets\
├── Bundled\              # Pre-installed widgets (read-only)
│   └── clock\
└── Community\            # User-installed widgets
    └── my-widget\
        ├── manifest.json
        ├── 3SC.Widgets.MyWidget.dll
        └── Assets\
```

---

## Build-Widget.ps1 - Centralized Build Script

Located at: `widgets/Build-Widget.ps1`

### Features

- Builds and publishes widgets in Release mode
- Automatically copies manifest.json and Assets/ folder
- Removes unnecessary files (Contracts, .pdb files)
- Creates .3scwidget package in `packages/` folder
- Supports building single widget or all widgets
- Shows package contents and size

### Usage

```powershell
# Build single widget
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.Clock"

# Build all widgets
.\Build-Widget.ps1 -All

# View usage help
.\Build-Widget.ps1
```

### Output

Packages are created in `widgets/packages/` with naming convention:
- Format: `{widgetkey}-widget.3scwidget`
- Example: `clock-widget.3scwidget`

The widget key is automatically extracted from the project name by:
1. Removing `3SC.Widgets.` prefix
2. Converting to lowercase

Examples:
- `3SC.Widgets.Clock` → `clock-widget.3scwidget`
- `3SC.Widgets.ThisDayInHistory` → `thisdayinhistory-widget.3scwidget`

### What Gets Packaged

The script automatically includes:
- Widget DLL (e.g., `3SC.Widgets.Clock.dll`)
- Dependencies (CommunityToolkit.Mvvm, Serilog, etc.)
- `manifest.json` (copied from project root if not in publish folder)
- `Assets/` folder (if it exists in project root)
- `.deps.json` file

The script automatically excludes:
- `3SC.Widgets.Contracts.dll` (provided by host)
- `.pdb` files (debug symbols)

---

## Install-Widget.ps1 - Centralized Install Script

Located at: `widgets/Install-Widget.ps1`

### Features

- Installs widgets from `packages/` folder or custom path
- Lists all installed widgets with versions
- Uninstalls widgets
- Validates package structure and manifest
- Handles widget key extraction automatically

### Installation Directory

Widgets are installed to:
```
%APPDATA%\3SC\Widgets\Community\{widget-key}\
```

Example:
```
C:\Users\YourName\AppData\Roaming\3SC\Widgets\Community\
├── clock\
│   ├── manifest.json
│   ├── 3SC.Widgets.Clock.dll
│   └── Assets\
└── this-day-in-history\
    ├── manifest.json
    ├── 3SC.Widgets.ThisDayInHistory.dll
    └── ...
```

### Usage

```powershell
# Install widget from packages folder (use package name without -widget.3scwidget)
.\Install-Widget.ps1 -WidgetKey clock

# Install from custom path
.\Install-Widget.ps1 -PackagePath "C:\Downloads\my-widget.3scwidget"

# List all installed widgets
.\Install-Widget.ps1 -List

# Uninstall widget
.\Install-Widget.ps1 -Uninstall -WidgetKey clock

# View usage help
.\Install-Widget.ps1
```

### Install Process

1. Extracts package to temporary directory
2. Reads `manifest.json` to get widget key
3. Validates required fields (widgetKey, name, version)
4. Copies all files to `%APPDATA%\3SC\Widgets\Community\{widget-key}\`
5. Notifies to restart 3SC host application

---

## Complete Workflow Example

### 1. Build Your Widget

```powershell
# Navigate to repository root
cd c:\Users\ALPHA\source\repos\widgets

# Build the widget
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.Clock"
```

Output:
```
========================================
Building: 3SC.Widgets.Clock
========================================

[1/4] Publishing...
Published successfully
[2/4] Copying assets...
[3/4] Cleaning package...
[4/4] Creating package...

✅ Package created: clock-widget.3scwidget (45.2 KB)

Package contents:
  - 3SC.Widgets.Clock.dll
  - CommunityToolkit.Mvvm.dll
  - Serilog.dll
  - manifest.json
  - 3SC.Widgets.Clock.deps.json
```

### 2. Install for Testing

```powershell
# Install the widget
.\Install-Widget.ps1 -WidgetKey clock
```

Output:
```
Extracting package...

✅ Widget installed successfully!
   Name: Clock Widget
   Version: 1.2.0
   Key: clock
   Path: C:\Users\ALPHA\AppData\Roaming\3SC\Widgets\Community\clock

Restart 3SC to use the widget.
```

### 3. Test in 3SC Host

1. Close 3SC host application if running
2. Start 3SC host application
3. Widget should appear in widget list
4. Add widget to desktop

### 4. Make Changes & Update

```powershell
# Make code changes...

# Rebuild
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.Clock"

# Reinstall (overwrites existing)
.\Install-Widget.ps1 -WidgetKey clock

# Restart 3SC
```

### 5. Uninstall When Done

```powershell
.\Install-Widget.ps1 -Uninstall -WidgetKey clock
```

---

## Building All Widgets

For bulk operations:

```powershell
# Build all widgets at once
.\Build-Widget.ps1 -All
```

Output:
```
Building all widgets...
Found 20 widgets

========================================
Building: 3SC.Widgets.Clock
========================================
...
✅ Package created: clock-widget.3scwidget (45.2 KB)

========================================
Building: 3SC.Widgets.Notes
========================================
...
✅ Package created: notes-widget.3scwidget (38.7 KB)

...

========================================
Build Summary
========================================
Successful: 18
Failed: 2

Packages saved to: c:\Users\ALPHA\source\repos\widgets\packages
```

---

## Package Validation

The Install-Widget.ps1 script automatically validates:
- Package is a valid ZIP archive
- Contains `manifest.json`
- Manifest has required fields: `widgetKey`, `name`, `version`
- Widget key format is valid

For additional validation, check the package manually:

```powershell
# Extract and inspect
Expand-Archive "packages\clock-widget.3scwidget" -DestinationPath "temp-inspect"
code "temp-inspect\manifest.json"
```

---

---

## Troubleshooting

### Widget Not Found After Install

**Problem:** Widget doesn't appear in 3SC after installation.

**Solutions:**
1. Verify installation path: `%APPDATA%\3SC\Widgets\Community\{widget-key}\`
2. Check `manifest.json` is present in the folder
3. Ensure 3SC was fully restarted (not just minimized)
4. Check 3SC logs for loading errors

### Package Build Fails

**Problem:** `Build-Widget.ps1` reports build failure.

**Solutions:**
1. Ensure widget builds successfully first: `dotnet build -c Release`
2. Check `manifest.json` exists in project root
3. Verify project name follows `3SC.Widgets.*` pattern
4. Check for compilation errors in project

### Wrong Widget Key

**Problem:** Widget installs with different key than expected.

**Solution:**
- Widget key is determined by `manifest.json` `widgetKey` field
- Package filename is based on project name (lowercase, no prefix)
- When installing, the `widgetKey` from manifest is used for the install folder

### Cannot Uninstall Widget

**Problem:** `Install-Widget.ps1 -Uninstall` says widget not found.

**Solutions:**
1. List installed widgets: `.\Install-Widget.ps1 -List`
2. Use exact widget key from the list
3. Manually delete from: `%APPDATA%\3SC\Widgets\Community\{widget-key}\`

### Package Too Large

**Problem:** Widget package is larger than expected.

**Solutions:**
1. Check if unnecessary dependencies are included
2. Verify Assets folder doesn't contain large files
3. Ensure `.pdb` files are excluded (Build-Widget.ps1 does this automatically)
4. Review package contents: `Expand-Archive` and inspect

---

## Best Practices

### 1. Always Use Centralized Scripts

❌ **DON'T** create individual `Build-And-Package-*.ps1` scripts in widget projects.

✅ **DO** use the repository root scripts:
```powershell
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.YourWidget"
```

### 2. Test Before Distribution

```powershell
# Build
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.YourWidget"

# Install locally
.\Install-Widget.ps1 -WidgetKey yourwidget

# Test in 3SC host app

# If good, package is ready in packages/ folder
```

### 3. Version Management

- Update version in `manifest.json` before building
- Package filename doesn't include version (e.g., `clock-widget.3scwidget`)
- Version is stored inside manifest within the package
- Users can check version: `.\Install-Widget.ps1 -List`

### 4. Asset Organization

Keep Assets/ folder in project root with:
- `icon.png` (128x128, transparent PNG)
- `preview.png` (optional, for marketplace)
- Other resources used by widget

Build-Widget.ps1 automatically copies entire Assets/ folder.

### 5. Clean Builds

For release builds, clean first:
```powershell
dotnet clean
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.YourWidget"
```

### 6. Package Storage

- Packages are created in `widgets/packages/` folder
- This folder is in `.gitignore` (don't commit packages to repo)
- Distribute packages via:
  - 3SC Workshop Portal (recommended)
  - GitHub Releases
  - Direct download links

---

## Integration with CI/CD

### GitHub Actions Example

```yaml
name: Build Widgets

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Build All Widgets
      run: .\Build-Widget.ps1 -All
      shell: pwsh
    
    - name: Upload Packages
      uses: actions/upload-artifact@v3
      with:
        name: widget-packages
        path: packages/*.3scwidget
```

---

## Related Skills

- [build-configuration.md](build-configuration.md) - Build setup
- [versioning.md](versioning.md) - Version management
- [distribution.md](distribution.md) - Publishing widgets

---

## Quick Reference Card

```
BUILD WIDGET
  .\Build-Widget.ps1 -WidgetName "3SC.Widgets.YourWidget"
  → Creates: packages/yourwidget-widget.3scwidget

INSTALL WIDGET
  .\Install-Widget.ps1 -WidgetKey yourwidget
  → Installs to: %APPDATA%\3SC\Widgets\Community\yourwidget\

LIST WIDGETS
  .\Install-Widget.ps1 -List

UNINSTALL
  .\Install-Widget.ps1 -Uninstall -WidgetKey yourwidget

BUILD ALL
  .\Build-Widget.ps1 -All
```

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 3.0.0 | 2026-01-20 | Migrated to centralized Build-Widget.ps1 and Install-Widget.ps1 |
| 2.0.0 | 2026-01-15 | Added validation, checksums |
| 1.0.0 | 2025-06-01 | Initial version |

