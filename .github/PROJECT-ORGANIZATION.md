# 📁 Project Organization Guide

> **How to Keep the Widgets Repository Clean, Scalable, and Maintainable**
> 
> **Last Updated:** 2026-01-20

---

## 🎯 Overview

This guide helps you maintain organization and scalability as you create **many widgets** in this repository. Follow these practices to keep the codebase clean, navigable, and professional.

---

## 📂 Current Repository Structure

```
widgets/
├── .github/                          # GitHub & Copilot configuration
│   ├── copilot-instructions.md       # Main Copilot behavior guide
│   ├── PROJECT-ORGANIZATION.md       # This file
│   └── skills/                       # Skill-based knowledge system
│       ├── SKILL-INDEX.md           # Master index
│       ├── core/                    # Core architecture
│       ├── ui/                      # UI/UX patterns
│       ├── data/                    # Data & settings
│       ├── packaging/               # Build & deployment
│       ├── templates/               # Code templates
│       └── ...
│
├── docs/                            # Documentation
│   ├── WIDGET-IDEAS-MEGA-LIST.md   # Widget ideas catalog
│   └── ...
│
├── packages/                        # Build output (.gitignored)
│   └── *.3scwidget
│
├── WidgetTestHost/                  # Test harness application
│   ├── MainWindow.xaml.cs          # Widget registration
│   └── WidgetTestHost.csproj       # Project references
│
├── 3SC.Widgets.*/                   # Individual widget projects
│   ├── manifest.json
│   ├── *WidgetFactory.cs
│   ├── *ViewModel.cs
│   ├── *Window.xaml
│   ├── Assets/
│   │   └── icon.png
│   ├── Models/
│   ├── Helpers/
│   └── ...
│
├── Build-Widget.ps1                 # Centralized build script
├── Install-Widget.ps1               # Centralized install script
└── widgets.sln                      # Solution file
```

---

## ✅ Best Practices for Organization

### 1. Widget Naming Convention

**Folder/Project Names:**
```
3SC.Widgets.{WidgetName}
```

**Examples:**
- ✅ `3SC.Widgets.Clock`
- ✅ `3SC.Widgets.ThisDayInHistory`
- ✅ `3SC.Widgets.PomodoroTimer`
- ❌ `ClockWidget` (missing prefix)
- ❌ `3SC.Widgets.clock` (not PascalCase)

**Widget Keys** (in manifest.json):
```json
{
  "widgetKey": "clock"          // lowercase, hyphens only
}
```

**Examples:**
- ✅ `clock`
- ✅ `this-day-in-history`
- ✅ `pomodoro-timer`
- ❌ `Clock` (not lowercase)
- ❌ `this_day_in_history` (use hyphens, not underscores)

---

### 2. Mandatory Files per Widget

Every widget **MUST** have:

```
3SC.Widgets.YourWidget/
├── manifest.json              ✅ REQUIRED - Widget metadata
├── *.csproj                   ✅ REQUIRED - Project file
├── *WidgetFactory.cs          ✅ REQUIRED - Factory implementation
├── WidgetWindowBase.cs        ✅ REQUIRED - Draggable window base
├── Assets/
│   └── icon.png              ✅ REQUIRED - 128×128 transparent PNG
├── README.md                  ✅ RECOMMENDED - Widget documentation
└── WIDGET-DELIVERY.md         ⚠️  OPTIONAL - Delivery notes
```

**Optional but Recommended:**
- `Models/` - Data models
- `Helpers/` - Utility classes
- `Converters/` - XAML converters
- `Data/` - Static data or databases

---

### 3. File Naming Patterns

Follow these patterns for consistency:

| File Type | Pattern | Example |
|-----------|---------|---------|
| Factory | `{WidgetName}WidgetFactory.cs` | `ClockWidgetFactory.cs` |
| ViewModel | `{WidgetName}ViewModel.cs` | `ClockWidgetViewModel.cs` |
| Window | `{WidgetName}Window.xaml` | `ClockWindow.xaml` |
| Settings Window | `{WidgetName}SettingsWindow.xaml` | `ClockSettingsWindow.xaml` |
| View | `{WidgetName}View.xaml` | `ClockView.xaml` |
| Model | `{EntityName}.cs` | `HistoricalEvent.cs` |
| Helper | `{Purpose}Helper.cs` | `DateTimeHelper.cs` |
| Converter | `{Purpose}Converter.cs` | `StringToVisibilityConverter.cs` |

---

### 4. Widget Categories (for Organization)

Organize your thinking by category (even if not enforced in folders):

| Category | Examples | Purpose |
|----------|----------|---------|
| **Productivity** | Clock, Pomodoro, Notes, Calendar | Time management, organization |
| **Information** | ThisDayInHistory, MoonPhase, SystemPulse | Educational, informative |
| **Utilities** | ClipboardHistory, Folders, QuickLinks | System utilities, shortcuts |
| **Media** | ImageViewer, VideoViewer, PdfViewer | File viewers |
| **Design** | GradientPlayground, LogoSizeTester | Design tools |
| **Entertainment** | GameVault, DesktopPet | Fun, games |
| **Wellness** | Breathe, AmbientSounds | Health, relaxation |

**Use in manifest.json:**
```json
{
  "category": "Productivity"
}
```

---

### 5. Version Management

Keep versions synchronized:

```
manifest.json
  "version": "1.2.3"
        ▼
.csproj
  <Version>1.2.3</Version>
```

**Versioning Strategy:**
- **v1.0.0** - Initial release
- **v1.1.0** - Add new feature
- **v1.1.1** - Bug fix
- **v2.0.0** - Breaking changes

**When to bump:**
- **Patch (1.0.x)** - Bug fixes, minor tweaks
- **Minor (1.x.0)** - New features, backward compatible
- **Major (x.0.0)** - Breaking API changes, major redesign

---

## 🔄 Workflow for Creating New Widgets

### Step-by-Step Checklist

```
[ ] 1. Plan widget (check WIDGET-IDEAS-MEGA-LIST.md)
[ ] 2. Create project folder: 3SC.Widgets.{WidgetName}
[ ] 3. Copy template from .github/skills/templates/new-widget-template.md
[ ] 4. Create manifest.json with unique widgetKey
[ ] 5. Implement Factory, ViewModel, Window
[ ] 6. Add icon.png to Assets/ (128×128)
[ ] 7. Add to widgets.sln
[ ] 8. Add to WidgetTestHost (for testing)
[ ] 9. Build & test locally
[ ] 10. Package with Build-Widget.ps1
[ ] 11. Create README.md
[ ] 12. Submit to Workshop Portal
```

---

## 🧹 Keeping the Repository Clean

### What to .gitignore

The repository already ignores:
```gitignore
bin/
obj/
packages/
*.user
*.suo
.vs/
```

**DO NOT commit:**
- ❌ Build outputs (bin/, obj/)
- ❌ Package files (packages/*.3scwidget)
- ❌ User-specific settings (*.user)
- ❌ IDE files (.vs/, .idea/)

**DO commit:**
- ✅ Source code (.cs, .xaml)
- ✅ Project files (.csproj, .sln)
- ✅ Manifests (manifest.json)
- ✅ Documentation (README.md, skills/)
- ✅ Assets (icon.png, but optimize size)

---

### Clean Up Old Widgets

If a widget is deprecated:

```powershell
# 1. Remove from WidgetTestHost
#    - Delete from WidgetTestHost.csproj
#    - Remove from MainWindow.xaml.cs

# 2. Remove from solution
#    - Delete from widgets.sln

# 3. Archive or delete folder
Move-Item "3SC.Widgets.OldWidget" "../archived-widgets/"
# Or delete entirely
```

---

## 📊 Tracking Widget Count

As you grow, track your widgets:

**Current Count:** 23 widgets (as of 2026-01-20)

Create a `WIDGETS-INVENTORY.md` file:

```markdown
# Widgets Inventory

| # | Name | Key | Category | Status | Version |
|---|------|-----|----------|--------|---------|
| 1 | Clock | clock | Productivity | ✅ Active | 1.2.0 |
| 2 | This Day in History | this-day-in-history | Information | ✅ Active | 1.0.0 |
| ... | ... | ... | ... | ... | ... |

**Total Active:** 23
**Total Archived:** 0
```

---

## 🏗️ Scaling Strategies

### For 50+ Widgets

Consider organizing by category:

```
widgets/
├── Productivity/
│   ├── 3SC.Widgets.Clock/
│   ├── 3SC.Widgets.Pomodoro/
│   └── 3SC.Widgets.Notes/
├── Information/
│   ├── 3SC.Widgets.ThisDayInHistory/
│   └── 3SC.Widgets.MoonPhase/
└── ...
```

**Update solution file structure:**
```
Solution 'widgets' (23 projects)
├── Productivity (5 projects)
│   ├── 3SC.Widgets.Clock
│   └── ...
├── Information (3 projects)
└── ...
```

### For 100+ Widgets

Consider **separate repositories** by category:
- `widgets-productivity/`
- `widgets-information/`
- `widgets-media/`

---

## 🛠️ Automation Scripts

### List All Widgets

```powershell
# List-Widgets.ps1
Get-ChildItem -Directory -Filter "3SC.Widgets.*" | 
    Where-Object { $_.Name -ne "3SC.Widgets.Contracts" } |
    ForEach-Object {
        $manifest = Get-Content "$($_.FullName)\manifest.json" -ErrorAction SilentlyContinue | ConvertFrom-Json
        [PSCustomObject]@{
            Name = $_.Name
            Key = $manifest.widgetKey
            Version = $manifest.version
            Category = $manifest.category
        }
    } | Format-Table
```

### Check for Missing Icons

```powershell
# Check-Assets.ps1
Get-ChildItem -Directory -Filter "3SC.Widgets.*" | 
    Where-Object { $_.Name -ne "3SC.Widgets.Contracts" } |
    ForEach-Object {
        $iconPath = "$($_.FullName)\Assets\icon.png"
        if (-not (Test-Path $iconPath)) {
            Write-Host "⚠️  Missing icon: $($_.Name)" -ForegroundColor Yellow
        }
    }
```

### Validate All Manifests

```powershell
# Validate-Manifests.ps1
Get-ChildItem -Directory -Filter "3SC.Widgets.*" |
    Where-Object { $_.Name -ne "3SC.Widgets.Contracts" } |
    ForEach-Object {
        $manifestPath = "$($_.FullName)\manifest.json"
        if (Test-Path $manifestPath) {
            $manifest = Get-Content $manifestPath | ConvertFrom-Json
            
            # Check required fields
            $required = @('widgetKey', 'name', 'version', 'category')
            foreach ($field in $required) {
                if (-not $manifest.$field) {
                    Write-Host "❌ $($_.Name): Missing $field" -ForegroundColor Red
                }
            }
        } else {
            Write-Host "❌ $($_.Name): No manifest.json" -ForegroundColor Red
        }
    }
```

---

## 📝 Documentation Standards

### Widget README Template

Every widget should have a README.md:

```markdown
# {Widget Name}

{Brief description}

## Features

- Feature 1
- Feature 2

## Usage

{How to use}

## Settings

{Configuration options}

## Technical Details

- Category: {Category}
- Version: {Version}
- Dependencies: {List}

## Screenshots

{Optional}

## License

Part of 3SC Widget Ecosystem
```

---

## 🎨 Asset Management

### Icon Guidelines

**Requirements:**
- **Size:** 128×128 pixels
- **Format:** PNG with transparency
- **Style:** Consistent across all widgets
- **File size:** < 50 KB

**Naming:**
- ✅ `icon.png` (lowercase)
- ❌ `Icon.png`, `ICON.PNG`

**Storage:**
```
3SC.Widgets.YourWidget/
└── Assets/
    ├── icon.png           # 128×128 main icon
    ├── preview.png        # Optional 800×600 preview
    └── screenshots/       # Optional gallery
```

---

## 🧪 Testing Organization

### WidgetTestHost Management

**When adding a new widget:**

1. **Add project reference** to `WidgetTestHost.csproj`:
```xml
<ProjectReference Include="..\3SC.Widgets.YourWidget\3SC.Widgets.YourWidget.csproj" />
```

2. **Register factory** in `MainWindow.xaml.cs`:
```csharp
_widgetFactories["Your Widget"] = new _3SC.Widgets.YourWidget.YourWidgetFactory();
```

**Keep factories alphabetically sorted** for easy navigation.

---

## 📦 Package Management

### packages/ Folder

The `packages/` folder contains built .3scwidget files:

```
packages/
├── clock-widget.3scwidget
├── thisdayinhistory-widget.3scwidget
└── ...
```

**Management:**
- ✅ Generated by `Build-Widget.ps1`
- ✅ .gitignored (don't commit)
- ✅ Distribute via Workshop Portal or GitHub Releases
- ❌ Never manually edit

**Cleanup:**
```powershell
# Remove all packages
Remove-Item packages/*.3scwidget

# Rebuild specific widget
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.Clock"
```

---

## 🔍 Quick Reference Commands

```powershell
# Build single widget
.\Build-Widget.ps1 -WidgetName "3SC.Widgets.YourWidget"

# Build all widgets
.\Build-Widget.ps1 -All

# Install for testing
.\Install-Widget.ps1 -WidgetKey yourwidget

# List installed
.\Install-Widget.ps1 -List

# Uninstall
.\Install-Widget.ps1 -Uninstall -WidgetKey yourwidget

# Build and run test host
dotnet build WidgetTestHost
dotnet run --project WidgetTestHost

# List all widgets in repo
Get-ChildItem -Directory -Filter "3SC.Widgets.*" | Select-Object Name

# Count widgets
(Get-ChildItem -Directory -Filter "3SC.Widgets.*" | 
 Where-Object { $_.Name -ne "3SC.Widgets.Contracts" }).Count
```

---

## 🎓 Learning Path for New Contributors

1. **Read:** `.github/copilot-instructions.md`
2. **Review:** `.github/skills/SKILL-INDEX.md`
3. **Study:** Existing widget (start with Clock)
4. **Create:** Simple widget following templates
5. **Test:** Using WidgetTestHost
6. **Package:** Using Build-Widget.ps1
7. **Document:** Write README.md
8. **Contribute:** Submit to Workshop

---

## 🚀 Future Organization Ideas

### When the repo grows beyond 50 widgets:

1. **Monorepo Tools:**
   - Lerna/Nx for multi-project management
   - Automated dependency updates
   - Shared component library

2. **CI/CD:**
   - GitHub Actions for automated builds
   - Automated testing on every PR
   - Auto-release to Workshop Portal

3. **Widget Marketplace:**
   - Self-serve publishing portal
   - Automated approval workflow
   - Analytics dashboard

4. **Shared Libraries:**
   - `3SC.Widgets.Common` - Shared utilities
   - `3SC.Widgets.UI` - Reusable UI components
   - `3SC.Widgets.Data` - Data access layer

---

## 📌 Summary Checklist

When creating a new widget, ensure:

- [ ] Unique widgetKey (lowercase, hyphens)
- [ ] Follows naming convention (3SC.Widgets.{Name})
- [ ] Has all required files (manifest, factory, icon)
- [ ] Added to widgets.sln
- [ ] Added to WidgetTestHost (both .csproj and MainWindow.xaml.cs)
- [ ] Builds successfully
- [ ] Has README.md
- [ ] Packaged with Build-Widget.ps1
- [ ] Tested in both WidgetTestHost and 3SC host

**Stay organized, stay productive!** 🎯

---

*Last Updated: 2026-01-20 | Widgets Count: 23 | Maintainer: 3SC Team*
