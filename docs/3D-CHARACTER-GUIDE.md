# 🎮 Creating a 3D Character Widget (like Captain Price)

> Guide for implementing high-quality 3D characters in 3SC widgets

## Overview

To create a high-quality 3D character widget like Captain Price from Call of Duty, you have several options depending on your requirements.

---

## Option 1: HelixToolkit (Recommended for WPF)

```xml
<PackageReference Include="HelixToolkit.Wpf" Version="2.24.0" />
```

**Capabilities:**
- Supports FBX, OBJ, GLTF model loading
- Hardware-accelerated 3D rendering
- Skeletal animation support
- Good performance for desktop widgets

**Example Usage:**
```xml
<helix:HelixViewport3D>
    <helix:DefaultLights/>
    <ModelVisual3D Content="{Binding CharacterModel}"/>
</helix:HelixViewport3D>
```

---

## Option 2: Unity WebGL Embedded

- Create character in Unity with full animations
- Export as WebGL, embed in WebView2 control
- Best for complex AAA-quality characters
- Higher resource usage

**Pros:**
- Full Unity engine capabilities
- Professional-grade rendering
- Complex shader support

**Cons:**
- Heavy resource usage
- Requires WebView2
- Larger distribution size

---

## Option 3: MonoGame/FNA Integration

- Game engine for .NET
- Full 3D pipeline with shaders
- More work but maximum control

**Best for:**
- Custom rendering requirements
- Game-like interactions
- Full control over pipeline

---

## Assets Required

| Asset | Description | Recommended Specs |
|-------|-------------|-------------------|
| **3D Model** | FBX/GLTF character mesh | ~50k triangles for quality |
| **Textures** | Diffuse, Normal, Specular maps | 2K-4K resolution |
| **Skeleton** | Armature for animations | 50-100 bones |
| **Animations** | Idle, walk, wave, reactions | FBX format |
| **Shaders** | PBR shaders for realistic rendering | HLSL/GLSL |

---

## Development Steps

### 1. Get/Create Model
- **Purchase:** Turbosquid, CGTrader, Sketchfab
- **Create:** Blender, Maya, ZBrush
- **Game Rips:** Some games allow model extraction (check licensing!)

### 2. Optimize for Real-time
```
Original Model → Reduce Polygons → Bake Textures → LOD Generation
```
- Keep polygon count under 100k for smooth desktop use
- Bake high-poly details into normal maps
- Create LOD (Level of Detail) variants

### 3. Animate
- Create idle loops (8-15 seconds)
- Reaction animations (wave, nod, thumbs up)
- Emotion states (happy, sad, excited)
- Export with embedded animations in FBX

### 4. Export Settings
```
Format: FBX 2020 or GLTF 2.0
Scale: 1 unit = 1 meter
Up Axis: Y-Up
Include: Mesh, Armature, Animations, Materials
```

### 5. Integrate with HelixToolkit

```csharp
using HelixToolkit.Wpf;

public class Character3DViewModel : ObservableObject
{
    private Model3D _characterModel;
    
    public async Task LoadCharacterAsync(string modelPath)
    {
        var importer = new ModelImporter();
        CharacterModel = await Task.Run(() => importer.Load(modelPath));
    }
    
    public Model3D CharacterModel
    {
        get => _characterModel;
        set => SetProperty(ref _characterModel, value);
    }
}
```

### 6. AI Behavior
- Same state machine logic as 2D pet
- Map states to animation clips
- Blend between animations smoothly

---

## Performance Considerations

| Aspect | Recommendation |
|--------|----------------|
| **Polygon Count** | < 100k for desktop widgets |
| **Texture Size** | 2K max, use compression |
| **Animation FPS** | 30 FPS is sufficient |
| **GPU Acceleration** | Required for smooth rendering |
| **Memory** | ~100-200MB for full character |
| **CPU Usage** | < 5% idle, < 15% animating |

---

## Sample Project Structure

```
3SC.Widgets.Character3D/
├── Assets/
│   ├── Models/
│   │   └── captain_price.fbx
│   ├── Textures/
│   │   ├── diffuse.png
│   │   ├── normal.png
│   │   └── specular.png
│   └── Animations/
│       ├── idle.fbx
│       ├── wave.fbx
│       └── talk.fbx
├── Character3DWidget.xaml
├── Character3DViewModel.cs
├── AnimationController.cs
└── manifest.json
```

---

## Resources

- [HelixToolkit Documentation](https://github.com/helix-toolkit/helix-toolkit)
- [Mixamo](https://www.mixamo.com/) - Free character animations
- [Sketchfab](https://sketchfab.com/) - 3D model marketplace
- [Blender](https://www.blender.org/) - Free 3D modeling software

---

*Last Updated: January 2026*
