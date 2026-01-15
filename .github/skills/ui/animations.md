# Animations

> **Category:** UI | **Priority:** 🟢 Optional
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers implementing smooth animations and transitions for 3SC widgets.

## Prerequisites

- [xaml-styling.md](xaml-styling.md)

---

## Storyboard Animations

### Fade In/Out

```xml
<Window.Resources>
    <!-- Fade In Animation -->
    <Storyboard x:Key="FadeIn">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1"
                         Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    
    <!-- Fade Out Animation -->
    <Storyboard x:Key="FadeOut">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="1" To="0"
                         Duration="0:0:0.2">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseIn"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
</Window.Resources>
```

### Scale Animation

```xml
<Storyboard x:Key="ScaleIn">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                     From="0.8" To="1"
                     Duration="0:0:0.3">
        <DoubleAnimation.EasingFunction>
            <BackEase EasingMode="EaseOut" Amplitude="0.3"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                     From="0.8" To="1"
                     Duration="0:0:0.3">
        <DoubleAnimation.EasingFunction>
            <BackEase EasingMode="EaseOut" Amplitude="0.3"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>

<!-- Element must have RenderTransform -->
<Border RenderTransformOrigin="0.5,0.5">
    <Border.RenderTransform>
        <ScaleTransform/>
    </Border.RenderTransform>
</Border>
```

### Slide Animation

```xml
<Storyboard x:Key="SlideInFromBottom">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                     From="20" To="0"
                     Duration="0:0:0.3">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                     From="0" To="1"
                     Duration="0:0:0.3"/>
</Storyboard>
```

---

## Trigger-Based Animations

### On Load

```xml
<Border x:Name="ContentBorder">
    <Border.Style>
        <Style TargetType="Border">
            <Style.Triggers>
                <EventTrigger RoutedEvent="Loaded">
                    <BeginStoryboard Storyboard="{StaticResource FadeIn}"/>
                </EventTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
</Border>
```

### On Mouse Over

```xml
<Border x:Name="Card">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="RenderTransform">
                <Setter.Value>
                    <ScaleTransform ScaleX="1" ScaleY="1"/>
                </Setter.Value>
            </Setter>
            <Setter Property="RenderTransformOrigin" Value="0.5,0.5"/>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Trigger.EnterActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                                                 To="1.02" Duration="0:0:0.2"/>
                                <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                                                 To="1.02" Duration="0:0:0.2"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </Trigger.EnterActions>
                    <Trigger.ExitActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                                                 To="1" Duration="0:0:0.2"/>
                                <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                                                 To="1" Duration="0:0:0.2"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </Trigger.ExitActions>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
</Border>
```

---

## Loading Animations

### Spinner

```xml
<Border x:Name="Spinner" 
        Width="24" Height="24"
        Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibility}}">
    <Border.RenderTransform>
        <RotateTransform/>
    </Border.RenderTransform>
    <Border.Style>
        <Style TargetType="Border">
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsLoading}" Value="True">
                    <DataTrigger.EnterActions>
                        <BeginStoryboard x:Name="SpinStoryboard">
                            <Storyboard RepeatBehavior="Forever">
                                <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(RotateTransform.Angle)"
                                                 From="0" To="360"
                                                 Duration="0:0:1"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </DataTrigger.EnterActions>
                    <DataTrigger.ExitActions>
                        <StopStoryboard BeginStoryboardName="SpinStoryboard"/>
                    </DataTrigger.ExitActions>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    
    <!-- Spinner visual -->
    <Ellipse Stroke="{StaticResource Accent}"
             StrokeThickness="2"
             StrokeDashArray="4,2"/>
</Border>
```

### Pulse Animation

```xml
<Storyboard x:Key="Pulse" RepeatBehavior="Forever">
    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="Opacity">
        <EasingDoubleKeyFrame KeyTime="0:0:0" Value="1"/>
        <EasingDoubleKeyFrame KeyTime="0:0:0.5" Value="0.5">
            <EasingDoubleKeyFrame.EasingFunction>
                <SineEase EasingMode="EaseInOut"/>
            </EasingDoubleKeyFrame.EasingFunction>
        </EasingDoubleKeyFrame>
        <EasingDoubleKeyFrame KeyTime="0:0:1" Value="1">
            <EasingDoubleKeyFrame.EasingFunction>
                <SineEase EasingMode="EaseInOut"/>
            </EasingDoubleKeyFrame.EasingFunction>
        </EasingDoubleKeyFrame>
    </DoubleAnimationUsingKeyFrames>
</Storyboard>
```

---

## Code-Behind Animations

```csharp
using System.Windows.Media.Animation;

public partial class MyWidgetWindow : Window
{
    public void AnimateIn()
    {
        // Opacity animation
        var fadeAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        // Scale animation
        var scaleXAnimation = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        var scaleYAnimation = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        // Apply animations
        BeginAnimation(OpacityProperty, fadeAnimation);
        
        if (RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }
    }
    
    public async Task AnimateOutAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        
        var fadeAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        
        fadeAnimation.Completed += (s, e) => tcs.SetResult(true);
        
        BeginAnimation(OpacityProperty, fadeAnimation);
        
        await tcs.Task;
    }
}
```

---

## Easing Functions

### Common Easing Functions

```xml
<!-- Smooth deceleration -->
<CubicEase EasingMode="EaseOut"/>

<!-- Smooth acceleration -->
<CubicEase EasingMode="EaseIn"/>

<!-- Smooth both ways -->
<CubicEase EasingMode="EaseInOut"/>

<!-- Bounce effect -->
<BounceEase Bounces="2" Bounciness="2" EasingMode="EaseOut"/>

<!-- Overshoot effect -->
<BackEase Amplitude="0.3" EasingMode="EaseOut"/>

<!-- Elastic effect -->
<ElasticEase Oscillations="1" Springiness="3" EasingMode="EaseOut"/>

<!-- Sine wave (smooth) -->
<SineEase EasingMode="EaseInOut"/>
```

### Easing Reference

| Easing | Use Case |
|--------|----------|
| `CubicEase` | General purpose, smooth |
| `QuadraticEase` | Lighter than cubic |
| `SineEase` | Very subtle, natural |
| `BackEase` | Playful overshoot |
| `BounceEase` | Attention-grabbing |
| `ElasticEase` | Spring-like |

---

## Content Transitions

### Data Trigger Animation

```xml
<TextBlock Text="{Binding Status}">
    <TextBlock.Style>
        <Style TargetType="TextBlock">
            <Style.Triggers>
                <DataTrigger Binding="{Binding HasNewData}" Value="True">
                    <DataTrigger.EnterActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <!-- Quick flash -->
                                <ColorAnimation Storyboard.TargetProperty="(TextBlock.Foreground).(SolidColorBrush.Color)"
                                                To="#2DD4BF" Duration="0:0:0.2"
                                                AutoReverse="True"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </DataTrigger.EnterActions>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </TextBlock.Style>
</TextBlock>
```

---

## Performance Tips

### Use Hardware Acceleration

```xml
<!-- Enable hardware acceleration -->
<Border CacheMode="BitmapCache">
    <!-- Animated content -->
</Border>
```

### Optimize Storyboards

```csharp
// Freeze resources
var brush = new SolidColorBrush(Colors.White);
brush.Freeze();

// Use independent animations when possible
Timeline.SetDesiredFrameRate(storyboard, 60);
```

### Avoid Heavy Animations

```xml
<!-- ❌ Avoid - Layout affecting properties -->
<DoubleAnimation Storyboard.TargetProperty="Width"/>

<!-- ✅ Better - Transform properties -->
<DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"/>
```

---

## Animation Library

### Standard Widget Animations

```xml
<Window.Resources>
    <!-- Widget Appear -->
    <Storyboard x:Key="WidgetAppear">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1" Duration="0:0:0.3"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                         From="10" To="0" Duration="0:0:0.3">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    
    <!-- Widget Disappear -->
    <Storyboard x:Key="WidgetDisappear">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         To="0" Duration="0:0:0.2"/>
    </Storyboard>
    
    <!-- Content Refresh -->
    <Storyboard x:Key="ContentRefresh">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0.5" To="1" Duration="0:0:0.2"/>
    </Storyboard>
    
    <!-- Button Press -->
    <Storyboard x:Key="ButtonPress">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                         To="0.95" Duration="0:0:0.1"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                         To="0.95" Duration="0:0:0.1"/>
    </Storyboard>
    
    <!-- Button Release -->
    <Storyboard x:Key="ButtonRelease">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                         To="1" Duration="0:0:0.1">
            <DoubleAnimation.EasingFunction>
                <BackEase EasingMode="EaseOut" Amplitude="0.2"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                         To="1" Duration="0:0:0.1">
            <DoubleAnimation.EasingFunction>
                <BackEase EasingMode="EaseOut" Amplitude="0.2"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
</Window.Resources>
```

---

## Related Skills

- [xaml-styling.md](xaml-styling.md) - XAML patterns
- [performance/rendering.md](../performance/rendering.md) - Performance

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Animation library added |
| 1.0.0 | 2025-06-01 | Initial version |
