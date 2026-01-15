# Notifications & Toasts

> **Category:** Advanced | **Priority:** 🟢 Optional
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Implement in-widget notifications and toast messages for user feedback without disrupting the widget experience.

---

## Toast Notification Model

```csharp
public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastMessage
{
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public ToastType Type { get; init; } = ToastType.Info;
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(3);
    public Action? OnClick { get; init; }
    public bool CanDismiss { get; init; } = true;
}
```

---

## Toast Service

```csharp
public class ToastService : ObservableObject
{
    private readonly ObservableCollection<ToastViewModel> _toasts = new();
    private readonly ILogger _log;
    
    public ReadOnlyObservableCollection<ToastViewModel> Toasts { get; }
    
    public ToastService(ILogger log)
    {
        _log = log;
        Toasts = new ReadOnlyObservableCollection<ToastViewModel>(_toasts);
    }
    
    public void Show(ToastMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = new ToastViewModel(message, () => Dismiss(message));
            _toasts.Add(vm);
            
            _log.Debug("Toast shown: {Title}", message.Title);
            
            // Auto dismiss
            if (message.Duration > TimeSpan.Zero)
            {
                Task.Delay(message.Duration).ContinueWith(_ => 
                    Dismiss(message), TaskScheduler.FromCurrentSynchronizationContext());
            }
        });
    }
    
    public void Dismiss(ToastMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = _toasts.FirstOrDefault(t => t.Message == message);
            if (toast != null)
            {
                _toasts.Remove(toast);
            }
        });
    }
    
    public void Clear()
    {
        Application.Current.Dispatcher.Invoke(() => _toasts.Clear());
    }
    
    // Convenience methods
    public void Info(string message, string? title = null) =>
        Show(new ToastMessage { Title = title ?? "Info", Message = message, Type = ToastType.Info });
    
    public void Success(string message, string? title = null) =>
        Show(new ToastMessage { Title = title ?? "Success", Message = message, Type = ToastType.Success });
    
    public void Warning(string message, string? title = null) =>
        Show(new ToastMessage { Title = title ?? "Warning", Message = message, Type = ToastType.Warning });
    
    public void Error(string message, string? title = null) =>
        Show(new ToastMessage { Title = title ?? "Error", Message = message, Type = ToastType.Error, Duration = TimeSpan.FromSeconds(5) });
}

public partial class ToastViewModel : ObservableObject
{
    public ToastMessage Message { get; }
    private readonly Action _dismiss;
    
    public ToastViewModel(ToastMessage message, Action dismiss)
    {
        Message = message;
        _dismiss = dismiss;
    }
    
    [RelayCommand]
    private void Dismiss() => _dismiss();
    
    [RelayCommand]
    private void Click()
    {
        Message.OnClick?.Invoke();
        _dismiss();
    }
}
```

---

## Toast Container XAML

```xml
<!-- Add to main widget view -->
<UserControl.Resources>
    <Style x:Key="ToastContainerStyle" TargetType="ItemsControl">
        <Setter Property="ItemsPanel">
            <Setter.Value>
                <ItemsPanelTemplate>
                    <StackPanel Orientation="Vertical" />
                </ItemsPanelTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</UserControl.Resources>

<Grid>
    <!-- Main widget content -->
    <ContentPresenter Content="{Binding Content}" />
    
    <!-- Toast overlay -->
    <ItemsControl ItemsSource="{Binding Toasts}"
                  Style="{StaticResource ToastContainerStyle}"
                  VerticalAlignment="Top"
                  HorizontalAlignment="Right"
                  Margin="10">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Border Margin="0,0,0,5"
                        Padding="12,8"
                        CornerRadius="4"
                        MinWidth="200"
                        MaxWidth="300">
                    <Border.Style>
                        <Style TargetType="Border">
                            <Setter Property="Background" Value="#2D2D3D"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Message.Type}" Value="Success">
                                    <Setter Property="Background" Value="#1B4332"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Message.Type}" Value="Warning">
                                    <Setter Property="Background" Value="#5C4813"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Message.Type}" Value="Error">
                                    <Setter Property="Background" Value="#5C1313"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Border.Style>
                    
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        
                        <!-- Icon -->
                        <TextBlock Grid.Column="0" 
                                   Margin="0,0,8,0"
                                   FontFamily="Segoe MDL2 Assets"
                                   FontSize="16"
                                   VerticalAlignment="Center">
                            <TextBlock.Style>
                                <Style TargetType="TextBlock">
                                    <Setter Property="Text" Value="&#xE946;"/>
                                    <Setter Property="Foreground" Value="#4A9EFF"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding Message.Type}" Value="Success">
                                            <Setter Property="Text" Value="&#xE73E;"/>
                                            <Setter Property="Foreground" Value="#52B788"/>
                                        </DataTrigger>
                                        <DataTrigger Binding="{Binding Message.Type}" Value="Warning">
                                            <Setter Property="Text" Value="&#xE7BA;"/>
                                            <Setter Property="Foreground" Value="#E9C46A"/>
                                        </DataTrigger>
                                        <DataTrigger Binding="{Binding Message.Type}" Value="Error">
                                            <Setter Property="Text" Value="&#xEA39;"/>
                                            <Setter Property="Foreground" Value="#E76F51"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </TextBlock.Style>
                        </TextBlock>
                        
                        <!-- Content -->
                        <StackPanel Grid.Column="1" VerticalAlignment="Center">
                            <TextBlock Text="{Binding Message.Title}"
                                       Foreground="#E8E8E8"
                                       FontWeight="SemiBold"
                                       FontSize="12"/>
                            <TextBlock Text="{Binding Message.Message}"
                                       Foreground="#B8B8B8"
                                       FontSize="11"
                                       TextWrapping="Wrap"/>
                        </StackPanel>
                        
                        <!-- Dismiss -->
                        <Button Grid.Column="2"
                                Command="{Binding DismissCommand}"
                                Visibility="{Binding Message.CanDismiss, Converter={StaticResource BoolToVis}}"
                                Background="Transparent"
                                BorderThickness="0"
                                Padding="4"
                                Cursor="Hand">
                            <TextBlock Text="&#xE711;" 
                                       FontFamily="Segoe MDL2 Assets"
                                       FontSize="10"
                                       Foreground="#808080"/>
                        </Button>
                    </Grid>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Grid>
```

---

## Toast Animation

```xml
<Style x:Key="AnimatedToastStyle" TargetType="Border">
    <Setter Property="RenderTransform">
        <Setter.Value>
            <TranslateTransform X="300"/>
        </Setter.Value>
    </Setter>
    <Setter Property="Opacity" Value="0"/>
    <Style.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="0" To="1"
                                     Duration="0:0:0.2"/>
                    <DoubleAnimation Storyboard.TargetProperty="(RenderTransform).(TranslateTransform.X)"
                                     From="300" To="0"
                                     Duration="0:0:0.3">
                        <DoubleAnimation.EasingFunction>
                            <CubicEase EasingMode="EaseOut"/>
                        </DoubleAnimation.EasingFunction>
                    </DoubleAnimation>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Style.Triggers>
</Style>
```

---

## Status Bar Notifications

```csharp
public partial class StatusBarViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? _statusMessage;
    
    [ObservableProperty]
    private bool _isLoading;
    
    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
    
    private CancellationTokenSource? _clearCts;
    
    public void ShowStatus(string message, TimeSpan? duration = null)
    {
        _clearCts?.Cancel();
        StatusMessage = message;
        
        if (duration.HasValue)
        {
            _clearCts = new CancellationTokenSource();
            var token = _clearCts.Token;
            
            Task.Delay(duration.Value, token).ContinueWith(_ =>
            {
                if (!token.IsCancellationRequested)
                {
                    StatusMessage = null;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    public void ShowLoading(string message)
    {
        StatusMessage = message;
        IsLoading = true;
    }
    
    public void HideLoading()
    {
        StatusMessage = null;
        IsLoading = false;
    }
}
```

---

## Status Bar XAML

```xml
<Border Background="#16161E" 
        Padding="8,4"
        Visibility="{Binding HasStatus, Converter={StaticResource BoolToVis}}">
    <StackPanel Orientation="Horizontal">
        <!-- Loading spinner -->
        <Border Width="12" Height="12" 
                Margin="0,0,8,0"
                Visibility="{Binding IsLoading, Converter={StaticResource BoolToVis}}">
            <Border.RenderTransform>
                <RotateTransform/>
            </Border.RenderTransform>
            <Border.Style>
                <Style TargetType="Border">
                    <Style.Triggers>
                        <DataTrigger Binding="{Binding IsLoading}" Value="True">
                            <DataTrigger.EnterActions>
                                <BeginStoryboard>
                                    <Storyboard RepeatBehavior="Forever">
                                        <DoubleAnimation 
                                            Storyboard.TargetProperty="(RenderTransform).(RotateTransform.Angle)"
                                            From="0" To="360"
                                            Duration="0:0:1"/>
                                    </Storyboard>
                                </BeginStoryboard>
                            </DataTrigger.EnterActions>
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </Border.Style>
            <Ellipse Stroke="#4A9EFF" StrokeThickness="2"
                     StrokeDashArray="3,2"/>
        </Border>
        
        <TextBlock Text="{Binding StatusMessage}"
                   Foreground="#B8B8B8"
                   FontSize="11"/>
    </StackPanel>
</Border>
```

---

## Usage Examples

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly ToastService _toasts;
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            await _dataService.SaveAsync();
            _toasts.Success("Changes saved successfully");
        }
        catch (Exception ex)
        {
            _toasts.Error($"Save failed: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void ShowActionToast()
    {
        _toasts.Show(new ToastMessage
        {
            Title = "New Update",
            Message = "Version 2.0 is available",
            Type = ToastType.Info,
            Duration = TimeSpan.Zero, // Don't auto dismiss
            OnClick = () => OpenUpdateWindow()
        });
    }
}
```

---

## Best Practices

1. **Keep messages short** - Users scan quickly
2. **Use appropriate types** - Success, error, warning, info
3. **Auto-dismiss info** - Errors stay longer
4. **Position consistently** - Top-right is standard
5. **Animate smoothly** - Slide in, fade out

---

## Related Skills

- [animations.md](../ui/animations.md) - Animation techniques
- [mvvm-patterns.md](../ui/mvvm-patterns.md) - ViewModel patterns

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added status bar |
| 1.0.0 | 2025-06-01 | Initial version |
