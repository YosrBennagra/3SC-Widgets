# Settings Window Template

> **Category:** Templates | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Complete template for a widget settings window with proper styling and MVVM pattern.

---

## SettingsWindow.xaml

```xml
<Window x:Class="_3SC.Widgets.MyWidget.Views.SettingsWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        Title="Widget Settings"
        Width="400" Height="350"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize"
        ShowInTaskbar="False"
        Background="#1A1A2E">
    
    <Window.Resources>
        <!-- Colors -->
        <Color x:Key="BackgroundColor">#1A1A2E</Color>
        <Color x:Key="SurfaceColor">#25253A</Color>
        <Color x:Key="TextColor">#E8E8E8</Color>
        <Color x:Key="TextSecondaryColor">#A0A0A0</Color>
        <Color x:Key="AccentColor">#4A9EFF</Color>
        <Color x:Key="BorderColor">#3A3A5A</Color>
        
        <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
        <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
        <SolidColorBrush x:Key="TextBrush" Color="{StaticResource TextColor}"/>
        <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
        <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
        <SolidColorBrush x:Key="BorderBrush" Color="{StaticResource BorderColor}"/>
        
        <!-- Label Style -->
        <Style x:Key="LabelStyle" TargetType="TextBlock">
            <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="Margin" Value="0,0,0,5"/>
        </Style>
        
        <!-- Description Style -->
        <Style x:Key="DescriptionStyle" TargetType="TextBlock">
            <Setter Property="Foreground" Value="{StaticResource TextSecondaryBrush}"/>
            <Setter Property="FontSize" Value="11"/>
            <Setter Property="Margin" Value="0,2,0,15"/>
            <Setter Property="TextWrapping" Value="Wrap"/>
        </Style>
        
        <!-- TextBox Style -->
        <Style x:Key="SettingsTextBoxStyle" TargetType="TextBox">
            <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
            <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
            <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Padding" Value="8,6"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="CaretBrush" Value="{StaticResource TextBrush}"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="TextBox">
                        <Border Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                CornerRadius="4">
                            <ScrollViewer x:Name="PART_ContentHost" Margin="{TemplateBinding Padding}"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        
        <!-- ComboBox Style -->
        <Style x:Key="SettingsComboBoxStyle" TargetType="ComboBox">
            <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
            <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
            <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Padding" Value="8,6"/>
            <Setter Property="FontSize" Value="13"/>
        </Style>
        
        <!-- CheckBox Style -->
        <Style x:Key="SettingsCheckBoxStyle" TargetType="CheckBox">
            <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="Margin" Value="0,0,0,10"/>
        </Style>
        
        <!-- Button Style -->
        <Style x:Key="PrimaryButtonStyle" TargetType="Button">
            <Setter Property="Background" Value="{StaticResource AccentBrush}"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Padding" Value="20,8"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}"
                                CornerRadius="4"
                                Padding="{TemplateBinding Padding}">
                            <ContentPresenter HorizontalAlignment="Center" 
                                              VerticalAlignment="Center"/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Background" Value="#5AAEFF"/>
                            </Trigger>
                            <Trigger Property="IsPressed" Value="True">
                                <Setter Property="Background" Value="#3A8EDF"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        
        <Style x:Key="SecondaryButtonStyle" TargetType="Button">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
            <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Padding" Value="20,8"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                CornerRadius="4"
                                Padding="{TemplateBinding Padding}">
                            <ContentPresenter HorizontalAlignment="Center" 
                                              VerticalAlignment="Center"/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Window.Resources>
    
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <TextBlock Grid.Row="0"
                   Text="Settings"
                   FontSize="20"
                   FontWeight="SemiBold"
                   Foreground="{StaticResource TextBrush}"
                   Margin="0,0,0,20"/>
        
        <!-- Settings Content -->
        <ScrollViewer Grid.Row="1"
                      VerticalScrollBarVisibility="Auto"
                      Margin="0,0,0,20">
            <StackPanel>
                
                <!-- Refresh Interval -->
                <TextBlock Text="Refresh Interval" 
                           Style="{StaticResource LabelStyle}"/>
                <ComboBox x:Name="RefreshIntervalComboBox"
                          Style="{StaticResource SettingsComboBoxStyle}"
                          Margin="0,0,0,5">
                    <ComboBoxItem Content="30 seconds" Tag="30"/>
                    <ComboBoxItem Content="1 minute" Tag="60" IsSelected="True"/>
                    <ComboBoxItem Content="5 minutes" Tag="300"/>
                    <ComboBoxItem Content="15 minutes" Tag="900"/>
                    <ComboBoxItem Content="30 minutes" Tag="1800"/>
                </ComboBox>
                <TextBlock Text="How often the widget updates its data"
                           Style="{StaticResource DescriptionStyle}"/>
                
                <!-- Theme -->
                <TextBlock Text="Theme" 
                           Style="{StaticResource LabelStyle}"/>
                <ComboBox x:Name="ThemeComboBox"
                          Style="{StaticResource SettingsComboBoxStyle}"
                          Margin="0,0,0,5">
                    <ComboBoxItem Content="Dark" Tag="dark" IsSelected="True"/>
                    <ComboBoxItem Content="Light" Tag="light"/>
                    <ComboBoxItem Content="System" Tag="system"/>
                </ComboBox>
                <TextBlock Text="Visual appearance of the widget"
                           Style="{StaticResource DescriptionStyle}"/>
                
                <!-- Show Title -->
                <CheckBox x:Name="ShowTitleCheckBox"
                          Content="Show widget title"
                          Style="{StaticResource SettingsCheckBoxStyle}"
                          IsChecked="True"/>
                
                <!-- Custom Text Setting -->
                <TextBlock Text="Custom Label" 
                           Style="{StaticResource LabelStyle}"/>
                <TextBox x:Name="CustomLabelTextBox"
                         Style="{StaticResource SettingsTextBoxStyle}"
                         Margin="0,0,0,5"
                         Text="My Widget"/>
                <TextBlock Text="Custom text displayed in the widget header"
                           Style="{StaticResource DescriptionStyle}"/>
                
            </StackPanel>
        </ScrollViewer>
        
        <!-- Buttons -->
        <StackPanel Grid.Row="2"
                    Orientation="Horizontal"
                    HorizontalAlignment="Right">
            <Button Content="Cancel"
                    Style="{StaticResource SecondaryButtonStyle}"
                    Margin="0,0,10,0"
                    Click="CancelButton_Click"/>
            <Button Content="Save"
                    Style="{StaticResource PrimaryButtonStyle}"
                    Click="SaveButton_Click"/>
        </StackPanel>
    </Grid>
</Window>
```

---

## SettingsWindow.xaml.cs

```csharp
using System.Windows;
using System.Windows.Controls;
using _3SC.Widgets.MyWidget.Models;

namespace _3SC.Widgets.MyWidget.Views;

public partial class SettingsWindow : Window
{
    public WidgetSettings Settings { get; private set; }
    
    public SettingsWindow(WidgetSettings settings)
    {
        InitializeComponent();
        Settings = settings;
        LoadSettingsToUI();
    }
    
    private void LoadSettingsToUI()
    {
        // Refresh Interval
        foreach (ComboBoxItem item in RefreshIntervalComboBox.Items)
        {
            if (int.TryParse(item.Tag?.ToString(), out var value) && 
                value == Settings.RefreshIntervalSeconds)
            {
                RefreshIntervalComboBox.SelectedItem = item;
                break;
            }
        }
        
        // Theme
        foreach (ComboBoxItem item in ThemeComboBox.Items)
        {
            if (item.Tag?.ToString() == Settings.Theme)
            {
                ThemeComboBox.SelectedItem = item;
                break;
            }
        }
        
        // Show Title
        ShowTitleCheckBox.IsChecked = Settings.ShowTitle;
        
        // Custom Label (if you add this to WidgetSettings)
        // CustomLabelTextBox.Text = Settings.CustomLabel;
    }
    
    private void SaveSettingsFromUI()
    {
        // Refresh Interval
        if (RefreshIntervalComboBox.SelectedItem is ComboBoxItem refreshItem &&
            int.TryParse(refreshItem.Tag?.ToString(), out var refreshValue))
        {
            Settings.RefreshIntervalSeconds = refreshValue;
        }
        
        // Theme
        if (ThemeComboBox.SelectedItem is ComboBoxItem themeItem)
        {
            Settings.Theme = themeItem.Tag?.ToString() ?? "dark";
        }
        
        // Show Title
        Settings.ShowTitle = ShowTitleCheckBox.IsChecked ?? true;
        
        // Custom Label
        // Settings.CustomLabel = CustomLabelTextBox.Text;
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSettingsFromUI();
        DialogResult = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
```

---

## Usage in Widget

```csharp
public void ShowSettings()
{
    // Clone settings so cancel works correctly
    var settingsClone = _viewModel.Settings.Clone();
    
    var settingsWindow = new SettingsWindow(settingsClone);
    settingsWindow.Owner = Window.GetWindow(_view);
    
    if (settingsWindow.ShowDialog() == true)
    {
        // User clicked Save
        _viewModel.ApplySettings(settingsWindow.Settings);
        _viewModel.SaveSettings();
    }
    // If DialogResult is false or null, settings weren't changed
}
```

---

## Adding New Settings

1. Add property to `WidgetSettings.cs`
2. Add UI element to `SettingsWindow.xaml`
3. Update `LoadSettingsToUI()` to populate the control
4. Update `SaveSettingsFromUI()` to read the value
5. Handle the setting in `MainViewModel.ApplySettings()`

---

## Related Skills

- [settings-management.md](../data/settings-management.md) - Settings patterns
- [new-widget-template.md](new-widget-template.md) - Full widget template

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added styled controls |
| 1.0.0 | 2025-06-01 | Initial version |
