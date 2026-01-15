# XAML Styling

> **Category:** UI | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers XAML patterns, styling, and the 3SC dark theme for widget development.

## Prerequisites

- [mvvm-patterns.md](mvvm-patterns.md)
- [project-setup.md](../core/project-setup.md)

---

## Window Configuration

### Standard Widget Window

```xml
<Window x:Class="_3SC.Widgets.MyWidget.MyWidgetWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:_3SC.Widgets.MyWidget"
        mc:Ignorable="d"
        
        Title="My Widget"
        Width="300"
        Height="200"
        
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        ShowInTaskbar="False"
        ResizeMode="NoResize"
        Topmost="False">
    
    <!-- Widget content here -->
    
</Window>
```

### Required Window Properties

| Property | Value | Purpose |
|----------|-------|---------|
| `WindowStyle` | `None` | Removes title bar and borders |
| `AllowsTransparency` | `True` | Enables transparent background |
| `Background` | `Transparent` | Window itself is transparent |
| `ShowInTaskbar` | `False` | Widget doesn't appear in taskbar |
| `ResizeMode` | `NoResize` | Prevent standard resize handles |
| `Topmost` | `False` | Allow other windows on top |

---

## 3SC Dark Theme

### ⚠️ CRITICAL: Use StaticResource

External widgets **cannot** use `DynamicResource` for colors because they don't inherit from the host application's resource dictionary. Always use `StaticResource`:

```xml
<!-- ✅ Correct for external widgets -->
<Border Background="{StaticResource WidgetBackground}" />

<!-- ❌ WRONG - won't work in external widgets -->
<Border Background="{DynamicResource Brushes.Background}" />
```

### Theme Color Palette

Define these in your widget's resources:

```xml
<Window.Resources>
    <!-- Primary Colors -->
    <SolidColorBrush x:Key="WidgetBackground" Color="#FF0A0A0F"/>
    <SolidColorBrush x:Key="WidgetSurface" Color="#D0080810"/>
    <SolidColorBrush x:Key="SurfaceElevated" Color="#FF16161F"/>
    <SolidColorBrush x:Key="SurfaceHover" Color="#FF1C1C28"/>
    
    <!-- Text Colors -->
    <SolidColorBrush x:Key="TextPrimary" Color="#FFF1F5F9"/>
    <SolidColorBrush x:Key="TextSecondary" Color="#FF94A3B8"/>
    <SolidColorBrush x:Key="TextTertiary" Color="#FF64748B"/>
    <SolidColorBrush x:Key="TextDisabled" Color="#FF475569"/>
    
    <!-- Accent Colors -->
    <SolidColorBrush x:Key="Accent" Color="#FF2DD4BF"/>
    <SolidColorBrush x:Key="AccentHover" Color="#FF5EEAD4"/>
    <SolidColorBrush x:Key="AccentPressed" Color="#FF14B8A6"/>
    
    <!-- Border Colors -->
    <SolidColorBrush x:Key="Border" Color="#FF2A2A3A"/>
    <SolidColorBrush x:Key="BorderHover" Color="#FF3A3A4A"/>
    <SolidColorBrush x:Key="BorderFocus" Color="#FF2DD4BF"/>
    
    <!-- Status Colors -->
    <SolidColorBrush x:Key="Success" Color="#FF22C55E"/>
    <SolidColorBrush x:Key="Warning" Color="#FFFBBF24"/>
    <SolidColorBrush x:Key="Error" Color="#FFEF4444"/>
    <SolidColorBrush x:Key="Info" Color="#FF3B82F6"/>
    
    <!-- Card Colors -->
    <SolidColorBrush x:Key="CardBackground" Color="#FF101018"/>
    <SolidColorBrush x:Key="CardBorder" Color="#FF2A2A3A"/>
    <SolidColorBrush x:Key="CardHover" Color="#FF16161F"/>
</Window.Resources>
```

### Color Reference Chart

```
┌────────────────────────────────────────────────────────┐
│  Background Hierarchy                                   │
├────────────────────────────────────────────────────────┤
│  #0A0A0F  WidgetBackground    (Darkest - base)         │
│  #080810  WidgetSurface       (Slightly elevated)      │
│  #101018  CardBackground      (Card/panel base)        │
│  #16161F  SurfaceElevated     (Elevated elements)      │
│  #1C1C28  SurfaceHover        (Hover states)           │
└────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│  Text Hierarchy                                         │
├────────────────────────────────────────────────────────┤
│  #F1F5F9  TextPrimary         (Main text)              │
│  #94A3B8  TextSecondary       (Secondary text)         │
│  #64748B  TextTertiary        (Hints, captions)        │
│  #475569  TextDisabled        (Disabled text)          │
└────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│  Accent (Teal)                                          │
├────────────────────────────────────────────────────────┤
│  #2DD4BF  Accent              (Primary accent)         │
│  #5EEAD4  AccentHover         (Hover state)            │
│  #14B8A6  AccentPressed       (Pressed state)          │
└────────────────────────────────────────────────────────┘
```

---

## Common Control Styles

### Button Style

```xml
<Style x:Key="WidgetButton" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource SurfaceElevated}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
    <Setter Property="BorderBrush" Value="{StaticResource Border}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="12,6"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background" 
                                Value="{StaticResource SurfaceHover}"/>
                        <Setter TargetName="border" Property="BorderBrush" 
                                Value="{StaticResource BorderHover}"/>
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="border" Property="Background" 
                                Value="{StaticResource Accent}"/>
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter Property="Opacity" Value="0.5"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### TextBox Style

```xml
<Style x:Key="WidgetTextBox" TargetType="TextBox">
    <Setter Property="Background" Value="{StaticResource SurfaceElevated}"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
    <Setter Property="BorderBrush" Value="{StaticResource Border}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="8,6"/>
    <Setter Property="CaretBrush" Value="{StaticResource Accent}"/>
    <Setter Property="SelectionBrush" Value="{StaticResource Accent}"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="TextBox">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <ScrollViewer x:Name="PART_ContentHost" 
                                  Margin="{TemplateBinding Padding}"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="BorderBrush" 
                                Value="{StaticResource BorderHover}"/>
                    </Trigger>
                    <Trigger Property="IsFocused" Value="True">
                        <Setter TargetName="border" Property="BorderBrush" 
                                Value="{StaticResource BorderFocus}"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### ScrollBar Style

```xml
<Style x:Key="WidgetScrollBar" TargetType="ScrollBar">
    <Setter Property="Width" Value="8"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ScrollBar">
                <Grid>
                    <Track x:Name="PART_Track" IsDirectionReversed="True">
                        <Track.Thumb>
                            <Thumb>
                                <Thumb.Template>
                                    <ControlTemplate TargetType="Thumb">
                                        <Border Background="{StaticResource Border}" 
                                                CornerRadius="4" 
                                                Margin="2"/>
                                    </ControlTemplate>
                                </Thumb.Template>
                            </Thumb>
                        </Track.Thumb>
                    </Track>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

---

## Widget Layout Patterns

### Basic Card Widget

```xml
<Border Background="{StaticResource WidgetSurface}"
        BorderBrush="{StaticResource Border}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="16">
    
    <!-- Header -->
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Title Bar -->
        <DockPanel Grid.Row="0" Margin="0,0,0,12">
            <TextBlock Text="{Binding DisplayName}" 
                       Foreground="{StaticResource TextPrimary}"
                       FontSize="14" 
                       FontWeight="SemiBold"
                       DockPanel.Dock="Left"/>
            
            <StackPanel Orientation="Horizontal" 
                        HorizontalAlignment="Right">
                <Button Style="{StaticResource IconButton}"
                        Command="{Binding SettingsCommand}"
                        Content="⚙"/>
                <Button Style="{StaticResource IconButton}"
                        Command="{Binding CloseCommand}"
                        Content="✕"/>
            </StackPanel>
        </DockPanel>
        
        <!-- Content Area -->
        <ContentPresenter Grid.Row="1" 
                          Content="{Binding Content}"/>
        
        <!-- Footer -->
        <TextBlock Grid.Row="2" 
                   Text="{Binding StatusText}"
                   Foreground="{StaticResource TextTertiary}"
                   FontSize="10"
                   Margin="0,12,0,0"/>
    </Grid>
</Border>
```

### Icon Button Style

```xml
<Style x:Key="IconButton" TargetType="Button">
    <Setter Property="Width" Value="24"/>
    <Setter Property="Height" Value="24"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Foreground" Value="{StaticResource TextSecondary}"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        CornerRadius="4">
                    <ContentPresenter HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background" 
                                Value="{StaticResource SurfaceHover}"/>
                        <Setter Property="Foreground" 
                                Value="{StaticResource TextPrimary}"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

---

## Typography

### Font Sizes

```xml
<!-- Title -->
<TextBlock FontSize="18" FontWeight="SemiBold"/>

<!-- Heading -->
<TextBlock FontSize="14" FontWeight="SemiBold"/>

<!-- Body -->
<TextBlock FontSize="12" FontWeight="Normal"/>

<!-- Caption -->
<TextBlock FontSize="10" FontWeight="Normal"/>
```

### Text Styles

```xml
<Style x:Key="TitleText" TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
    <Setter Property="FontSize" Value="18"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
</Style>

<Style x:Key="HeadingText" TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
</Style>

<Style x:Key="BodyText" TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
    <Setter Property="FontSize" Value="12"/>
</Style>

<Style x:Key="SecondaryText" TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource TextSecondary}"/>
    <Setter Property="FontSize" Value="12"/>
</Style>

<Style x:Key="CaptionText" TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource TextTertiary}"/>
    <Setter Property="FontSize" Value="10"/>
</Style>
```

---

## Spacing & Layout

### Standard Spacing

```xml
<!-- Margins (multiples of 4) -->
<Thickness x:Key="SpacingXS">4</Thickness>
<Thickness x:Key="SpacingSM">8</Thickness>
<Thickness x:Key="SpacingMD">12</Thickness>
<Thickness x:Key="SpacingLG">16</Thickness>
<Thickness x:Key="SpacingXL">24</Thickness>

<!-- Corner Radius -->
<CornerRadius x:Key="RadiusSM">4</CornerRadius>
<CornerRadius x:Key="RadiusMD">8</CornerRadius>
<CornerRadius x:Key="RadiusLG">12</CornerRadius>
<CornerRadius x:Key="RadiusRound">9999</CornerRadius>
```

### Grid Patterns

```xml
<!-- Two-column layout -->
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
</Grid>

<!-- Header-content-footer -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>   <!-- Header -->
        <RowDefinition Height="*"/>      <!-- Content -->
        <RowDefinition Height="Auto"/>   <!-- Footer -->
    </Grid.RowDefinitions>
</Grid>
```

---

## Resource Dictionary Organization

### Per-Widget Resources

```xml
<!-- In Window.Resources or App.xaml -->
<Window.Resources>
    <ResourceDictionary>
        <!-- Merge base styles -->
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/Colors.xaml"/>
            <ResourceDictionary Source="Themes/Controls.xaml"/>
        </ResourceDictionary.MergedDictionaries>
        
        <!-- Widget-specific resources -->
        <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
    </ResourceDictionary>
</Window.Resources>
```

### Separate Resource Files

```
Themes/
├── Colors.xaml      # Color brushes
├── Controls.xaml    # Control styles
├── Typography.xaml  # Text styles
└── Converters.xaml  # Value converters
```

---

## Common Mistakes

### ❌ Using DynamicResource

```xml
<!-- WRONG for external widgets -->
<Border Background="{DynamicResource Brushes.Background}"/>

<!-- CORRECT -->
<Border Background="{StaticResource WidgetBackground}"/>
```

### ❌ Missing Window Properties

```xml
<!-- WRONG - window has borders and taskbar button -->
<Window>
    <Border Background="#0A0A0F"/>
</Window>

<!-- CORRECT -->
<Window WindowStyle="None" 
        AllowsTransparency="True" 
        Background="Transparent"
        ShowInTaskbar="False">
    <Border Background="#0A0A0F" CornerRadius="8"/>
</Window>
```

### ❌ Hard-Coded Colors

```xml
<!-- Bad - hard to maintain -->
<Border Background="#0A0A0F"/>

<!-- Good - use resources -->
<Border Background="{StaticResource WidgetBackground}"/>
```

---

## Related Skills

- [mvvm-patterns.md](mvvm-patterns.md) - MVVM implementation
- [drag-resize.md](drag-resize.md) - Drag behavior
- [animations.md](animations.md) - Animations
- [context-menus.md](context-menus.md) - Context menus

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Complete theme documentation |
| 1.0.0 | 2025-06-01 | Initial version |
