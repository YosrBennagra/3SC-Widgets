# Desktop Pet Widget - Delivery Documentation

## Package Contents

When packaged for distribution, the widget contains:

| File | Purpose |
|------|---------|
| `3SC.Widgets.DesktopPet.dll` | Main widget assembly |
| `manifest.json` | Widget metadata and configuration |
| `Assets/preview.png` | Static preview image |
| `Assets/preview.gif` | Animated preview |

## Installation Methods

### 1. Via 3SC Workshop (Recommended)
- Upload the `.3scwidget` package to the Workshop Portal
- Wait for admin approval
- Users can then install directly from the 3SC app

### 2. Manual Installation (Testing)
```powershell
.\Build-And-Package-DesktopPet.ps1 -InstallLocal
```
Or manually copy to: `%APPDATA%\3SC\Widgets\Community\desktop-pet\`

## Build Instructions

### Debug Build (Testing)
```powershell
dotnet build -c Debug
dotnet run
```

### Release Build (Distribution)
```powershell
dotnet publish -c Release -o publish
```

### Package for Distribution
```powershell
.\Build-And-Package-DesktopPet.ps1
```

## Widget Metadata

- **Package ID**: `com.3sc.desktop-pet`
- **Widget Key**: `desktop-pet`
- **Display Name**: Desktop Pet
- **Version**: 1.0.0
- **Category**: Entertainment
- **Entry Point**: `3SC.Widgets.DesktopPet.dll`

## Dependencies

The following dependencies are bundled with the widget:
- CommunityToolkit.Mvvm 8.2.2
- Serilog 3.1.1
- Serilog.Sinks.File 5.0.0
- System.Drawing.Common 8.0.7

**Note**: `3SC.Widgets.Contracts.dll` is NOT included - it's provided by the host application.

## Settings Storage

Pet settings are saved to:
```
%APPDATA%\3SC\WidgetData\desktop-pet\settings.json
```

This includes:
- Pet name, stats (happiness, energy, hunger)
- Window position
- User preferences (colors, auto-walk, particles)
- Lifetime statistics
