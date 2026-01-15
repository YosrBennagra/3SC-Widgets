# Accessibility

> **Category:** Advanced | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Make widgets accessible to all users including those using screen readers, keyboard navigation, and high contrast modes.

---

## Automation Properties

```xml
<!-- Set accessible names for screen readers -->
<Button Content="Save" 
        AutomationProperties.Name="Save settings"
        AutomationProperties.HelpText="Saves current settings to disk"/>

<!-- For icon-only buttons -->
<Button AutomationProperties.Name="Close widget">
    <TextBlock Text="&#xE711;" FontFamily="Segoe MDL2 Assets"/>
</Button>

<!-- For images -->
<Image Source="icon.png" 
       AutomationProperties.Name="Weather icon showing sunny conditions"/>

<!-- For complex controls -->
<ListBox AutomationProperties.Name="Available items"
         AutomationProperties.LabeledBy="{Binding ElementName=ItemsLabel}">
    <!-- items -->
</ListBox>
```

---

## Keyboard Navigation

```xml
<!-- Tab order -->
<StackPanel>
    <TextBox TabIndex="0" AutomationProperties.Name="Search"/>
    <Button TabIndex="1" Content="Search"/>
    <ListBox TabIndex="2"/>
</StackPanel>

<!-- Focus visual style -->
<Style TargetType="Button">
    <Setter Property="FocusVisualStyle">
        <Setter.Value>
            <Style>
                <Setter Property="Control.Template">
                    <Setter.Value>
                        <ControlTemplate>
                            <Border BorderBrush="#4A9EFF" 
                                    BorderThickness="2" 
                                    CornerRadius="4"
                                    Margin="-2"/>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

---

## Keyboard Shortcuts

```csharp
public partial class MainWidget : UserControl
{
    public MainWidget()
    {
        InitializeComponent();
        
        // Register keyboard shortcuts
        InputBindings.Add(new KeyBinding(
            new RelayCommand(Refresh),
            Key.F5, ModifierKeys.None));
        
        InputBindings.Add(new KeyBinding(
            new RelayCommand(OpenSettings),
            Key.S, ModifierKeys.Control));
        
        InputBindings.Add(new KeyBinding(
            new RelayCommand(CloseWidget),
            Key.Escape, ModifierKeys.None));
    }
}
```

```xml
<!-- XAML keyboard bindings -->
<UserControl.InputBindings>
    <KeyBinding Key="F5" Command="{Binding RefreshCommand}"/>
    <KeyBinding Key="S" Modifiers="Ctrl" Command="{Binding SaveCommand}"/>
    <KeyBinding Key="Escape" Command="{Binding CloseCommand}"/>
</UserControl.InputBindings>
```

---

## High Contrast Support

```csharp
public static class HighContrastHelper
{
    public static bool IsHighContrast => 
        SystemParameters.HighContrast;
    
    public static void ApplyHighContrastStyles(FrameworkElement element)
    {
        if (IsHighContrast)
        {
            // Use system colors in high contrast mode
            element.Resources["BackgroundColor"] = SystemColors.WindowBrush;
            element.Resources["ForegroundColor"] = SystemColors.WindowTextBrush;
            element.Resources["AccentColor"] = SystemColors.HighlightBrush;
            element.Resources["BorderColor"] = SystemColors.ActiveBorderBrush;
        }
    }
}
```

```xml
<!-- High contrast aware styles -->
<Style x:Key="AccessibleButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="#2D2D3D"/>
    <Setter Property="Foreground" Value="#E8E8E8"/>
    <Setter Property="BorderBrush" Value="#404050"/>
    <Style.Triggers>
        <DataTrigger Binding="{x:Static SystemParameters.HighContrast}" Value="True">
            <Setter Property="Background" Value="{DynamicResource {x:Static SystemColors.ControlBrushKey}}"/>
            <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}"/>
            <Setter Property="BorderBrush" Value="{DynamicResource {x:Static SystemColors.ActiveBorderBrushKey}}"/>
        </DataTrigger>
    </Style.Triggers>
</Style>
```

---

## Screen Reader Announcements

```csharp
public static class ScreenReaderHelper
{
    public static void Announce(string message, DependencyObject element)
    {
        AutomationPeer peer = UIElementAutomationPeer.FromElement(element as UIElement);
        peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        
        // Set the announcement
        AutomationProperties.SetLiveSetting(element, AutomationLiveSetting.Polite);
        AutomationProperties.SetName(element, message);
    }
}
```

```xml
<!-- Live region for dynamic updates -->
<TextBlock x:Name="StatusText"
           AutomationProperties.LiveSetting="Polite"
           Text="{Binding StatusMessage}"/>
```

---

## Accessible Colors

```csharp
public static class AccessibilityColors
{
    // Minimum contrast ratio: 4.5:1 for normal text, 3:1 for large text
    
    // High contrast pairs (dark background)
    public static readonly Color Background = Color.FromRgb(0x1A, 0x1A, 0x2E); // #1A1A2E
    public static readonly Color TextPrimary = Color.FromRgb(0xE8, 0xE8, 0xE8); // #E8E8E8 (13.5:1)
    public static readonly Color TextSecondary = Color.FromRgb(0xB8, 0xB8, 0xB8); // #B8B8B8 (8.9:1)
    public static readonly Color Accent = Color.FromRgb(0x4A, 0x9E, 0xFF); // #4A9EFF (5.2:1)
    public static readonly Color Error = Color.FromRgb(0xE7, 0x6F, 0x51); // #E76F51 (4.6:1)
    public static readonly Color Success = Color.FromRgb(0x52, 0xB7, 0x88); // #52B788 (6.8:1)
    
    public static double CalculateContrastRatio(Color foreground, Color background)
    {
        var l1 = GetRelativeLuminance(foreground);
        var l2 = GetRelativeLuminance(background);
        var lighter = Math.Max(l1, l2);
        var darker = Math.Min(l1, l2);
        return (lighter + 0.05) / (darker + 0.05);
    }
    
    private static double GetRelativeLuminance(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;
        
        r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);
        
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
}
```

---

## Focus Management

```csharp
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        
        // Set initial focus
        Loaded += (s, e) => FirstInput.Focus();
    }
    
    // Trap focus in dialog
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            var focused = Keyboard.FocusedElement as FrameworkElement;
            
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && focused == FirstInput)
            {
                LastButton.Focus();
                e.Handled = true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.None && focused == LastButton)
            {
                FirstInput.Focus();
                e.Handled = true;
            }
        }
        
        base.OnPreviewKeyDown(e);
    }
}
```

---

## Accessible Tooltips

```xml
<!-- Informative tooltips -->
<Button Content="&#xE74E;">
    <Button.ToolTip>
        <ToolTip>
            <StackPanel MaxWidth="200">
                <TextBlock FontWeight="Bold" Text="Refresh Data"/>
                <TextBlock TextWrapping="Wrap" 
                           Text="Reload all data from source. Keyboard shortcut: F5"/>
            </StackPanel>
        </ToolTip>
    </Button.ToolTip>
</Button>
```

---

## Form Labels

```xml
<!-- Proper label association -->
<StackPanel>
    <Label x:Name="NameLabel" 
           Content="Name:" 
           Target="{Binding ElementName=NameInput}"/>
    <TextBox x:Name="NameInput"
             AutomationProperties.LabeledBy="{Binding ElementName=NameLabel}"/>
</StackPanel>

<!-- Required field indicator -->
<StackPanel Orientation="Horizontal">
    <Label Content="Email:"/>
    <TextBlock Text="*" Foreground="#E76F51" 
               AutomationProperties.Name="Required field"/>
</StackPanel>
```

---

## Error Messages

```xml
<!-- Accessible error display -->
<StackPanel>
    <TextBox x:Name="EmailInput"
             AutomationProperties.Name="Email address"
             AutomationProperties.HelpText="{Binding EmailError}"/>
    
    <TextBlock Text="{Binding EmailError}"
               Foreground="#E76F51"
               Visibility="{Binding HasEmailError, Converter={StaticResource BoolToVis}}"
               AutomationProperties.LiveSetting="Assertive"/>
</StackPanel>
```

---

## Accessibility Checklist

- [ ] All interactive elements have accessible names
- [ ] Tab order is logical
- [ ] Focus is visible on all focusable elements
- [ ] Keyboard shortcuts exist for common actions
- [ ] Color contrast meets WCAG 4.5:1 minimum
- [ ] Information isn't conveyed by color alone
- [ ] Form fields have associated labels
- [ ] Error messages are announced to screen readers
- [ ] High contrast mode is supported
- [ ] Tooltips provide helpful information

---

## Best Practices

1. **Label everything** - Screen readers need text descriptions
2. **Test with keyboard only** - All features must be keyboard accessible
3. **Use semantic elements** - Proper roles for controls
4. **High contrast colors** - Minimum 4.5:1 contrast ratio
5. **Don't rely on color alone** - Use icons or text too

---

## Related Skills

- [xaml-styling.md](../ui/xaml-styling.md) - Theme colors
- [context-menus.md](../ui/context-menus.md) - Accessible menus

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added high contrast support |
| 1.0.0 | 2025-06-01 | Initial version |
