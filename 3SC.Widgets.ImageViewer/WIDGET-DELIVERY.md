# 📦 Widget Delivery Guide

This document covers everything you need to know about preparing a widget for distribution, including **preview images** (static and animated).

---

## 📋 Table of Contents

1. [Required Assets](#-required-assets)
2. [Static Preview Images](#-static-preview-images)
3. [Animated Preview (GIF)](#-animated-preview-gif)
4. [Screenshots](#-screenshots)
5. [Tools & Software](#-tools--software)
6. [Step-by-Step Tutorial](#-step-by-step-tutorial)
7. [Manifest Configuration](#-manifest-configuration)
8. [Packaging Checklist](#-packaging-checklist)

---

## 🎯 Required Assets

| Asset | Dimensions | Format | Purpose | Required |
|-------|-----------|--------|---------|----------|
| `icon.png` | 128×128 px | PNG | Widget picker icon | ✅ Yes |
| `preview.png` | 800×600 px | PNG | Static store preview | ✅ Yes |
| `preview.gif` | 800×600 px | GIF | Animated preview | 🟡 Recommended |
| `screenshots/*.png` | 1280×720 px | PNG | Feature showcase | 🟡 Recommended |

### Asset Location
All assets should be placed in an `Assets/` folder:

```
3SC.Widgets.Clock/
└── Assets/
    ├── icon.png           # Widget icon
    ├── preview.png        # Static preview
    ├── preview.gif        # Animated preview
    └── screenshots/
        ├── main.png       # Main view
        ├── settings.png   # Settings dialog
        └── timezones.png  # Feature showcase
```

---

## 🖼️ Static Preview Images

Static previews are used in the Workshop Portal and widget picker when animated previews can't be displayed.

### Specifications
- **Dimensions**: 800×600 pixels (landscape)
- **Format**: PNG with transparency if needed
- **File size**: Under 500KB recommended
- **Background**: Transparent or match 3SC dark theme (#0A0A0F)

### How to Create a Static Preview

#### Method 1: Windows Snipping Tool
1. Run your widget in Debug mode
2. Press `Win + Shift + S` or open Snipping Tool
3. Select "Window" capture mode
4. Click on your widget window
5. Save as PNG

#### Method 2: Using PowerShell
```powershell
# Take a screenshot of a specific window
Add-Type -AssemblyName System.Windows.Forms

# Find widget window (adjust process name)
$widget = Get-Process | Where-Object { $_.MainWindowTitle -like "*Clock*" } | Select-Object -First 1

# Capture and save
[System.Windows.Forms.Screen]::PrimaryScreen | 
    ForEach-Object {
        $bitmap = New-Object System.Drawing.Bitmap($_.Bounds.Width, $_.Bounds.Height)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.CopyFromScreen($_.Bounds.Location, [System.Drawing.Point]::Empty, $_.Bounds.Size)
        $bitmap.Save("preview.png", [System.Drawing.Imaging.ImageFormat]::Png)
    }
```

#### Method 3: Using ShareX (Recommended)
1. Download [ShareX](https://getsharex.com/) (free, open-source)
2. Run your widget
3. Press `Ctrl + Shift + PrintScreen` (or your configured hotkey)
4. Select "Window" → Click your widget
5. Edit if needed → Save as PNG

#### Method 4: Visual Studio Screenshot
1. Run widget in Debug mode
2. Right-click the widget window in VS Designer
3. Select "Take Screenshot" (if available with extensions)

### Best Practices for Static Previews
- ✅ Show the widget in its default/best state
- ✅ Use the actual widget with real data
- ✅ Center the widget in frame if adding background
- ✅ Ensure text is readable at preview size
- ❌ Don't use placeholder or "Lorem ipsum" content
- ❌ Don't show error states or loading screens

---

## 🎬 Animated Preview (GIF)

Animated GIFs show your widget in action and are highly recommended for widgets with dynamic content like the Clock.

### Specifications
- **Dimensions**: 800×600 pixels (same as static)
- **Format**: GIF
- **Duration**: 3-10 seconds (loop)
- **Frame rate**: 10-15 FPS (balance quality vs size)
- **File size**: Under 2MB recommended
- **Colors**: 256 colors max (GIF limitation)

### How to Create an Animated Preview

#### Method 1: ScreenToGif (Recommended - Free)

[ScreenToGif](https://www.screentogif.com/) is a free, open-source screen recorder that exports directly to GIF.

1. **Download & Install**
   ```powershell
   # Using winget
   winget install ScreenToGif
   
   # Or download from https://www.screentogif.com/
   ```

2. **Record Your Widget**
   - Launch ScreenToGif
   - Click "Recorder"
   - Position the recording frame over your widget
   - Set dimensions to 800×600 (or scale later)
   - Click "Record" (F7)
   - Wait 5-10 seconds to capture the animation
   - Click "Stop" (F8)

3. **Edit the Recording**
   - Remove unwanted frames (start/end)
   - Adjust playback speed if needed
   - Add effects (optional):
     - Progress indicator
     - Border/padding
     - Fade in/out

4. **Export as GIF**
   - Click "Save as" → Select GIF
   - Choose encoder: "FFmpeg" for best quality
   - Set quality/colors: 256 colors
   - Enable looping
   - Save as `preview.gif`

#### Method 2: ShareX (Free)

1. **Configure ShareX for GIF**
   - Open ShareX → Task settings → Screen recorder
   - Set output: GIF
   - Set FPS: 15
   - Set duration or use hotkey to stop

2. **Record**
   - Press configured hotkey (default: `Ctrl + Shift + PrintScreen`)
   - Select region around your widget
   - Wait 5-10 seconds
   - Stop recording

3. **Edit & Save**
   - ShareX will open the editor
   - Trim, resize, add effects
   - Save as GIF

#### Method 3: FFmpeg (Command Line)

For advanced users, FFmpeg provides maximum control:

```powershell
# Record screen region to video first
ffmpeg -f gdigrab -framerate 15 -offset_x 100 -offset_y 100 -video_size 800x600 -i desktop -t 10 recording.mp4

# Convert video to GIF with optimized palette
ffmpeg -i recording.mp4 -vf "fps=15,scale=800:600:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" -loop 0 preview.gif
```

#### Method 4: Using PowerShell + .NET

```powershell
# Requires: Install-Package GifMotion or similar library
# This is a conceptual example

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$frames = @()
$duration = 5  # seconds
$fps = 10

for ($i = 0; $i -lt ($duration * $fps); $i++) {
    $bitmap = New-Object System.Drawing.Bitmap(800, 600)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen(100, 100, 0, 0, [System.Drawing.Size]::new(800, 600))
    $frames += $bitmap
    Start-Sleep -Milliseconds (1000 / $fps)
}

# Save frames as GIF (requires GIF encoder library)
```

### GIF Optimization Tips

1. **Reduce Colors**: Limit to 64-128 colors if possible
2. **Optimize Frames**: Remove duplicate frames
3. **Crop Tightly**: Don't include unnecessary background
4. **Use FFmpeg**: Best compression with palette optimization
5. **Check File Size**: Keep under 2MB for fast loading

```powershell
# Optimize existing GIF with gifsicle
gifsicle -O3 --colors 128 preview.gif -o preview-optimized.gif
```

---

## 📸 Screenshots

Screenshots showcase different states and features of your widget.

### Specifications
- **Dimensions**: 1280×720 px (720p)
- **Format**: PNG
- **Quantity**: 3-5 recommended

### Recommended Screenshots

| Screenshot | Content | Filename |
|------------|---------|----------|
| Main View | Widget in normal operation | `main.png` |
| Settings | Settings dialog open | `settings.png` |
| Feature 1 | Unique feature showcase | `timezones.png` |
| Feature 2 | Another feature/state | `formats.png` |
| Context Menu | Right-click menu open | `context-menu.png` |

### How to Create Screenshots

1. Run widget in its various states
2. Use any method from [Static Preview](#how-to-create-a-static-preview)
3. Resize to 1280×720 if needed
4. Save in `Assets/screenshots/` folder

---

## 🛠️ Tools & Software

### Free Tools (Recommended)

| Tool | Purpose | Link |
|------|---------|------|
| **ScreenToGif** | Record & create GIFs | [screentogif.com](https://www.screentogif.com/) |
| **ShareX** | Screenshots & recording | [getsharex.com](https://getsharex.com/) |
| **GIMP** | Image editing | [gimp.org](https://www.gimp.org/) |
| **FFmpeg** | Video/GIF conversion | [ffmpeg.org](https://ffmpeg.org/) |
| **Gifsicle** | GIF optimization | [lcdf.org/gifsicle](https://www.lcdf.org/gifsicle/) |

### Install with winget
```powershell
winget install ScreenToGif
winget install ShareX.ShareX
winget install GIMP.GIMP
winget install Gyan.FFmpeg
```

### Paid Alternatives
- **Snagit** - Professional screenshots
- **Camtasia** - Professional screen recording
- **Adobe Photoshop** - Image editing
- **CleanShot X** (macOS) - Screenshots & GIFs

---

## 📖 Step-by-Step Tutorial

### Creating Preview Assets for Clock Widget

#### Step 1: Prepare Your Widget
```powershell
# Build and run in Debug mode
cd C:\Users\ALPHA\source\repos\widgets\3SC.Widgets.Clock
dotnet run -c Debug
```

#### Step 2: Set Up Recording Area
1. Position the Clock widget where you want it
2. Ensure it's in its best visual state:
   - Correct timezone selected
   - Your preferred time format
   - All elements visible

#### Step 3: Create Static Preview
1. Open ShareX or Snipping Tool
2. Capture the widget window (800×600)
3. Save as `Assets/preview.png`

#### Step 4: Create Animated Preview
1. Open ScreenToGif
2. Position 800×600 frame over widget
3. Record for 5-10 seconds (showing time changing)
4. Edit: Remove first/last frames if needed
5. Export as `Assets/preview.gif` (256 colors, loop)

#### Step 5: Create Screenshots
1. Take screenshot of main widget → `screenshots/main.png`
2. Open settings dialog → capture → `screenshots/settings.png`
3. Show timezone dropdown → capture → `screenshots/timezones.png`

#### Step 6: Create Icon
1. Take a small screenshot of the widget
2. Crop to square (centered on clock face)
3. Resize to 128×128
4. Save as `Assets/icon.png`

---

## ⚙️ Manifest Configuration

Update your `manifest.json` to reference all preview assets:

```json
{
  "packageId": "com.3sc.clock",
  "widgetKey": "clock",
  "displayName": "Clock",
  "version": "1.2.0",
  "icon": "Assets/icon.png",
  "preview": {
    "static": "Assets/preview.png",
    "animated": "Assets/preview.gif",
    "screenshots": [
      "Assets/screenshots/main.png",
      "Assets/screenshots/settings.png",
      "Assets/screenshots/timezones.png"
    ]
  }
}
```

### Update .csproj to Include Assets

Add to your `.csproj` file to ensure assets are copied to output:

```xml
<ItemGroup>
  <None Update="Assets\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

## ✅ Packaging Checklist

Before submitting your widget, verify:

### Required Files
- [ ] `manifest.json` - Valid JSON with all required fields
- [ ] `{WidgetName}.dll` - Main widget assembly (Release build)
- [ ] `Assets/icon.png` - 128×128 widget icon

### Recommended Files
- [ ] `Assets/preview.png` - 800×600 static preview
- [ ] `Assets/preview.gif` - 800×600 animated preview
- [ ] `Assets/screenshots/` - 3-5 feature screenshots
- [ ] `README.md` - Widget documentation
- [ ] `CHANGELOG.md` - Version history
- [ ] `LICENSE` - Open source license

### Quality Checks
- [ ] Widget builds successfully in Release mode
- [ ] Widget loads and functions in 3SC host app
- [ ] All preview images are the correct dimensions
- [ ] GIF file size is under 2MB
- [ ] No `3SC.Widgets.Contracts.dll` in package (provided by host)
- [ ] Version in manifest matches assembly version

### Build & Package
```powershell
# Build in Release mode
dotnet build -c Release

# Run packaging script
.\Build-And-Package-Clock.ps1 -Configuration Release

# Verify package contents
Expand-Archive .\packages\clock-widget.3scwidget -DestinationPath .\packages\verify
Get-ChildItem .\packages\verify -Recurse
```

---

## 🎥 Video Preview (Future)

> 📌 **Note**: Video previews (MP4/WebM) may be supported in future versions of the Workshop Portal.

If you want to prepare for video support:

```powershell
# Record with OBS or FFmpeg
ffmpeg -f gdigrab -framerate 30 -offset_x 100 -offset_y 100 -video_size 800x600 -i desktop -t 10 -c:v libx264 -preset fast preview.mp4

# Or convert GIF to video
ffmpeg -i preview.gif -movflags faststart -pix_fmt yuv420p -vf "scale=trunc(iw/2)*2:trunc(ih/2)*2" preview.mp4
```

---

## 🆘 Troubleshooting

### GIF is too large
```powershell
# Reduce colors and optimize
gifsicle -O3 --colors 64 --lossy=80 preview.gif -o preview-small.gif
```

### GIF quality is poor
- Use higher frame rate during recording (20+ FPS)
- Use FFmpeg with palette optimization
- Consider fewer colors if file size allows

### Screenshots look pixelated
- Capture at native resolution, don't scale up
- Use PNG format (not JPEG)
- Ensure DPI scaling is consistent

### Assets not included in package
- Check `.csproj` has `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`
- Verify assets exist in the correct paths
- Check packaging script includes `Assets/**`

---

## 📚 Additional Resources

- [3SC Widget Development Guide](../docs/WIDGET-DEVELOPMENT.md)
- [Workshop Portal Documentation](https://workshop.3sc.app/docs)
- [ScreenToGif Documentation](https://www.screentogif.com/docs)
- [FFmpeg Documentation](https://ffmpeg.org/documentation.html)
