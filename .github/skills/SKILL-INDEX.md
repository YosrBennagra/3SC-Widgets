# 📚 3SC Widget Skills Index

> **Master Reference for All Copilot Skills**
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

---

## 🎯 Quick Navigation

| Need to... | Go to Skill |
|------------|-------------|
| Create a new widget | [new-widget-template.md](templates/new-widget-template.md) |
| Understand widget structure | [widget-architecture.md](core/widget-architecture.md) |
| Implement settings | [settings-management.md](data/settings-management.md) |
| Style your UI | [xaml-styling.md](ui/xaml-styling.md) |
| Add drag/resize | [drag-resize.md](ui/drag-resize.md) |
| Package for release | [packaging-deployment.md](packaging/packaging-deployment.md) |
| Debug issues | [debugging-guide.md](troubleshooting/debugging-guide.md) |
| Fix common problems | [common-issues.md](troubleshooting/common-issues.md) |

---

## 📁 Skills by Category

### 🏗️ Core Architecture (`core/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [widget-architecture.md](core/widget-architecture.md) | Widget structure, lifecycle, discovery | 🔴 Critical |
| [contracts-interfaces.md](core/contracts-interfaces.md) | IWidget, IWidgetFactory, IExternalWidget | 🔴 Critical |
| [manifest-specification.md](core/manifest-specification.md) | manifest.json format & validation | 🔴 Critical |
| [project-setup.md](core/project-setup.md) | .csproj configuration, dependencies | 🔴 Critical |

### 🎨 User Interface (`ui/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [mvvm-patterns.md](ui/mvvm-patterns.md) | MVVM with CommunityToolkit.MVVM | 🔴 Critical |
| [xaml-styling.md](ui/xaml-styling.md) | XAML patterns, 3SC dark theme | 🔴 Critical |
| [drag-resize.md](ui/drag-resize.md) | Widget dragging & resizing | 🟡 Important |
| [context-menus.md](ui/context-menus.md) | Right-click context menus | 🟡 Important |
| [animations.md](ui/animations.md) | Animations & transitions | 🟢 Optional |

### 💾 Data & Settings (`data/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [settings-management.md](data/settings-management.md) | Settings storage & UI | 🔴 Critical |
| [data-persistence.md](data/data-persistence.md) | File & state persistence | 🟡 Important |
| [serialization.md](data/serialization.md) | JSON serialization patterns | 🟡 Important |
| [configuration.md](data/configuration.md) | Widget configuration options | 🟡 Important |

### 🔌 Integration (`integration/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [host-communication.md](integration/host-communication.md) | Widget-to-host interaction | 🟡 Important |
| [service-locator.md](integration/service-locator.md) | Accessing host services | 🟡 Important |
| [external-apis.md](integration/external-apis.md) | REST APIs, web services | 🟢 Optional |
| [inter-widget.md](integration/inter-widget.md) | Widget-to-widget communication | 🟢 Optional |

### 📦 Packaging & Deployment (`packaging/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [build-configuration.md](packaging/build-configuration.md) | Build settings & optimization | 🔴 Critical |
| [packaging-deployment.md](packaging/packaging-deployment.md) | Package creation & distribution | 🔴 Critical |
| [versioning.md](packaging/versioning.md) | Semantic versioning | 🟡 Important |
| [distribution.md](packaging/distribution.md) | Widget store, updates | 🟢 Optional |

### ✅ Quality Assurance (`quality/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [testing-strategies.md](quality/testing-strategies.md) | Unit & integration testing | 🟡 Important |
| [error-handling.md](quality/error-handling.md) | Exception handling patterns | 🔴 Critical |
| [logging.md](quality/logging.md) | Serilog configuration | 🟡 Important |
| [code-standards.md](quality/code-standards.md) | Coding conventions | 🟡 Important |

### ⚡ Performance (`performance/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [optimization.md](performance/optimization.md) | General optimization | 🟡 Important |
| [memory-management.md](performance/memory-management.md) | Memory best practices | 🟡 Important |
| [async-patterns.md](performance/async-patterns.md) | Async/await patterns | 🟡 Important |
| [rendering.md](performance/rendering.md) | UI rendering performance | 🟢 Optional |

### 🚀 Advanced Features (`advanced/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [media-handling.md](advanced/media-handling.md) | Images, video, PDF | 🟢 Optional |
| [file-operations.md](advanced/file-operations.md) | File system operations | 🟢 Optional |
| [notifications.md](advanced/notifications.md) | Toast notifications | 🟢 Optional |
| [localization.md](advanced/localization.md) | Multi-language support | 🟢 Optional |
| [accessibility.md](advanced/accessibility.md) | Accessibility features | 🟢 Optional |

### 🔒 Security (`security/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [input-validation.md](security/input-validation.md) | Input sanitization | 🟡 Important |
| [secure-storage.md](security/secure-storage.md) | Credential storage | 🟢 Optional |
| [permissions.md](security/permissions.md) | Permission system | 🟢 Optional |

### 🔧 Troubleshooting (`troubleshooting/`)

| Skill | Description | Priority |
|-------|-------------|----------|
| [common-issues.md](troubleshooting/common-issues.md) | FAQ & solutions | 🔴 Critical |
| [debugging-guide.md](troubleshooting/debugging-guide.md) | Debug techniques | 🟡 Important |
| [migration-guide.md](troubleshooting/migration-guide.md) | Version migration | 🟢 Optional |

### 📝 Templates (`templates/`)

| Template | Description |
|----------|-------------|
| [new-widget-template.md](templates/new-widget-template.md) | Complete widget scaffold |
| [viewmodel-template.md](templates/viewmodel-template.md) | ViewModel boilerplate |
| [window-template.md](templates/window-template.md) | Widget window XAML |
| [test-template.md](templates/test-template.md) | Test class template |

---

## 🎓 Learning Paths

### Path 1: New Widget Developer

```
1. core/widget-architecture.md      (30 min)
2. core/contracts-interfaces.md     (20 min)
3. core/project-setup.md            (15 min)
4. templates/new-widget-template.md (Practice)
5. ui/mvvm-patterns.md              (30 min)
6. ui/xaml-styling.md               (30 min)
7. data/settings-management.md      (20 min)
8. packaging/packaging-deployment.md (20 min)
```

### Path 2: UI/UX Focus

```
1. ui/xaml-styling.md
2. ui/mvvm-patterns.md
3. ui/animations.md
4. ui/drag-resize.md
5. ui/context-menus.md
6. performance/rendering.md
```

### Path 3: Advanced Integration

```
1. integration/host-communication.md
2. integration/service-locator.md
3. integration/external-apis.md
4. integration/inter-widget.md
5. security/permissions.md
```

### Path 4: Quality & Performance

```
1. quality/code-standards.md
2. quality/testing-strategies.md
3. quality/error-handling.md
4. quality/logging.md
5. performance/optimization.md
6. performance/memory-management.md
```

---

## 📊 Skill Dependencies

```
                    widget-architecture
                           │
            ┌──────────────┼──────────────┐
            ▼              ▼              ▼
    contracts-interfaces   project-setup   manifest-specification
            │              │              │
            └──────────────┴──────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         ▼                 ▼                 ▼
    mvvm-patterns     xaml-styling    settings-management
         │                 │                 │
         └─────────────────┴─────────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         ▼                 ▼                 ▼
    drag-resize       animations     data-persistence
         │                 │                 │
         └─────────────────┴─────────────────┘
                           │
                           ▼
               packaging-deployment
```

---

## 🔍 Search Tags

### By Technology
- `#wpf` - WPF-specific skills
- `#mvvm` - MVVM patterns
- `#xaml` - XAML templates
- `#json` - JSON handling
- `#async` - Async patterns

### By Task
- `#create` - Creating new things
- `#configure` - Configuration tasks
- `#debug` - Debugging help
- `#optimize` - Performance optimization
- `#test` - Testing guidance

### By Experience Level
- `#beginner` - New to widget development
- `#intermediate` - Some experience
- `#advanced` - Complex scenarios

---

## 📝 Contributing to Skills

### Adding a New Skill

1. Identify the category
2. Create file with naming convention: `kebab-case.md`
3. Use the standard skill template (see below)
4. Add to this index
5. Update dependencies diagram if needed
6. Add cross-references to related skills

### Skill File Template

```markdown
# [Skill Name]

> **Category:** [category] | **Priority:** [Critical/Important/Optional]
> **Version:** 1.0.0 | **Last Updated:** YYYY-MM-DD

## Overview
Brief description of what this skill covers.

## Prerequisites
- Skill 1
- Skill 2

## Core Concepts
### Concept 1
...

## Patterns & Examples
### Pattern 1
...

## Best Practices
...

## Common Mistakes
...

## Related Skills
- [Related Skill 1](path/to/skill.md)

## Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | YYYY-MM-DD | Initial version |
```

---

## 🔄 Maintenance

### Review Schedule

| Frequency | Action |
|-----------|--------|
| Weekly | Check for outdated examples |
| Monthly | Review troubleshooting for new issues |
| Quarterly | Full skill audit |
| On Release | Update version-specific content |

### Version Compatibility

| Skill Version | Compatible With |
|---------------|-----------------|
| 2.x | .NET 8, WPF, 3SC 2.x |
| 1.x | .NET 7, WPF, 3SC 1.x (deprecated) |

---

*Navigate back to [Copilot Instructions](../copilot-instructions.md)*
