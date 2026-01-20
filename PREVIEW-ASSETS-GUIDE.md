# Widget Preview Assets Guide

## Overview
All widgets now use **video previews (MP4)** for display in the 3SC host app. The `icon` field has been removed from all manifests. **Video is preferred over GIF** for better performance and smaller file sizes.

## What Changed

### ✅ Clock Widget (Latest)
- Switched from GIF to **MP4 video** preview
- Updated manifest: `"animated": "Assets/preview.mp4"`
- Rebuilt package with video included (104 KB vs 400 KB GIF = 74% smaller!)

### 📋 Preview Format Priority
1. **MP4 Video** (recommended) - Hardware-accelerated, 10x less CPU
2. GIF (fallback) - Still supported for backward compatibility
3. PNG (static fallback) - For when animation isn't needed

## Required Assets for Each Widget

Each widget should have an `Assets/` folder with:

```
3SC.Widgets.{WidgetName}/
└── Assets/
    ├── preview.mp4          # Video preview (preferred) - 5-10 sec loop
    ├── preview.png          # Static fallback (800×600 px) - REQUIRED
    └── screenshots/         # Feature screenshots - OPTIONAL
        ├── main.png
        ├── settings.png
        └── feature.png
```

## Current Status

| Widget | preview.mp4 | preview.png | screenshots | Status |
|--------|-------------|-------------|-------------|--------|
| Clock | ✅ Done | ✅ Done | ✅ Done | **Complete** |
| AppLauncher | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| Calendar | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| ClipboardHistory | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| Folders | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| GameVault | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| ImageViewer | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| Notes | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| PdfViewer | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| QuickLinks | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |
| VideoViewer | ⚠️ Needed | ⚠️ Needed | ⚠️ Needed | Needs Update |

## Preview Behavior in Widget Library

| Location | What's Displayed | Behavior |
|----------|------------------|----------|
| **Grid Card** | `preview.png` (static) | Shows thumbnail, no video playback |
| **Details Panel** | `preview.mp4` (video) | Plays looping video when widget selected |
| **Screenshots Carousel** | `screenshots/*.png` | Shown in details panel below preview |

> **Note:** The grid uses static images for performance. Video only plays when a widget is selected in the details panel.

## How to Create Preview Assets

### Quick Method (Screen Recording to MP4)
1. Use **Xbox Game Bar** (built into Windows)
   - Press `Win + G` to open
   - Click the Record button (or `Win + Alt + R`)
2. Record 5-10 seconds of your widget in action
3. Trim in Photos app or any video editor
4. Rename to `preview.mp4` and place in `Assets/` folder

### Recommended Method (OBS or ScreenToGif)
1. **OBS Studio** (free, high quality):
   ```powershell
   winget install OBSProject.OBSStudio
   ```
2. Set up a Window Capture for your widget
3. Record at 1080p, 30fps, ~5-10 seconds
4. Export as MP4 (H.264)
5. Trim and optimize if needed

### Converting GIF to MP4 (If you have existing GIFs)
```powershell
# Using FFmpeg
ffmpeg -i preview.gif -movflags +faststart -pix_fmt yuv420p preview.mp4
```

## Next Steps

### For Each Widget (Priority Order):

1. **Clock** ✅ Done
   - preview.mp4 created and deployed
   - Need to create preview.png as fallback

2. **Remaining Widgets**
   - [ ] Record screen capture (5-10 sec)
   - [ ] Save as preview.mp4 in Assets folder
   - [ ] Create preview.png as fallback
   - [ ] Update manifest to reference preview.mp4
   - [ ] Rebuild widget package
   - [ ] Test installation

### Widgets That Benefit from Video Preview:
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
    "animated": "Assets/preview.mp4",
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
1. Look for `preview.animated` (MP4 video or GIF) first
2. Fall back to `preview.static` (PNG) if no video
3. Display preview using MediaElement (hardware-accelerated video playback)
4. Auto-loop video previews seamlessly
5. No icon field is used

## Video vs GIF Comparison

| Aspect | MP4 Video | GIF |
|--------|-----------|-----|
| File Size | ~100 KB | ~400+ KB |
| CPU Usage | ~1% (GPU accelerated) | ~10% (CPU decoded) |
| Quality | Full color, 30fps | 256 colors, choppy |
| Recommended | ✅ Yes | Legacy support only |

## Additional Resources

See [WIDGET-DELIVERY.md](3SC.Widgets.Clock/WIDGET-DELIVERY.md) for:
- Detailed screen recording tutorials
- FFmpeg optimization commands
- Screenshot best practices
- Video compression tips
