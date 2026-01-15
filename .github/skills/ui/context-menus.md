# Context Menus

> **Category:** UI | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers implementing right-click context menus for 3SC widgets.

## Prerequisites

- [xaml-styling.md](xaml-styling.md)
- [mvvm-patterns.md](mvvm-patterns.md)

---

## Basic Context Menu

### XAML Definition

```xml
<Border x:Name="WidgetContainer">
    <Border.ContextMenu>
        <ContextMenu Style="{StaticResource WidgetContextMenu}">
            
            <!-- Settings -->
            <MenuItem Header="Settings" 
                      Command="{Binding OpenSettingsCommand}">
                <MenuItem.Icon>
                    <TextBlock Text="⚙" FontSize="14"/>
                </MenuItem.Icon>
            </MenuItem>
            
            <Separator/>
            
            <!-- Position/Size -->
            <MenuItem Header="Lock Position" 
                      IsCheckable="True"
                      IsChecked="{Binding IsLocked}"
                      Command="{Binding ToggleLockCommand}"/>
            
            <MenuItem Header="Reset Size" 
                      Command="{Binding ResetSizeCommand}"/>
            
            <Separator/>
            
            <!-- Widget Actions -->
            <MenuItem Header="Refresh" 
                      Command="{Binding RefreshCommand}"
                      InputGestureText="F5"/>
            
            <Separator/>
            
            <!-- Close -->
            <MenuItem Header="Close Widget" 
                      Command="{Binding CloseCommand}">
                <MenuItem.Icon>
                    <TextBlock Text="✕" FontSize="14"/>
                </MenuItem.Icon>
            </MenuItem>
            
        </ContextMenu>
    </Border.ContextMenu>
</Border>
```

---

## Context Menu Styling

### Dark Theme Style

```xml
<Window.Resources>
    <!-- Context Menu Style -->
    <Style x:Key="WidgetContextMenu" TargetType="ContextMenu">
        <Setter Property="Background" Value="{StaticResource SurfaceElevated}"/>
        <Setter Property="BorderBrush" Value="{StaticResource Border}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="4"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ContextMenu">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="8"
                            Padding="{TemplateBinding Padding}">
                        <Border.Effect>
                            <DropShadowEffect ShadowDepth="4" 
                                              Opacity="0.3" 
                                              BlurRadius="8"/>
                        </Border.Effect>
                        <StackPanel IsItemsHost="True"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- MenuItem Style -->
    <Style TargetType="MenuItem">
        <Setter Property="Foreground" Value="{StaticResource TextPrimary}"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Padding" Value="12,8"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="MenuItem">
                    <Border x:Name="Border"
                            Background="{TemplateBinding Background}"
                            CornerRadius="4"
                            Padding="{TemplateBinding Padding}">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="20"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <!-- Icon/Checkmark -->
                            <ContentPresenter Grid.Column="0"
                                              ContentSource="Icon"
                                              VerticalAlignment="Center"/>
                            
                            <!-- Header -->
                            <ContentPresenter Grid.Column="1"
                                              ContentSource="Header"
                                              VerticalAlignment="Center"
                                              Margin="8,0"/>
                            
                            <!-- Shortcut -->
                            <TextBlock Grid.Column="2"
                                       Text="{TemplateBinding InputGestureText}"
                                       Foreground="{StaticResource TextTertiary}"
                                       FontSize="11"
                                       VerticalAlignment="Center"/>
                        </Grid>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Border" Property="Background" 
                                    Value="{StaticResource SurfaceHover}"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Foreground" 
                                    Value="{StaticResource TextDisabled}"/>
                        </Trigger>
                        <Trigger Property="IsChecked" Value="True">
                            <Setter Property="Icon">
                                <Setter.Value>
                                    <TextBlock Text="✓" 
                                               Foreground="{StaticResource Accent}"
                                               FontWeight="Bold"/>
                                </Setter.Value>
                            </Setter>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Separator Style -->
    <Style TargetType="Separator">
        <Setter Property="Height" Value="1"/>
        <Setter Property="Margin" Value="8,4"/>
        <Setter Property="Background" Value="{StaticResource Border}"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Separator">
                    <Rectangle Fill="{TemplateBinding Background}"
                               Height="{TemplateBinding Height}"/>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</Window.Resources>
```

---

## ViewModel Commands

```csharp
public partial class MyWidgetViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLocked;
    
    [RelayCommand]
    private void OpenSettings()
    {
        // Open settings dialog or navigate to settings
        Log.Debug("Opening settings");
    }
    
    [RelayCommand]
    private void ToggleLock()
    {
        IsLocked = !IsLocked;
        SaveSettings();
        Log.Debug("Widget lock: {IsLocked}", IsLocked);
    }
    
    [RelayCommand]
    private void ResetSize()
    {
        // Reset to default size
        ResetSizeRequested?.Invoke(this, new SizeEventArgs(300, 200));
    }
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        // Refresh widget data
        await LoadDataAsync();
    }
    
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler<SizeEventArgs>? ResetSizeRequested;
    public event EventHandler? CloseRequested;
}
```

---

## Dynamic Context Menus

### Conditional Menu Items

```xml
<ContextMenu Style="{StaticResource WidgetContextMenu}">
    
    <!-- Always visible -->
    <MenuItem Header="Settings" Command="{Binding OpenSettingsCommand}"/>
    
    <!-- Only visible when data is loaded -->
    <MenuItem Header="Export Data" 
              Command="{Binding ExportCommand}"
              Visibility="{Binding HasData, 
                          Converter={StaticResource BoolToVisibility}}"/>
    
    <!-- Different text based on state -->
    <MenuItem Command="{Binding TogglePlayCommand}">
        <MenuItem.Style>
            <Style TargetType="MenuItem" BasedOn="{StaticResource {x:Type MenuItem}}">
                <Setter Property="Header" Value="Play"/>
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsPlaying}" Value="True">
                        <Setter Property="Header" Value="Pause"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </MenuItem.Style>
    </MenuItem>
    
</ContextMenu>
```

### Submenu

```xml
<MenuItem Header="Size">
    <MenuItem Header="Small" Command="{Binding SetSizeCommand}" CommandParameter="Small"/>
    <MenuItem Header="Medium" Command="{Binding SetSizeCommand}" CommandParameter="Medium"/>
    <MenuItem Header="Large" Command="{Binding SetSizeCommand}" CommandParameter="Large"/>
</MenuItem>
```

---

## Code-Behind Context Menu

```csharp
public partial class MyWidgetWindow : Window
{
    public MyWidgetWindow()
    {
        InitializeComponent();
        CreateContextMenu();
    }
    
    private void CreateContextMenu()
    {
        var contextMenu = new ContextMenu
        {
            Style = (Style)FindResource("WidgetContextMenu")
        };
        
        // Settings
        var settingsItem = new MenuItem { Header = "Settings" };
        settingsItem.Click += (s, e) => OpenSettings();
        contextMenu.Items.Add(settingsItem);
        
        contextMenu.Items.Add(new Separator());
        
        // Lock
        var lockItem = new MenuItem 
        { 
            Header = "Lock Position",
            IsCheckable = true
        };
        lockItem.Click += (s, e) => ToggleLock(lockItem);
        contextMenu.Items.Add(lockItem);
        
        contextMenu.Items.Add(new Separator());
        
        // Close
        var closeItem = new MenuItem { Header = "Close Widget" };
        closeItem.Click += (s, e) => Close();
        contextMenu.Items.Add(closeItem);
        
        WidgetContainer.ContextMenu = contextMenu;
    }
    
    private void OpenSettings()
    {
        // Implementation
    }
    
    private void ToggleLock(MenuItem item)
    {
        item.IsChecked = !item.IsChecked;
        // Save state
    }
}
```

---

## Context Menu with Icons

### Using Unicode Icons

```xml
<MenuItem Header="Settings">
    <MenuItem.Icon>
        <TextBlock Text="⚙" FontSize="14" 
                   Foreground="{StaticResource TextSecondary}"/>
    </MenuItem.Icon>
</MenuItem>

<MenuItem Header="Refresh">
    <MenuItem.Icon>
        <TextBlock Text="🔄" FontSize="14"/>
    </MenuItem.Icon>
</MenuItem>

<MenuItem Header="Delete">
    <MenuItem.Icon>
        <TextBlock Text="🗑" FontSize="14" 
                   Foreground="{StaticResource Error}"/>
    </MenuItem.Icon>
</MenuItem>
```

### Using Path Icons

```xml
<MenuItem Header="Settings">
    <MenuItem.Icon>
        <Path Data="M12 15.5A3.5 3.5 0 0 1 8.5 12 3.5 3.5 0 0 1 12 8.5a3.5 3.5 0 0 1 3.5 3.5 3.5 3.5 0 0 1-3.5 3.5m7.43-2.53c.04-.32.07-.64.07-.97 0-.33-.03-.66-.07-1l2.11-1.63c.19-.15.24-.42.12-.64l-2-3.46c-.12-.22-.39-.31-.61-.22l-2.49 1c-.52-.39-1.06-.73-1.69-.98l-.37-2.65A.506.506 0 0 0 14 2h-4c-.25 0-.46.18-.5.42l-.37 2.65c-.63.25-1.17.59-1.69.98l-2.49-1c-.22-.09-.49 0-.61.22l-2 3.46c-.13.22-.07.49.12.64L4.57 11c-.04.34-.07.67-.07 1 0 .33.03.65.07.97l-2.11 1.66c-.19.15-.25.42-.12.64l2 3.46c.12.22.39.3.61.22l2.49-1.01c.52.4 1.06.74 1.69.99l.37 2.65c.04.24.25.42.5.42h4c.25 0 .46-.18.5-.42l.37-2.65c.63-.26 1.17-.59 1.69-.99l2.49 1.01c.22.08.49 0 .61-.22l2-3.46c.12-.22.07-.49-.12-.64l-2.11-1.66Z"
              Fill="{StaticResource TextSecondary}"
              Width="16" Height="16"
              Stretch="Uniform"/>
    </MenuItem.Icon>
</MenuItem>
```

---

## Keyboard Shortcuts

```csharp
public partial class MyWidgetWindow : Window
{
    public MyWidgetWindow()
    {
        InitializeComponent();
        
        // Register keyboard shortcuts
        InputBindings.Add(new KeyBinding(
            new RelayCommand(() => (DataContext as MyWidgetViewModel)?.RefreshCommand.Execute(null)),
            Key.F5, ModifierKeys.None));
        
        InputBindings.Add(new KeyBinding(
            new RelayCommand(() => (DataContext as MyWidgetViewModel)?.OpenSettingsCommand.Execute(null)),
            Key.OemComma, ModifierKeys.Control));
    }
}
```

---

## Standard Widget Context Menu

```xml
<!-- Reusable context menu for all widgets -->
<ContextMenu x:Key="StandardWidgetContextMenu" 
             Style="{StaticResource WidgetContextMenu}">
    
    <!-- Settings (if widget has settings) -->
    <MenuItem Header="⚙ Settings" 
              Command="{Binding OpenSettingsCommand}"
              Visibility="{Binding HasSettings, 
                          Converter={StaticResource BoolToVisibility}}"/>
    
    <Separator Visibility="{Binding HasSettings, 
                           Converter={StaticResource BoolToVisibility}}"/>
    
    <!-- Position Controls -->
    <MenuItem Header="📌 Lock Position" 
              IsCheckable="True"
              IsChecked="{Binding IsLocked}"/>
    
    <MenuItem Header="↩ Reset Position" 
              Command="{Binding ResetPositionCommand}"/>
    
    <MenuItem Header="📐 Reset Size" 
              Command="{Binding ResetSizeCommand}"/>
    
    <Separator/>
    
    <!-- Actions -->
    <MenuItem Header="🔄 Refresh" 
              Command="{Binding RefreshCommand}"
              InputGestureText="F5"
              IsEnabled="{Binding CanRefresh}"/>
    
    <Separator/>
    
    <!-- About -->
    <MenuItem Header="ℹ About" 
              Command="{Binding ShowAboutCommand}"/>
    
    <Separator/>
    
    <!-- Close -->
    <MenuItem Header="✕ Close Widget" 
              Command="{Binding CloseCommand}"/>
    
</ContextMenu>
```

---

## Best Practices

1. **Consistent ordering** - Settings first, Close last
2. **Use separators** to group related items
3. **Show keyboard shortcuts** where applicable
4. **Disable unavailable items** instead of hiding
5. **Use icons sparingly** for important actions

---

## Related Skills

- [xaml-styling.md](xaml-styling.md) - XAML patterns
- [mvvm-patterns.md](mvvm-patterns.md) - Commands

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Complete styling examples |
| 1.0.0 | 2025-06-01 | Initial version |
