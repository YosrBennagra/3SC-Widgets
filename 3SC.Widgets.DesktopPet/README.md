# 🐾 Desktop Pet Widget

> An adorable interactive companion that lives on your desktop!

![Desktop Pet Preview](Assets/preview.gif)

## ✨ Features

### 🎭 Expressive Pet
- **Living Eyes** - Eyes follow your mouse cursor around the screen
- **Blinking** - Natural random blinking animation
- **Emotions** - Facial expressions change based on mood
- **Smooth Animations** - Breathing, bouncing, stretching, and more!

### 🧠 Smart AI Behavior
- **Autonomous Walking** - Pet randomly explores your desktop
- **Auto Sleep** - Gets tired and sleeps to restore energy
- **Mood System** - Happiness affected by care and attention
- **State Machine** - Idle, Walking, Sleeping, Eating, Playing, and more

### 💝 Interactions
- **Feed** (Right-click → Feed) - Give your pet food to restore hunger
- **Play** (Right-click → Play) - Play with your pet for happiness boost
- **Pet** (Double-click) - Show affection with hearts!
- **Sleep** (Right-click → Sleep) - Put your pet to bed

### 📊 Needs System
| Stat | Description | Decay Rate |
|------|-------------|------------|
| ❤️ **Happiness** | Overall mood | Slow decay, faster if hungry/tired |
| ⚡ **Energy** | Tiredness level | Decreases when awake, restores when sleeping |
| 🍎 **Hunger** | Food level | Steady decay over time |

### 🎨 Customization
- **6 Color Options**: Blue 💙, Pink 💗, Green 💚, Purple 💜, Orange 🧡, Mint 🩵
- **Toggle Auto-Walk**: Enable/disable autonomous movement
- **Toggle Effects**: Show/hide particle effects (hearts, sparkles, ZZZ)
- **Lock Position**: Prevent accidental dragging

### ✨ Particle Effects
- **Hearts** 💕 - Float up when being petted
- **Sparkles** ✨ - Appear when playing
- **ZZZ** 💤 - Float when sleeping
- **Food** 🍎 - Bounces when eating

### 💾 Persistence
- Pet state saves automatically
- Remembers position between sessions
- Tracks lifetime statistics (times fed, played, petted)
- Calculates time-based decay when you're away

## 🚀 Quick Start

### Debug Testing
```powershell
cd 3SC.Widgets.DesktopPet
dotnet build -c Debug
dotnet run
```

### Build for Distribution
```powershell
.\Build-And-Package-DesktopPet.ps1
```

### Install Locally
```powershell
.\Build-And-Package-DesktopPet.ps1 -InstallLocal
# Then restart 3SC
```

## 🎮 Controls

| Action | How |
|--------|-----|
| Move pet | Drag with left mouse button |
| Show menu | Right-click |
| Pet / Show love | Double-click |
| View stats | Hover for tooltip OR right-click menu |

## 📁 Project Structure

```
3SC.Widgets.DesktopPet/
├── 3SC.Widgets.DesktopPet.csproj    # Project configuration
├── manifest.json                     # Widget metadata
│
├── DesktopPetWidgetFactory.cs       # Factory + IWidget implementation
├── DesktopPetViewModel.cs           # ViewModel with AI and state
├── DesktopPetWindow.xaml            # Pet UI and animations
├── DesktopPetWindow.xaml.cs         # Window code-behind
├── WidgetWindowBase.cs              # Base class for widget behavior
│
├── PetState.cs                      # State enums (Idle, Walking, etc.)
├── PetSettings.cs                   # Persistence model
│
├── TestLauncher.cs                  # Standalone debug runner
│
├── Helpers/
│   ├── WidgetBehaviorHelper.cs      # Drag, resize, lock functionality
│   ├── Win32Interop.cs              # P/Invoke for window behavior
│   └── ScreenBoundsHelper.cs        # Multi-monitor support
│
└── Assets/
    ├── preview.png                  # Static preview
    └── preview.gif                  # Animated preview
```

## 🔧 Technical Details

### Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.2.2 | MVVM framework |
| Serilog | 3.1.1 | Logging |
| Serilog.Sinks.File | 5.0.0 | File logging |
| System.Drawing.Common | 8.0.7 | Screen bounds |

### Pet States
```
    ┌─────────┐
    │  Idle   │◄────────────────────┐
    └────┬────┘                     │
         │ random chance            │ timeout
         ▼                          │
    ┌─────────┐                ┌────┴────┐
    │ Walking │───────────────►│ Arrived │
    └─────────┘   reached      └─────────┘
         │        target
         │ low energy
         ▼
    ┌─────────┐
    │Sleeping │────► energy full ───► Idle
    └─────────┘

User Actions:
    Feed ──► Eating ──► Idle
    Play ──► Playing ──► Idle  
    Pet ──► BeingPetted ──► Idle
```

### Animation System
- **60 FPS** animation timer for smooth visuals
- **Body transforms**: ScaleX, ScaleY, TranslateY
- **Eye tracking**: Real-time mouse position tracking
- **Particle systems**: Hearts, sparkles, ZZZ, food

## 🐛 Known Issues

- None currently! Report issues via GitHub.

## 📜 License

MIT License - Part of the 3SC Widgets project.

---

Made with 💙 for the 3SC community
