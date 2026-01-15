# Widget Test Host

A shared WPF application for testing all widgets in the solution individually.

## Features

- **Dynamic Widget Loading**: Select any widget from a dropdown menu
- **Supports Both Widget Types**:
  - Embedded widgets (displays UserControl in the main window)
  - Windowed widgets (opens in separate window)
- **Easy Testing**: Test widget initialization, UI, and behavior without running the full 3SC application

## Usage

### Run from Visual Studio
1. Set `WidgetTestHost` as the startup project
2. Press F5 to run
3. Select a widget from the dropdown

### Run from Command Line
```powershell
dotnet run --project WidgetTestHost/WidgetTestHost.csproj -c Debug
```

## Available Widgets

- Calendar
- Clock
- Clipboard History
- Folders
- Game Vault
- Image Viewer
- Notes
- PDF Viewer
- QuickLinks
- Video Viewer

## Adding New Widgets

To add a new widget to the test host:

1. Add a project reference:
   ```powershell
   dotnet add WidgetTestHost/WidgetTestHost.csproj reference 3SC.Widgets.YourWidget/3SC.Widgets.YourWidget.csproj
   ```

2. Register the widget factory in `MainWindow.xaml.cs`:
   ```csharp
   _widgetFactories["Your Widget"] = new _3SC.Widgets.YourWidget.YourWidgetFactory();
   ```

3. Rebuild and run

## Benefits

- Test widgets without the full application host
- Quickly iterate on widget development
- Debug widget-specific issues
- No need to fix errors in other widgets to test one widget
