# 📦 Clock Widget - External Widget Template

This is a **standalone external widget** that demonstrates the complete structure for creating 3SC widgets.

## 🎯 Quick Start

### Run in Debug Mode (Standalone Testing)
```powershell
# Open in Visual Studio and press F5, or:
dotnet run --configuration Debug
```
This will show the Clock widget window directly for testing.

### Build as Plugin (For Deployment)
```powershell
dotnet build --configuration Release
```
This creates a DLL that can be dynamically loaded by the 3SC main app.

## 📁 Project Files

```
3SC.Widgets.Clock/
├── 3SC.Widgets.Clock.csproj    # Project configuration
├── ClockWidgetPlugin.cs         # Entry point - implements IExternalWidget
├── ClockWidgetWindow.xaml       # Widget UI
├── ClockWidgetWindow.xaml.cs    # Widget logic
├── manifest.json                # Widget metadata (required!)
└── TestLauncher.cs             # Standalone test launcher
```

## 🔑 Key Concepts

### 1. Plugin Entry Point (`ClockWidgetPlugin.cs`)
Every widget must have a class that implements `IExternalWidget`:
```csharp
public class ClockWidgetPlugin : IExternalWidget
{
    public string WidgetKey => "clock-external";
    public string DisplayName => "Clock (External)";
    // ... other metadata
    
    public object CreateWidgetWindow(Guid widgetInstanceId, string? settingsJson)
    {
        return new ClockWidgetWindow(widgetInstanceId, settings);
    }
}
```

### 2. Widget Window
A standard WPF Window with your widget UI and logic.

### 3. Manifest File
JSON file with widget metadata - **must be copied to output directory**.

### 4. Dual-Mode Configuration
- **Debug**: Builds as executable (`WinExe`) for standalone testing
- **Release**: Builds as library (`Library`) for plugin loading

## 📋 Required References

Your widget needs:
1. `3SC.Widgets.Contracts` - IExternalWidget interface
2. `3SC.Domain` - Shared domain types (like ClockWidgetSettings)
3. WPF packages

## 🚀 Create Your Own Widget

1. **Copy this folder** to create a new widget
2. **Rename** the project and files
3. **Update** manifest.json with your widget info
4. **Implement** IExternalWidget in your plugin class
5. **Create** your widget window UI
6. **Test** in Debug mode (F5)
7. **Build** in Release mode
8. **Package** using Build-ExternalWidget.ps1

## 📦 Packaging

From the main 3SC repo:
```powershell
.\Build-ExternalWidget.ps1 -WidgetName Clock -Install
```

This will:
- Build the widget in Release mode
- Create a .3scwidget package
- Install to Community folder (with -Install flag)

## 🎨 Widget Design Guidelines

- Use rounded corners (`CornerRadius="12"`)
- Transparent window background
- Drop shadow for depth
- Light/dark theme support
- Clean up resources in `OnClosing()`

## 💡 Tips

- Always test in Debug mode first (standalone)
- Handle null settings gracefully
- Use DispatcherTimer for UI updates
- Stop timers in OnClosing()
- Keep the widget lightweight

---

**This is your template!** Copy this entire folder structure to create new widgets.
