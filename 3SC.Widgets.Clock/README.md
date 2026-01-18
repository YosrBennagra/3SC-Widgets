# � Clock Widget

A beautiful digital clock widget for 3SC with timezone support, 12/24-hour formats, and customizable display options.

## ✨ Features

- **Multiple Timezones** - Display time from any timezone worldwide
- **12/24 Hour Format** - Switch between AM/PM and 24-hour display
- **Seconds Toggle** - Show or hide seconds for cleaner display
- **Timezone Label** - Optional timezone indicator pill
- **Smooth Animations** - Fluid time updates every 500ms
- **Dark Theme** - Beautiful 3SC-themed dark UI
- **Resizable** - Scales proportionally with font size adjustment
- **Desktop Widget Mode** - Stays on desktop, behind normal windows

## 🎯 Quick Start

### Run in Debug Mode (Standalone Testing)
```powershell
# Open in Visual Studio and press F5, or:
dotnet run --configuration Debug
```
This will show the Clock widget window directly for testing.

### Build as Plugin (For 3SC Deployment)
```powershell
dotnet build --configuration Release
```
This creates a DLL that can be dynamically loaded by the 3SC main app.

## 📁 Project Structure

```
3SC.Widgets.Clock/
├── manifest.json                # Widget metadata (REQUIRED)
├── ClockWidgetFactory.cs        # IWidgetFactory implementation
├── ClockWidget.cs               # IWidget implementation (UserControl mode)
├── ClockWidgetWindow.xaml       # Widget window UI (HasOwnWindow mode)
├── ClockWidgetWindow.xaml.cs    # Widget window logic
├── ClockWidgetView.xaml         # Clock display UserControl
├── ClockWidgetView.xaml.cs      # Clock display logic
├── ClockSettingsWindow.xaml     # Settings dialog
├── ClockSettingsWindow.xaml.cs  # Settings logic with live preview
├── WidgetWindowBase.cs          # Base class for widget windows
├── CommonTimeZones.cs           # Curated timezone list
├── ValueObjects/
│   └── ClockWidgetSettings.cs   # Settings data model
├── Helpers/
│   ├── ScreenBoundsHelper.cs    # Multi-monitor screen handling
│   ├── WidgetBehaviorHelper.cs  # Common widget behaviors
│   └── Win32Interop.cs          # Windows API interop
└── Assets/
    ├── icon.png                 # Widget icon (128x128)
    ├── preview.png              # Static preview (800x600)
    ├── preview.gif              # Animated preview
    └── screenshots/
        ├── main.png             # Main widget view
        ├── settings.png         # Settings dialog
        └── timezones.png        # Timezone selection
```

## 🔑 Widget Interface Implementation

### IWidgetFactory
The factory is discovered by 3SC via reflection and marked with `[Widget]` attribute:

```csharp
[Widget("clock", "Clock")]
public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget() => new ClockWindowWidget();
}
```

### IWidget Implementation
The widget implements all required interface members:

```csharp
[Widget("clock", "Clock")]
public class ClockWindowWidget : IWidget
{
    public string WidgetKey => "clock";
    public string DisplayName => "Clock";
    public string Version => "1.0.0";
    public bool HasSettings => true;
    public bool HasOwnWindow => true;   // Uses custom window
    
    public Window? CreateWindow() => new ClockWidgetWindow();
    public UserControl GetView() => throw new NotSupportedException();
    public void OnInitialize() { }
    public void OnDispose() { }
    public void ShowSettings() { }
}
```

## 📋 Configuration

### manifest.json
The manifest file contains widget metadata and must be in the output directory:

```json
{
  "packageId": "com.3sc.clock",
  "widgetKey": "clock",
  "displayName": "Clock",
  "version": "1.2.0",
  "hasSettings": true,
  "hasOwnWindow": true,
  "preview": {
    "static": "Assets/preview.png",
    "animated": "Assets/preview.gif"
  }
}
```

### ClockWidgetSettings
User-configurable settings:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TimeZoneId` | string | Local | Windows timezone ID |
| `Use24HourFormat` | bool | false | 24-hour vs 12-hour |
| `ShowSeconds` | bool | true | Display seconds |
| `ShowTimeZoneLabel` | bool | true | Show timezone pill |

## 🚀 Building & Packaging

### Build
```powershell
# Debug (standalone executable for testing)
dotnet build -c Debug

# Release (library DLL for 3SC)
dotnet build -c Release
```

### Package
```powershell
# From widgets root folder:
.\Build-And-Package-Clock.ps1 -Configuration Release
```

This creates:
- `packages/clock/` - Unpackaged files for local testing
- `packages/clock-widget.3scwidget` - Packaged ZIP for distribution

### Install for Testing
```powershell
# Copy to 3SC Community folder
$dest = "$env:APPDATA\3SC\Widgets\Community\clock"
Copy-Item packages/clock/* $dest -Recurse -Force
```

## 🎨 Design Guidelines

- **Dark Theme**: Uses 3SC color palette (#0A0A0F, #2DD4BF accent)
- **Rounded Corners**: CornerRadius="12" for container
- **Transparent Window**: AllowsTransparency="True"
- **Drop Shadow**: Depth effect for floating appearance
- **Scaling**: Font sizes scale proportionally on resize

## 📦 Required Assets for Distribution

See [WIDGET-DELIVERY.md](./WIDGET-DELIVERY.md) for complete guide on creating preview images.

| Asset | Dimensions | Format | Purpose |
|-------|-----------|--------|---------|
| icon.png | 128×128 | PNG | Widget picker icon |
| preview.png | 800×600 | PNG | Static store preview |
| preview.gif | 800×600 | GIF | Animated preview |
| screenshots/*.png | 1280×720 | PNG | Feature showcase |

## 📚 Dependencies

- `3SC.Widgets.Contracts` - IWidget interface (provided by host)
- `CommunityToolkit.Mvvm 8.2.2` - MVVM support
- `Serilog 3.1.1` - Logging (NOT 4.x for host compatibility)
- Handle null settings gracefully
- Use DispatcherTimer for UI updates
- Stop timers in OnClosing()
- Keep the widget lightweight

---

**This is your template!** Copy this entire folder structure to create new widgets.
