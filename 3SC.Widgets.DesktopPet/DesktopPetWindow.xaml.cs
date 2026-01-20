using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Serilog;
using _3SC.Widgets.DesktopPet.Helpers;

// Explicit aliases to avoid conflicts with System.Windows.Forms
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using Point = System.Windows.Point;

namespace _3SC.Widgets.DesktopPet;

/// <summary>
/// Code-behind for the Desktop Pet window.
/// Handles window behavior, animations, and user interactions.
/// HIGH FPS (120Hz) for smooth animations!
/// </summary>
public partial class DesktopPetWindow : WidgetWindowBase, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DesktopPetWindow>();
    private readonly DesktopPetViewModel _viewModel;
    private readonly DispatcherTimer _renderTimer;      // 120 FPS main render loop
    private readonly DispatcherTimer _blinkTimer;
    private bool _isDisposed;

    // Animation state
    private int _frameCount;
    private int _particleFrame;
    private Point _lastMousePosition;
    private bool _mouseTracking;

    public DesktopPetWindow()
    {
        InitializeComponent();

        _viewModel = new DesktopPetViewModel();
        DataContext = _viewModel;

        // HIGH FPS render timer - 120 FPS for buttery smooth animations
        _renderTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(8.33) // ~120 FPS
        };
        _renderTimer.Tick += OnRenderTick;

        // Blink timer - random intervals
        _blinkTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _blinkTimer.Tick += OnBlinkTick;

        // Subscribe to ViewModel changes
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Initialize window position
        var init = new WidgetWindowInit(
            Guid.NewGuid(),
            _viewModel.PetX > 0 ? _viewModel.PetX : 300,
            _viewModel.PetY > 0 ? _viewModel.PetY : 300,
            180,
            180,
            false,
            true);

        var parts = new WidgetWindowParts(
            LockWidgetMenuItem: LockWidgetMenuItem,
            WidgetKey: "desktop-pet");

        InitializeWidgetWindow(init, parts);

        Log.Debug("DesktopPetWindow created with 120 FPS rendering");
    }

    protected override async Task OnWidgetLoadedAsync()
    {
        await base.OnWidgetLoadedAsync();

        _viewModel.Initialize();
        _renderTimer.Start();
        _blinkTimer.Start();

        Log.Information("Desktop Pet loaded: {Name} @ 120 FPS", _viewModel.PetName);
    }

    protected override async Task OnWidgetClosingAsync()
    {
        await base.OnWidgetClosingAsync();
        Dispose();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _renderTimer.Stop();
        _blinkTimer.Stop();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _viewModel.Dispose();

        Log.Debug("DesktopPetWindow disposed");
    }

    #region High FPS Render Loop

    private void OnRenderTick(object? sender, EventArgs e)
    {
        if (_isDisposed) return;

        _frameCount++;

        // Update eye tracking every frame for smooth following
        UpdateEyeTracking();

        // Update body animations
        UpdateBodyAnimation();

        // Update particle animations
        UpdateParticleAnimations();
    }

    private void UpdateEyeTracking()
    {
        try
        {
            // Get current mouse position in screen coordinates
            var mousePos = Win32Interop.GetMousePosition();
            _lastMousePosition = mousePos;

            // Get window position
            var windowCenter = new Point(
                Left + ActualWidth / 2,
                Top + ActualHeight / 2
            );

            // Calculate direction to mouse
            var dx = mousePos.X - windowCenter.X;
            var dy = mousePos.Y - windowCenter.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // Don't move eyes if sleeping
            if (_viewModel.IsSleeping)
            {
                SetPupilOffset(0, 2);
                return;
            }

            // Calculate eye offset (max 4 pixels)
            double eyeX = 0, eyeY = 0;
            if (distance > 20)
            {
                eyeX = Math.Clamp((dx / distance) * 4, -4, 4);
                eyeY = Math.Clamp((dy / distance) * 3, -3, 3);
            }

            // Apply smooth interpolation for fluid movement
            SetPupilOffset(eyeX, eyeY);
        }
        catch
        {
            // Ignore eye tracking errors
        }
    }

    private void SetPupilOffset(double x, double y)
    {
        // Apply offset to both pupils with smooth animation
        var leftMargin = LeftPupil.Margin;
        var rightMargin = RightPupil.Margin;

        // Smooth interpolation (lerp)
        var currentX = leftMargin.Left - 5; // Adjust for base centering
        var currentY = leftMargin.Top - 5;

        var newX = currentX + (x - currentX) * 0.3; // 30% interpolation per frame = smooth
        var newY = currentY + (y - currentY) * 0.3;

        LeftPupil.Margin = new Thickness(5 + newX, 5 + newY, 0, 0);
        RightPupil.Margin = new Thickness(5 + newX, 5 + newY, 0, 0);
    }

    private void UpdateBodyAnimation()
    {
        var t = _frameCount * 0.05; // Time factor

        double scaleX = 1.0, scaleY = 1.0, offsetY = 0;

        switch (_viewModel.CurrentState)
        {
            case PetState.Idle:
                // Gentle breathing
                scaleY = 1.0 + Math.Sin(t) * 0.02;
                scaleX = 1.0 - Math.Sin(t) * 0.01;
                break;

            case PetState.Walking:
                // Bouncy walk
                scaleY = 1.0 + Math.Sin(t * 6) * 0.1;
                scaleX = 1.0 - Math.Sin(t * 6) * 0.05;
                offsetY = Math.Abs(Math.Sin(t * 6)) * -8;
                break;

            case PetState.Sleeping:
                // Slow breathing
                scaleY = 1.0 + Math.Sin(t * 0.4) * 0.04;
                scaleX = 1.0 - Math.Sin(t * 0.4) * 0.02;
                break;

            case PetState.Eating:
                // Chomping
                scaleY = 1.0 + Math.Sin(t * 8) * 0.12;
                scaleX = 1.0 - Math.Sin(t * 8) * 0.06;
                break;

            case PetState.Playing:
                // Excited bouncing
                scaleY = 1.0 + Math.Sin(t * 10) * 0.18;
                scaleX = 1.0 - Math.Sin(t * 10) * 0.09;
                offsetY = Math.Abs(Math.Sin(t * 10)) * -15;
                break;

            case PetState.BeingPetted:
                // Happy squish
                scaleY = 0.88 + Math.Sin(t * 3) * 0.05;
                scaleX = 1.12 - Math.Sin(t * 3) * 0.03;
                offsetY = 3;
                break;

            case PetState.Celebrating:
                // Excited jumping
                scaleY = 1.0 + Math.Sin(t * 12) * 0.25;
                offsetY = Math.Abs(Math.Sin(t * 12)) * -20;
                break;
        }

        PetScaleTransform.ScaleX = scaleX;
        PetScaleTransform.ScaleY = scaleY;
        PetTranslateTransform.Y = offsetY;

        // Flip based on direction
        FaceFlip.ScaleX = _viewModel.FacingLeft ? -1 : 1;
    }

    private void UpdateParticleAnimations()
    {
        _particleFrame++;
        var t = _particleFrame * 0.08;

        // Hearts - float upward with wave motion
        if (_viewModel.ShowHearts && HeartsCanvas.Visibility == Visibility.Visible)
        {
            Canvas.SetTop(Heart1, -40 + Math.Sin(t) * 5 - (_particleFrame % 60));
            Canvas.SetLeft(Heart1, 0 + Math.Sin(t * 1.5) * 10);
            Heart1.Opacity = 1.0 - (_particleFrame % 60) / 60.0;

            Canvas.SetTop(Heart2, -30 + Math.Sin(t + 1) * 5 - ((_particleFrame + 20) % 60));
            Canvas.SetLeft(Heart2, 40 + Math.Cos(t * 1.3) * 8);
            Heart2.Opacity = 1.0 - ((_particleFrame + 20) % 60) / 60.0;

            Canvas.SetTop(Heart3, -35 + Math.Sin(t + 2) * 5 - ((_particleFrame + 40) % 60));
            Canvas.SetLeft(Heart3, 80 + Math.Sin(t * 1.7) * 6);
            Heart3.Opacity = 1.0 - ((_particleFrame + 40) % 60) / 60.0;
        }

        // Sparkles - pulsing scale and rotation
        if (_viewModel.ShowSparkles && SparklesCanvas.Visibility == Visibility.Visible)
        {
            var scale1 = 0.6 + Math.Abs(Math.Sin(t * 2)) * 0.8;
            var scale2 = 0.6 + Math.Abs(Math.Sin(t * 2 + 1)) * 0.8;
            var scale3 = 0.6 + Math.Abs(Math.Sin(t * 2 + 2)) * 0.8;

            Sparkle1.RenderTransform = new ScaleTransform(scale1, scale1);
            Sparkle2.RenderTransform = new ScaleTransform(scale2, scale2);
            Sparkle3.RenderTransform = new ScaleTransform(scale3, scale3);

            Canvas.SetTop(Sparkle1, -10 + Math.Sin(t * 3) * 15);
            Canvas.SetTop(Sparkle2, 5 + Math.Cos(t * 2.5) * 12);
            Canvas.SetTop(Sparkle3, -20 + Math.Sin(t * 3.5) * 18);
        }

        // ZZZ - float up and to the right
        if (_viewModel.ShowZzz && ZzzCanvas.Visibility == Visibility.Visible)
        {
            var zzzT = (_particleFrame % 120) / 120.0;

            Canvas.SetTop(Zzz1, 30 - zzzT * 40);
            Canvas.SetLeft(Zzz1, zzzT * 20);
            Zzz1.Opacity = 1.0 - zzzT;

            Canvas.SetTop(Zzz2, 15 - ((zzzT + 0.33) % 1) * 40);
            Canvas.SetLeft(Zzz2, 15 + ((zzzT + 0.33) % 1) * 20);
            Zzz2.Opacity = 1.0 - ((zzzT + 0.33) % 1);

            Canvas.SetTop(Zzz3, -5 - ((zzzT + 0.66) % 1) * 40);
            Canvas.SetLeft(Zzz3, 35 + ((zzzT + 0.66) % 1) * 20);
            Zzz3.Opacity = 1.0 - ((zzzT + 0.66) % 1);
        }

        // Food - bounce down then disappear
        if (_viewModel.ShowFood && FoodCanvas.Visibility == Visibility.Visible)
        {
            var foodT = (_particleFrame % 40) / 40.0;
            Canvas.SetTop(FoodEmoji, -30 + Math.Abs(Math.Sin(foodT * Math.PI * 3)) * 20 * (1 - foodT));
            FoodEmoji.Opacity = 1.0 - foodT * 0.5;
        }
    }

    #endregion

    #region Blink Timer

    private int _blinkCounter;
    private int _nextBlinkAt;
    private readonly Random _random = new();

    private void OnBlinkTick(object? sender, EventArgs e)
    {
        _blinkCounter++;

        if (_blinkCounter >= _nextBlinkAt)
        {
            // Start blink
            SetEyesClosed(true);
            _blinkCounter = 0;
            _nextBlinkAt = 30 + _random.Next(50); // 3-8 seconds between blinks

            // Schedule eye open
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            timer.Tick += (s, _) =>
            {
                if (!_viewModel.IsSleeping)
                {
                    SetEyesClosed(false);
                }
                ((DispatcherTimer)s!).Stop();
            };
            timer.Start();
        }
    }

    private void SetEyesClosed(bool closed)
    {
        var isClosed = closed || _viewModel.IsSleeping;

        LeftEyeWhite.Visibility = isClosed ? Visibility.Collapsed : Visibility.Visible;
        RightEyeWhite.Visibility = isClosed ? Visibility.Collapsed : Visibility.Visible;
        LeftPupil.Visibility = isClosed ? Visibility.Collapsed : Visibility.Visible;
        RightPupil.Visibility = isClosed ? Visibility.Collapsed : Visibility.Visible;
        LeftShine.Visibility = isClosed ? Visibility.Collapsed : Visibility.Visible;
        RightShine.Visibility = isClosed ? Visibility.Collapsed : Visibility.Visible;
        LeftEyeClosed.Visibility = isClosed ? Visibility.Visible : Visibility.Collapsed;
        RightEyeClosed.Visibility = isClosed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region ViewModel Property Changes

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DesktopPetViewModel.CurrentState):
                UpdateVisualState();
                break;
            case nameof(DesktopPetViewModel.ShowHearts):
                HeartsCanvas.Visibility = _viewModel.ShowHearts ? Visibility.Visible : Visibility.Collapsed;
                if (_viewModel.ShowHearts) _particleFrame = 0;
                break;
            case nameof(DesktopPetViewModel.ShowSparkles):
                SparklesCanvas.Visibility = _viewModel.ShowSparkles ? Visibility.Visible : Visibility.Collapsed;
                if (_viewModel.ShowSparkles) _particleFrame = 0;
                break;
            case nameof(DesktopPetViewModel.ShowZzz):
                ZzzCanvas.Visibility = _viewModel.ShowZzz ? Visibility.Visible : Visibility.Collapsed;
                if (_viewModel.ShowZzz) _particleFrame = 0;
                break;
            case nameof(DesktopPetViewModel.ShowFood):
                FoodCanvas.Visibility = _viewModel.ShowFood ? Visibility.Visible : Visibility.Collapsed;
                if (_viewModel.ShowFood) _particleFrame = 0;
                break;
            case nameof(DesktopPetViewModel.IsSleeping):
                SetEyesClosed(_viewModel.IsSleeping);
                break;
        }
    }

    private void UpdateVisualState()
    {
        PetMouth.Visibility = Visibility.Visible;
        PetMouthSad.Visibility = Visibility.Collapsed;
        PetMouthEating.Visibility = Visibility.Collapsed;

        switch (_viewModel.CurrentState)
        {
            case PetState.Eating:
                PetMouth.Visibility = Visibility.Collapsed;
                PetMouthEating.Visibility = Visibility.Visible;
                break;
            case PetState.Sad:
                PetMouth.Visibility = Visibility.Collapsed;
                PetMouthSad.Visibility = Visibility.Visible;
                break;
            case PetState.Sleeping:
                SetEyesClosed(true);
                break;
        }
    }

    #endregion

    #region Mouse Events

    private void Pet_MouseEnter(object sender, MouseEventArgs e)
    {
        _viewModel.OnMouseEnter();
        _mouseTracking = true;
    }

    private void Pet_MouseLeave(object sender, MouseEventArgs e)
    {
        _viewModel.OnMouseLeave();
        _mouseTracking = false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // Double-click to pet
        if (e.ClickCount == 2)
        {
            _viewModel.PetCommand.Execute(null);
        }
    }

    #endregion

    #region Context Menu Actions

    private void Feed_Click(object sender, RoutedEventArgs e) => _viewModel.FeedCommand.Execute(null);
    private void Play_Click(object sender, RoutedEventArgs e) => _viewModel.PlayCommand.Execute(null);
    private void Sleep_Click(object sender, RoutedEventArgs e) => _viewModel.SleepCommand.Execute(null);
    private void ToggleWalk_Click(object sender, RoutedEventArgs e) => _viewModel.CanWalk = !_viewModel.CanWalk;
    private void ToggleParticles_Click(object sender, RoutedEventArgs e) => _viewModel.ShowParticles = !_viewModel.ShowParticles;

    private void SetColorBlue_Click(object sender, RoutedEventArgs e) => _viewModel.SetColorCommand.Execute("#FF6B9DFF");
    private void SetColorPink_Click(object sender, RoutedEventArgs e) => _viewModel.SetColorCommand.Execute("#FFFF9DCC");
    private void SetColorGreen_Click(object sender, RoutedEventArgs e) => _viewModel.SetColorCommand.Execute("#FF9DFF9D");
    private void SetColorPurple_Click(object sender, RoutedEventArgs e) => _viewModel.SetColorCommand.Execute("#FFB39DFF");
    private void SetColorOrange_Click(object sender, RoutedEventArgs e) => _viewModel.SetColorCommand.Execute("#FFFFB366");
    private void SetColorMint_Click(object sender, RoutedEventArgs e) => _viewModel.SetColorCommand.Execute("#FF9DFFEF");

    #endregion
}
