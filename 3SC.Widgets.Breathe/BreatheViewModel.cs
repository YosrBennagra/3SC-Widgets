using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.Breathe;

public partial class BreatheViewModel : ObservableObject, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<BreatheViewModel>();

    private readonly DispatcherTimer _breathTimer;
    private readonly DispatcherTimer _animationTimer;
    private int _phaseTimeRemaining;
    private int _cycleCount;
    private bool _isDisposed;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<BreathingPattern> _patterns = new();

    [ObservableProperty]
    private BreathingPattern? _selectedPattern;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private BreathPhase _currentPhase = BreathPhase.Idle;

    [ObservableProperty]
    private string _phaseText = "Tap to start";

    [ObservableProperty]
    private string _timerText = "";

    [ObservableProperty]
    private int _completedCycles;

    [ObservableProperty]
    private double _circleScale = 0.5;

    [ObservableProperty]
    private double _circleOpacity = 0.6;

    [ObservableProperty]
    private double _ringRotation;

    [ObservableProperty]
    private Brush _circleBrush = new SolidColorBrush(Color.FromRgb(79, 209, 197));

    [ObservableProperty]
    private Brush _backgroundBrush = new LinearGradientBrush(
        Color.FromRgb(26, 32, 44), Color.FromRgb(45, 55, 72), 90);

    #endregion

    public BreatheViewModel()
    {
        InitializePatterns();

        _breathTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _breathTimer.Tick += OnBreathTick;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)  // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
    }

    private void InitializePatterns()
    {
        Patterns = new ObservableCollection<BreathingPattern>
        {
            new()
            {
                Name = "Relaxing",
                Description = "4-7-8 technique for deep relaxation",
                InhaleSeconds = 4,
                HoldSeconds = 7,
                ExhaleSeconds = 8,
                RestSeconds = 0,
                ColorStart = "#4FD1C5",
                ColorEnd = "#38B2AC"
            },
            new()
            {
                Name = "Box",
                Description = "4-4-4-4 box breathing for focus",
                InhaleSeconds = 4,
                HoldSeconds = 4,
                ExhaleSeconds = 4,
                RestSeconds = 4,
                ColorStart = "#667EEA",
                ColorEnd = "#5A67D8"
            },
            new()
            {
                Name = "Calming",
                Description = "5-5 simple breathing for calm",
                InhaleSeconds = 5,
                HoldSeconds = 0,
                ExhaleSeconds = 5,
                RestSeconds = 0,
                ColorStart = "#9F7AEA",
                ColorEnd = "#805AD5"
            },
            new()
            {
                Name = "Energizing",
                Description = "4-0-4 quick breaths for energy",
                InhaleSeconds = 4,
                HoldSeconds = 0,
                ExhaleSeconds = 4,
                RestSeconds = 0,
                ColorStart = "#F6AD55",
                ColorEnd = "#ED8936"
            },
            new()
            {
                Name = "Sleep",
                Description = "4-7-8 extended for sleep preparation",
                InhaleSeconds = 4,
                HoldSeconds = 7,
                ExhaleSeconds = 8,
                RestSeconds = 2,
                ColorStart = "#4A5568",
                ColorEnd = "#2D3748"
            }
        };

        SelectedPattern = Patterns[0];
    }

    public void Initialize()
    {
        _animationTimer.Start();
        UpdateColors();
        Log.Information("Breathe widget initialized");
    }

    #region Commands

    [RelayCommand]
    private void ToggleBreathing()
    {
        if (IsRunning)
        {
            StopBreathing();
        }
        else
        {
            StartBreathing();
        }
    }

    [RelayCommand]
    private void SelectPattern(BreathingPattern? pattern)
    {
        if (pattern == null) return;

        SelectedPattern = pattern;
        UpdateColors();

        if (IsRunning)
        {
            StopBreathing();
            StartBreathing();
        }
    }

    [RelayCommand]
    private void NextPattern()
    {
        if (SelectedPattern == null) return;

        var index = Patterns.IndexOf(SelectedPattern);
        var nextIndex = (index + 1) % Patterns.Count;
        SelectPattern(Patterns[nextIndex]);
    }

    #endregion

    #region Breathing Logic

    private void StartBreathing()
    {
        if (SelectedPattern == null) return;

        IsRunning = true;
        _cycleCount = 0;
        StartPhase(BreathPhase.Inhale);
        _breathTimer.Start();

        Log.Information("Started breathing pattern: {Pattern}", SelectedPattern.Name);
    }

    private void StopBreathing()
    {
        IsRunning = false;
        _breathTimer.Stop();
        CurrentPhase = BreathPhase.Idle;
        PhaseText = "Tap to start";
        TimerText = "";
        CircleScale = 0.5;

        Log.Information("Stopped breathing");
    }

    private void StartPhase(BreathPhase phase)
    {
        if (SelectedPattern == null) return;

        CurrentPhase = phase;

        switch (phase)
        {
            case BreathPhase.Inhale:
                _phaseTimeRemaining = SelectedPattern.InhaleSeconds;
                PhaseText = "Breathe In";
                break;
            case BreathPhase.Hold:
                _phaseTimeRemaining = SelectedPattern.HoldSeconds;
                PhaseText = "Hold";
                break;
            case BreathPhase.Exhale:
                _phaseTimeRemaining = SelectedPattern.ExhaleSeconds;
                PhaseText = "Breathe Out";
                break;
            case BreathPhase.Rest:
                _phaseTimeRemaining = SelectedPattern.RestSeconds;
                PhaseText = "Rest";
                break;
        }

        TimerText = _phaseTimeRemaining.ToString();
    }

    private void OnBreathTick(object? sender, EventArgs e)
    {
        if (SelectedPattern == null) return;

        _phaseTimeRemaining--;
        TimerText = _phaseTimeRemaining > 0 ? _phaseTimeRemaining.ToString() : "";

        if (_phaseTimeRemaining <= 0)
        {
            // Move to next phase
            var nextPhase = CurrentPhase switch
            {
                BreathPhase.Inhale => SelectedPattern.HoldSeconds > 0 ? BreathPhase.Hold : BreathPhase.Exhale,
                BreathPhase.Hold => BreathPhase.Exhale,
                BreathPhase.Exhale => SelectedPattern.RestSeconds > 0 ? BreathPhase.Rest : BreathPhase.Inhale,
                BreathPhase.Rest => BreathPhase.Inhale,
                _ => BreathPhase.Inhale
            };

            if (nextPhase == BreathPhase.Inhale)
            {
                _cycleCount++;
                CompletedCycles = _cycleCount;
            }

            StartPhase(nextPhase);
        }
    }

    #endregion

    #region Animation

    private double _targetScale;
    private double _animationPhase;

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _animationPhase += 0.02;
        RingRotation = (_animationPhase * 30) % 360;

        if (!IsRunning)
        {
            // Idle gentle pulse
            CircleScale = 0.5 + Math.Sin(_animationPhase * 2) * 0.05;
            CircleOpacity = 0.6 + Math.Sin(_animationPhase * 3) * 0.1;
            return;
        }

        // Calculate target scale based on phase
        _targetScale = CurrentPhase switch
        {
            BreathPhase.Inhale => 1.0,
            BreathPhase.Hold => 1.0,
            BreathPhase.Exhale => 0.4,
            BreathPhase.Rest => 0.4,
            _ => 0.5
        };

        // Smooth interpolation toward target
        var diff = _targetScale - CircleScale;
        var speed = CurrentPhase switch
        {
            BreathPhase.Inhale => 0.015,
            BreathPhase.Exhale => 0.012,
            _ => 0.01
        };
        CircleScale += diff * speed * 2;

        // Opacity pulse
        CircleOpacity = 0.7 + Math.Sin(_animationPhase * 4) * 0.15;
    }

    private void UpdateColors()
    {
        if (SelectedPattern == null) return;

        try
        {
            var startColor = (Color)ColorConverter.ConvertFromString(SelectedPattern.ColorStart);
            var endColor = (Color)ColorConverter.ConvertFromString(SelectedPattern.ColorEnd);

            CircleBrush = new RadialGradientBrush(startColor, endColor);
            BackgroundBrush = new LinearGradientBrush(
                Color.FromRgb(26, 32, 44),
                Color.FromRgb((byte)(startColor.R / 4), (byte)(startColor.G / 4), (byte)(startColor.B / 4)),
                90);
        }
        catch { }
    }

    #endregion

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _breathTimer.Stop();
        _animationTimer.Stop();

        Log.Information("Breathe widget disposed");
    }
}
