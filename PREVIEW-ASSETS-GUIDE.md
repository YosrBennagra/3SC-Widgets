# Widget Preview Assets Guide

## Overview
All widgets now use **preview images/gifs** for display in the 3SC host app. The `icon` field has been removed from all manifests.

## What Changed

### ✅ Clock Widget
- Renamed `clockgif.gif` → `preview.gif`
- Updated manifest to remove icon field
- Rebuilt package with preview.gif included (400 KB)

### 📋 All Widgets Updated
All 11 widget manifests have been updated to:
- ✅ Remove `icon` field
- ✅ Keep `preview` section with both `static` and `animated` options
- ✅ Reference preview assets in `Assets/` folder

## Required Assets for Each Widget

Each widget should have an `Assets/` folder with:

```
3SC.Widgets.{WidgetName}/
└── Assets/
    ├── preview.png          # Static preview (800×600 px) - REQUIRED
    ├── preview.gif          # Animated preview (800×600 px) - OPTIONAL
    └── screenshots/         # Feature screenshots - OPTIONAL
        ├── main.png
        ├── settings.png
        └── feature.png
```

## Current Status

| Widget | preview.png | preview.gif | Assets Folder |
|--------|-------------|-------------|---------------|
| Clock | ⚠️ Needed | ✅ **Done** | ✅ Yes |
| AppLauncher | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| Calendar | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| ClipboardHistory | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| Folders | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| GameVault | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| ImageViewer | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| Notes | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| PdfViewer | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| QuickLinks | ⚠️ Needed | ⚠️ Needed | ✅ Yes |
| VideoViewer | ⚠️ Needed | ⚠️ Needed | ✅ Yes |

## How to Create Preview Assets

### Quick Method (Static PNG)
1. Run your widget in Debug mode
2. Press `Win + Shift + S` (Snipping Tool)
3. Capture the widget window
4. Save as `preview.png` in the widget's `Assets/` folder
5. Resize to 800×600 px if needed

### Recommended Method (Animated GIF)
1. Download **ScreenToGif** (already documented in WIDGET-DELIVERY.md)
   ```powershell
   winget install ScreenToGif
   ```
2. Run your widget
3. Open ScreenToGif → Recorder
4. Position 800×600 frame over widget
5. Record 5-10 seconds
6. Export as GIF with 256 colors, looping
7. Save as `preview.gif` in the widget's `Assets/` folder

## Next Steps

### For Each Widget (Priority Order):

1. **Clock** ✅ Done
   - preview.gif created and included
   - Need to create preview.png as fallback

2. **Remaining Widgets**
   - [ ] Create preview.png (required for all)
   - [ ] Create preview.gif (recommended for dynamic widgets)
   - [ ] Rebuild widget package
   - [ ] Test installation

### Widgets That Would Benefit Most from Animated GIFs:
- ✨ **Clock** - Shows time changing (DONE)
- ✨ **Calendar** - Shows date navigation
- ✨ **ClipboardHistory** - Shows items being added/removed
- ✨ **VideoViewer** - Shows video playback
- ✨ **Notes** - Shows typing/editing
- ✨ **GameVault** - Shows game selection

### Widgets Where Static PNG is Sufficient:
- **AppLauncher** - Static app icons
- **Folders** - Static folder list
- **ImageViewer** - Static image display
- **PdfViewer** - Static PDF view
- **QuickLinks** - Static links list

## Build & Package

After adding preview assets to a widget:

```powershell
# Build specific widget
cd c:\Users\ALPHA\source\repos\widgets
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.{WidgetName}"

# Build all widgets
.\Build-Widget.ps1 -All
```

## Installation & Testing

```powershell
# Install widget
.\Install-Widget.ps1 -WidgetKey {widget-key}

# List installed widgets
.\Install-Widget.ps1 -List

# Uninstall widget
.\Install-Widget.ps1 -Uninstall -WidgetKey {widget-key}
```

## Manifest Configuration

All widgets now use this structure:

```json
{
  "packageId": "com.3sc.{widgetkey}",
  "widgetKey": "{widget-key}",
  "displayName": "{Widget Name}",
  "category": "productivity",
  "entry": "3SC.Widgets.{WidgetName}.dll",
  "preview": {
    "static": "Assets/preview.png",
    "animated": "Assets/preview.gif",
    "screenshots": [
      "Assets/screenshots/main.png"
    ]
  }
}
```

**Note:** The `icon` field has been completely removed from all widgets.

## .csproj Configuration

All widgets already have this configuration to copy assets:

```xml
<ItemGroup>
  <None Update="Assets\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Host App Integration

The 3SC host app will:
1. Look for `preview.animated` (GIF) first
2. Fall back to `preview.static` (PNG) if no GIF
3. Display preview in widget picker/gallery
4. No icon field is used

## Additional Resources

See [WIDGET-DELIVERY.md](3SC.Widgets.Clock/WIDGET-DELIVERY.md) for:
- Detailed ScreenToGif tutorial
- FFmpeg optimization commands
- Screenshot best practices
- GIF compression tips
