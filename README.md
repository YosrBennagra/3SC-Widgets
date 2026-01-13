# 3SC External Widgets Repository

This is the **external widgets workspace** for creating standalone widgets that can be dynamically loaded by the 3SC application.

## 📂 Structure

```
widgets/
├── widgets.sln                  # Solution file for all widgets
├── 3SC.Widgets.Clock/           # Example widget (template)
│   ├── ClockWidgetPlugin.cs
│   ├── ClockWidgetWindow.xaml
│   ├── manifest.json
│   └── README.md
└── [Your new widgets here]
```

## 🚀 Quick Start

### Open in Visual Studio
```powershell
cd C:\Users\ALPHA\source\repos\widgets
start widgets.sln
```

### Test a Widget
1. Set the widget project as startup project
2. Make sure it's in **Debug** configuration
3. Press **F5** to run standalone

### Build for Deployment
1. Switch to **Release** configuration
2. Build the project
3. Use the Build-ExternalWidget.ps1 script from the main 3SC repo

## 📋 Creating a New Widget

### Step 1: Copy the Template
```powershell
cd C:\Users\ALPHA\source\repos\widgets
cp -r 3SC.Widgets.Clock 3SC.Widgets.YourWidget
```

### Step 2: Update Project Files
1. Rename `3SC.Widgets.YourWidget.csproj`
2. Update namespaces in all `.cs` files
3. Update `manifest.json` with your widget info

### Step 3: Add to Solution
```powershell
dotnet sln widgets.sln add 3SC.Widgets.YourWidget/3SC.Widgets.YourWidget.csproj
```

### Step 4: Implement Your Widget
- Edit `YourWidgetPlugin.cs` - Entry point
- Edit `YourWidgetWindow.xaml` - UI design
- Edit `YourWidgetWindow.xaml.cs` - Widget logic
- Update `manifest.json` - Widget metadata

### Step 5: Test
```powershell
cd 3SC.Widgets.YourWidget
dotnet run --configuration Debug
```

## 🔧 Required Dependencies

All widgets reference:
- **3SC.Widgets.Contracts** (from main 3SC repo)
- **3SC.Domain** (from main 3SC repo)
- **CommunityToolkit.Mvvm** (NuGet package)

These paths are relative: `..\..\3SC\3SC.Widgets.Contracts\`

## 📦 Packaging and Deployment

From the main 3SC repo folder:
```powershell
cd C:\Users\ALPHA\source\repos\3SC
.\Build-ExternalWidget.ps1 -WidgetName YourWidget -Install
```

This will:
1. Build your widget in Release mode
2. Create a `.3scwidget` package
3. Copy to the Community widgets folder

## 📝 Widget Requirements Checklist

✅ Implement `IExternalWidget` interface
✅ Have a `manifest.json` file with correct entry point
✅ Reference `3SC.Widgets.Contracts` and `3SC.Domain`
✅ Target `net8.0-windows` framework
✅ Return a `Window` from `CreateWidgetWindow()`
✅ Clean up resources in `OnClosing()`
✅ Handle null `settingsJson` gracefully

## 🎨 Available Widgets

- **3SC.Widgets.Clock** - Clock with timezone support (template/example)

## 💡 Tips

- Each widget is a separate project
- Use Debug mode for testing (builds as .exe)
- Use Release mode for deployment (builds as .dll)
- Test standalone before packaging
- Study the Clock widget as reference

## 🔗 Related Files

- Main app: `C:\Users\ALPHA\source\repos\3SC\`
- Build script: `C:\Users\ALPHA\source\repos\3SC\Build-ExternalWidget.ps1`
- Community folder: `%APPDATA%\3SC\Widgets\Community\`

---

**Start with the Clock widget** - it's a complete working example that serves as your template!
