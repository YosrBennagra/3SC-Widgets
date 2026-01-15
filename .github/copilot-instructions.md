# 🤖 Copilot Instructions for 3SC Widget Development

> **Version:** 2.0.0 | **Last Updated:** 2026-01-15 | **Maintained By:** 3SC Team

## 📋 Table of Contents

1. [Quick Start](#quick-start)
2. [Project Overview](#project-overview)
3. [Skill System](#skill-system)
4. [How to Use Skills](#how-to-use-skills)
5. [Skill Management](#skill-management)
6. [Development Workflow](#development-workflow)
7. [Critical Rules](#critical-rules)

---

## 🚀 Quick Start

When working on this project, **ALWAYS** consult the relevant skill files in `.github/skills/` before:
- Creating new widgets
- Implementing features
- Writing tests
- Building/packaging
- Troubleshooting issues

### Essential Commands

```powershell
# Build a widget for debugging
dotnet build -c Debug

# Build and publish for release
dotnet publish -c Release -o bin\Release\net8.0-windows\publish

# Run tests
dotnet test

# Package a widget
.\Build-And-Package-{WidgetName}.ps1
```

---

## 🏗️ Project Overview

### What is 3SC?

3SC is a modern Windows desktop widget application that allows users to place interactive widgets on their desktop. This repository (`widgets/`) contains **external community widgets** that integrate with the main 3SC application.

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      3SC Host Application                    │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐│
│  │  Widget Loader  │  │ Settings Service│  │ Theme Service││
│  └────────┬────────┘  └────────┬────────┘  └──────┬───────┘│
└───────────┼────────────────────┼─────────────────┼─────────┘
            │                    │                 │
            ▼                    ▼                 ▼
┌───────────────────────────────────────────────────────────────┐
│                    3SC.Widgets.Contracts                       │
│  ┌──────────────┐  ┌────────────────┐  ┌───────────────────┐ │
│  │   IWidget    │  │IExternalWidget │  │ IWidgetFactory    │ │
│  └──────────────┘  └────────────────┘  └───────────────────┘ │
└───────────────────────────────────────────────────────────────┘
            ▲                    ▲                 ▲
            │                    │                 │
┌───────────┴────────────────────┴─────────────────┴───────────┐
│                    External Widgets (This Repo)               │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────────┐ │
│  │  Clock  │  │  Notes  │  │ Calendar│  │  Your Widget!   │ │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────────┘ │
└───────────────────────────────────────────────────────────────┘
```

### Key Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Runtime |
| WPF | - | UI Framework |
| CommunityToolkit.MVVM | 8.2.2 | MVVM Framework |
| Serilog | 3.1.1 | Logging |
| xUnit | Latest | Testing |

---

## 📚 Skill System

### What are Skills?

Skills are specialized knowledge files that teach Copilot about specific aspects of this project. They contain:
- **Patterns** - Code patterns and conventions to follow
- **Templates** - Ready-to-use code templates
- **Rules** - Critical requirements and constraints
- **Examples** - Real examples from this codebase

### Skill Categories

```
.github/skills/
├── 📘 SKILL-INDEX.md                    # Master index of all skills
├── 📁 core/                             # Core architecture skills
│   ├── widget-architecture.md           # Widget structure & lifecycle
│   ├── contracts-interfaces.md          # IWidget, IWidgetFactory
│   ├── manifest-specification.md        # manifest.json format
│   └── project-setup.md                 # .csproj configuration
│
├── 📁 ui/                               # UI/UX skills
│   ├── mvvm-patterns.md                 # MVVM implementation
│   ├── xaml-styling.md                  # XAML patterns & theme
│   ├── drag-resize.md                   # Drag & resize behavior
│   ├── context-menus.md                 # Context menu patterns
│   └── animations.md                    # Animation patterns
│
├── 📁 data/                             # Data & settings skills
│   ├── settings-management.md           # Settings storage patterns
│   ├── data-persistence.md              # File/database storage
│   ├── serialization.md                 # JSON serialization
│   └── configuration.md                 # Widget configuration
│
├── 📁 integration/                      # Integration skills
│   ├── host-communication.md            # Widget-host interaction
│   ├── service-locator.md               # Service access patterns
│   ├── external-apis.md                 # External API integration
│   └── inter-widget.md                  # Widget-to-widget comm
│
├── 📁 packaging/                        # Build & deploy skills
│   ├── build-configuration.md           # Build settings
│   ├── packaging-deployment.md          # Package creation
│   ├── versioning.md                    # Version management
│   └── distribution.md                  # Distribution channels
│
├── 📁 quality/                          # Quality assurance skills
│   ├── testing-strategies.md            # Test patterns
│   ├── error-handling.md                # Exception handling
│   ├── logging.md                       # Serilog patterns
│   └── code-standards.md                # Coding conventions
│
├── 📁 performance/                      # Performance skills
│   ├── optimization.md                  # Performance patterns
│   ├── memory-management.md             # Memory best practices
│   ├── async-patterns.md                # Async/await patterns
│   └── rendering.md                     # UI rendering perf
│
├── 📁 advanced/                         # Advanced feature skills
│   ├── media-handling.md                # Image/video/PDF
│   ├── file-operations.md               # File system access
│   ├── notifications.md                 # Toast/alerts
│   ├── localization.md                  # Multi-language support
│   └── accessibility.md                 # Accessibility features
│
├── 📁 security/                         # Security skills
│   ├── input-validation.md              # Input sanitization
│   ├── secure-storage.md                # Credential storage
│   └── permissions.md                   # Permission system
│
├── 📁 troubleshooting/                  # Debugging skills
│   ├── common-issues.md                 # FAQ & solutions
│   ├── debugging-guide.md               # Debug techniques
│   └── migration-guide.md               # Migration patterns
│
└── 📁 templates/                        # Ready-to-use templates
    ├── new-widget-template.md           # Complete widget template
    ├── viewmodel-template.md            # ViewModel template
    ├── window-template.md               # Window XAML template
    └── test-template.md                 # Test class template
```

---

## 🎯 How to Use Skills

### When Creating a New Widget

1. **Start with:** `core/widget-architecture.md`
2. **Then read:** `core/project-setup.md`
3. **Use template:** `templates/new-widget-template.md`
4. **For UI:** `ui/mvvm-patterns.md` + `ui/xaml-styling.md`
5. **For settings:** `data/settings-management.md`
6. **For packaging:** `packaging/packaging-deployment.md`

### When Implementing Features

| Feature Type | Primary Skill | Secondary Skills |
|--------------|---------------|------------------|
| Settings UI | `data/settings-management.md` | `ui/mvvm-patterns.md` |
| File handling | `advanced/file-operations.md` | `data/data-persistence.md` |
| Media display | `advanced/media-handling.md` | `performance/rendering.md` |
| API integration | `integration/external-apis.md` | `quality/error-handling.md` |
| Animations | `ui/animations.md` | `performance/rendering.md` |

### Quick Reference Pattern

```
@copilot Reference skill: .github/skills/[category]/[skill-name].md

Then describe what you want to do.
```

---

## 🔧 Skill Management

### Updating Skills

Skills should be updated when:
1. **New patterns emerge** - Document new successful patterns
2. **Breaking changes occur** - Update after .NET/WPF updates
3. **Bugs are discovered** - Add to troubleshooting skills
4. **Templates improve** - Refine based on real usage

### Skill Update Process

```markdown
## Skill Update Checklist

- [ ] Identify the skill file to update
- [ ] Document the change reason in the skill's changelog section
- [ ] Update the "Last Updated" header
- [ ] Bump the skill version if breaking change
- [ ] Update SKILL-INDEX.md if skill scope changes
- [ ] Test any code examples still work
- [ ] Update cross-references to other skills
```

### Removing Skills

Skills should be removed when:
- The pattern is deprecated
- Better alternatives exist (reference the replacement)
- The feature is no longer supported

### Merging Skills

Skills should be merged when:
- Two skills have significant overlap (>40%)
- Topics are better understood together
- Simplification improves usability

---

## 💼 Development Workflow

### Testing vs Production

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           DEVELOPMENT (Testing)                          │
│                                                                          │
│  Build Release ──▶ Copy files UNZIPPED to:                              │
│                    %APPDATA%\3SC\Widgets\Community\{widget-key}\        │
│                    ──▶ Restart 3SC host                                 │
│                                                                          │
│  Required files: manifest.json + YourWidget.dll + dependencies          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                          PRODUCTION (Distribution)                       │
│                                                                          │
│  Build Release ──▶ Package as ZIP (.3scwidget)                          │
│                    ──▶ Upload to Workshop Portal                        │
│                    ──▶ Admin/Reviewer approval                          │
│                    ──▶ Available in 3SC host app                        │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1. Local Testing (Quick Iteration)

```powershell
# Build and deploy for testing
dotnet build -c Release
$dest = "$env:APPDATA\3SC\Widgets\Community\your-widget-key"
New-Item -ItemType Directory -Force -Path $dest
Copy-Item ".\bin\Release\net8.0-windows\*.dll" $dest -Exclude "3SC.Widgets.Contracts.dll"
Copy-Item ".\bin\Release\net8.0-windows\manifest.json" $dest
# Restart 3SC host app
```

### 2. Feature Development

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Read Skills │ ──▶ │ Implement    │ ──▶ │ Test        │
└─────────────┘     └──────────────┘     └─────────────┘
       │                   │                    │
       │                   ▼                    │
       │           ┌──────────────┐            │
       └─────────▶ │ Update Skill │ ◀──────────┘
                   │ (if needed)  │
                   └──────────────┘
```

### 3. Bug Fix

1. Check `troubleshooting/common-issues.md` first
2. If new issue, fix it
3. Add solution to `troubleshooting/common-issues.md`

### 4. New Widget

1. Copy from `templates/new-widget-template.md`
2. Follow `core/widget-architecture.md`
3. Implement features using relevant skills
4. **Test locally** by copying files to Community folder
5. **Package for release** using `packaging/packaging-deployment.md`
6. **Submit to Workshop Portal** for approval

---

## ⚠️ Critical Rules

### MUST Follow

| Rule | Reason |
|------|--------|
| Use Serilog **3.1.1** | Host compatibility |
| Use CommunityToolkit.Mvvm **8.2.2** | API consistency |
| Target **net8.0-windows** | Platform requirement |
| Use **StaticResource** in widgets | External widgets don't inherit app resources |
| Implement **IWidgetFactory** | Required for widget discovery |
| Include **manifest.json** | Required for widget registration |
| Match **widgetKey** everywhere | Must be identical in manifest and code |

### MUST NOT Do

| Anti-Pattern | Why |
|--------------|-----|
| Use Serilog 4.x | Breaking changes with host |
| Use DynamicResource for colors | Won't work in external widgets |
| Skip OnDispose cleanup | Memory leaks |
| Hardcode paths | Use %APPDATA% patterns |
| Block UI thread | Causes freezes |
| Skip null checks | Crash risk |

### Performance Targets

| Metric | Target |
|--------|--------|
| Widget startup | < 500ms |
| Memory footprint | < 50MB |
| UI responsiveness | 60 FPS |
| Settings save | < 100ms |

---

## 📞 Getting Help

1. **Check Skills First** - Most answers are in skill files
2. **Check Troubleshooting** - `troubleshooting/common-issues.md`
3. **Check Examples** - Look at existing widgets (Clock, Notes)
4. **Ask Copilot** - Reference specific skills in your question

---

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Complete skill system overhaul |
| 1.0.0 | 2025-06-01 | Initial instructions |

---

*This file is automatically referenced by GitHub Copilot. Keep it updated!*
