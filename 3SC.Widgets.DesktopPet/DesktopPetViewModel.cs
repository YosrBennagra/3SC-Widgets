using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

// Explicit aliases to avoid conflicts with System.Drawing
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace _3SC.Widgets.DesktopPet;

/// <summary>
/// ViewModel for the Desktop Pet widget.
/// Manages pet state, animations, AI behavior, and user interactions.
/// </summary>
public partial class DesktopPetViewModel : ObservableObject, IDisposable
{
    #region Fields

    private static readonly ILogger Log = Serilog.Log.ForContext<DesktopPetViewModel>();
    private readonly string _settingsPath;
    private readonly DispatcherTimer _behaviorTimer;
    private readonly DispatcherTimer _needsDecayTimer;
    private readonly Random _random = new();
    private bool _isDisposed;

    // Movement
    private double _targetX;
    private double _targetY;
    private int _idleCounter;
    private int _stateTimer;

    #endregion

    #region Observable Properties - Pet Stats

    [ObservableProperty]
    private string _petName = "Blobby";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Mood))]
    [NotifyPropertyChangedFor(nameof(MoodEmoji))]
    [NotifyPropertyChangedFor(nameof(HappinessPercent))]
    private double _happiness = 80;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnergyPercent))]
    [NotifyPropertyChangedFor(nameof(IsTired))]
    private double _energy = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HungerPercent))]
    [NotifyPropertyChangedFor(nameof(IsHungry))]
    private double _hunger = 80;

    [ObservableProperty]
    private double _ageMinutes;

    [ObservableProperty]
    private int _timesFed;

    [ObservableProperty]
    private int _timesPlayed;

    [ObservableProperty]
    private int _timesPetted;

    #endregion

    #region Observable Properties - State

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWalking))]
    [NotifyPropertyChangedFor(nameof(IsSleeping))]
    [NotifyPropertyChangedFor(nameof(IsEating))]
    [NotifyPropertyChangedFor(nameof(IsPlaying))]
    [NotifyPropertyChangedFor(nameof(StateText))]
    private PetState _currentState = PetState.Idle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FacingLeft))]
    private PetDirection _direction = PetDirection.Right;

    [ObservableProperty]
    private bool _isMouseOver;

    [ObservableProperty]
    private double _petX;

    [ObservableProperty]
    private double _petY;

    #endregion

    #region Observable Properties - Particles

    [ObservableProperty]
    private bool _showHearts;

    [ObservableProperty]
    private bool _showSparkles;

    [ObservableProperty]
    private bool _showZzz;

    [ObservableProperty]
    private bool _showFood;

    #endregion

    #region Observable Properties - Appearance

    [ObservableProperty]
    private Brush _petBodyBrush = new SolidColorBrush(Color.FromRgb(107, 157, 255));

    [ObservableProperty]
    private string _petColorHex = "#FF6B9DFF";

    #endregion

    #region Observable Properties - Settings

    [ObservableProperty]
    private double _speedMultiplier = 1.0;

    [ObservableProperty]
    private bool _canWalk = true;

    [ObservableProperty]
    private bool _showParticles = true;

    #endregion

    #region Computed Properties

    public PetMood Mood => Happiness switch
    {
        > 90 => PetMood.Ecstatic,
        > 70 => PetMood.Happy,
        > 50 => PetMood.Content,
        > 30 => PetMood.Neutral,
        > 10 => PetMood.Sad,
        _ => PetMood.Miserable
    };

    public string MoodEmoji => Mood switch
    {
        PetMood.Ecstatic => "🤩",
        PetMood.Happy => "😊",
        PetMood.Content => "🙂",
        PetMood.Neutral => "😐",
        PetMood.Sad => "😢",
        PetMood.Miserable => "😭",
        _ => "🙂"
    };

    public double HappinessPercent => Happiness / 100.0;
    public double EnergyPercent => Energy / 100.0;
    public double HungerPercent => Hunger / 100.0;

    public bool IsTired => Energy < 30;
    public bool IsHungry => Hunger < 30;

    public bool IsWalking => CurrentState == PetState.Walking;
    public bool IsSleeping => CurrentState == PetState.Sleeping;
    public bool IsEating => CurrentState == PetState.Eating;
    public bool IsPlaying => CurrentState == PetState.Playing;
    public bool FacingLeft => Direction == PetDirection.Left;

    public string StateText => CurrentState switch
    {
        PetState.Idle => $"{PetName} is chilling",
        PetState.Walking => $"{PetName} is exploring",
        PetState.Sleeping => $"{PetName} is sleeping... 💤",
        PetState.Eating => $"{PetName} is eating! 🍎",
        PetState.Playing => $"{PetName} is having fun! ⭐",
        PetState.BeingPetted => $"{PetName} loves this! ❤️",
        PetState.Celebrating => $"{PetName} is so happy! 🎉",
        PetState.Sad => $"{PetName} is feeling down... 😢",
        PetState.FollowingMouse => $"{PetName} wants attention!",
        _ => $"{PetName}"
    };

    public string AgeText
    {
        get
        {
            if (AgeMinutes < 60) return $"{(int)AgeMinutes} minutes old";
            if (AgeMinutes < 1440) return $"{(int)(AgeMinutes / 60)} hours old";
            return $"{(int)(AgeMinutes / 1440)} days old";
        }
    }

    #endregion

    #region Constructor

    public DesktopPetViewModel()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", "desktop-pet", "settings.json");

        // Behavior timer - controls AI decisions (every 100ms)
        _behaviorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _behaviorTimer.Tick += OnBehaviorTick;

        // Needs decay timer - decreases stats over time (every 30 seconds)
        _needsDecayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _needsDecayTimer.Tick += OnNeedsDecayTick;

        Log.Debug("DesktopPetViewModel created");
    }

    #endregion

    #region Lifecycle

    public void Initialize()
    {
        LoadSettings();
        _behaviorTimer.Start();
        _needsDecayTimer.Start();
        Log.Information("Desktop Pet initialized: {Name}", PetName);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _behaviorTimer.Stop();
        _needsDecayTimer.Stop();

        SaveSettings();
        Log.Information("Desktop Pet disposed: {Name}", PetName);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Feed()
    {
        if (CurrentState == PetState.Eating) return;

        Log.Debug("Feeding pet");
        CurrentState = PetState.Eating;
        _stateTimer = 30; // 3 seconds of eating

        Hunger = Math.Min(100, Hunger + 25);
        Happiness = Math.Min(100, Happiness + 5);
        TimesFed++;

        if (ShowParticles) ShowFood = true;

        SaveSettings();
    }

    [RelayCommand]
    private void Play()
    {
        if (CurrentState == PetState.Playing || Energy < 10) return;

        Log.Debug("Playing with pet");
        CurrentState = PetState.Playing;
        _stateTimer = 50; // 5 seconds of playing

        Happiness = Math.Min(100, Happiness + 15);
        Energy = Math.Max(0, Energy - 10);
        TimesPlayed++;

        if (ShowParticles) ShowSparkles = true;

        SaveSettings();
    }

    [RelayCommand]
    private void Pet()
    {
        Log.Debug("Petting the pet");
        CurrentState = PetState.BeingPetted;
        _stateTimer = 20; // 2 seconds

        Happiness = Math.Min(100, Happiness + 3);
        TimesPetted++;

        if (ShowParticles) ShowHearts = true;

        SaveSettings();
    }

    [RelayCommand]
    private void Sleep()
    {
        if (CurrentState == PetState.Sleeping)
        {
            // Wake up
            CurrentState = PetState.Idle;
            ShowZzz = false;
            return;
        }

        Log.Debug("Pet going to sleep");
        CurrentState = PetState.Sleeping;
        _stateTimer = 100; // 10 seconds of sleep

        if (ShowParticles) ShowZzz = true;
    }

    [RelayCommand]
    private void SetColor(string? colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex)) return;

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            PetBodyBrush = new SolidColorBrush(color);
            PetColorHex = colorHex;
            SaveSettings();
        }
        catch { /* Invalid color */ }
    }

    #endregion

    #region AI Behavior

    private void OnBehaviorTick(object? sender, EventArgs e)
    {
        if (_isDisposed) return;

        // Decrease state timer
        if (_stateTimer > 0)
        {
            _stateTimer--;
            if (_stateTimer == 0)
            {
                OnStateComplete();
            }
        }

        // AI decision making
        switch (CurrentState)
        {
            case PetState.Idle:
                HandleIdleState();
                break;
            case PetState.Walking:
                HandleWalkingState();
                break;
            case PetState.Sleeping:
                HandleSleepingState();
                break;
        }

        // Update age
        AgeMinutes += 0.1 / 60.0;
    }

    private void HandleIdleState()
    {
        _idleCounter++;

        // Random chance to start walking
        if (CanWalk && _idleCounter > 20 && _random.Next(100) < 3)
        {
            StartWalking();
        }

        // Auto-sleep if very tired
        if (Energy < 15 && _random.Next(100) < 5)
        {
            Sleep();
        }

        // Show sadness if unhappy
        if (Happiness < 20)
        {
            CurrentState = PetState.Sad;
            _stateTimer = 30;
        }
    }

    private void HandleWalkingState()
    {
        // Move toward target
        var dx = _targetX - PetX;
        var dy = _targetY - PetY;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < 5)
        {
            CurrentState = PetState.Idle;
            _idleCounter = 0;
            return;
        }

        // Update direction
        Direction = dx < 0 ? PetDirection.Left : PetDirection.Right;

        // Move pet
        var speed = 2.0 * SpeedMultiplier;
        PetX += (dx / distance) * speed;
        PetY += (dy / distance) * speed;
    }

    private void HandleSleepingState()
    {
        // Restore energy while sleeping
        Energy = Math.Min(100, Energy + 0.3);
        Happiness = Math.Min(100, Happiness + 0.05);

        if (Energy >= 100)
        {
            CurrentState = PetState.Idle;
            ShowZzz = false;
        }
    }

    private void OnStateComplete()
    {
        ShowHearts = false;
        ShowSparkles = false;
        ShowFood = false;

        if (CurrentState != PetState.Sleeping)
        {
            ShowZzz = false;
        }

        CurrentState = PetState.Idle;
        _idleCounter = 0;
    }

    private void StartWalking()
    {
        _targetX = PetX + _random.Next(-30, 30);
        _targetY = PetY + _random.Next(-15, 15);

        _targetX = Math.Clamp(_targetX, -20, 20);
        _targetY = Math.Clamp(_targetY, -10, 10);

        CurrentState = PetState.Walking;
    }

    #endregion

    #region Needs Decay

    private void OnNeedsDecayTick(object? sender, EventArgs e)
    {
        if (_isDisposed) return;

        Hunger = Math.Max(0, Hunger - 1.5);

        if (Hunger < 30) Happiness = Math.Max(0, Happiness - 1);
        if (Energy < 30) Happiness = Math.Max(0, Happiness - 0.5);

        Happiness = Math.Max(0, Happiness - 0.2);

        if (CurrentState != PetState.Sleeping)
        {
            Energy = Math.Max(0, Energy - 0.5);
        }

        SaveSettings();
    }

    #endregion

    #region Mouse Tracking

    public void OnMouseEnter()
    {
        IsMouseOver = true;
        if (CurrentState == PetState.Idle || CurrentState == PetState.Sad)
        {
            Happiness = Math.Min(100, Happiness + 0.5);
        }
    }

    public void OnMouseLeave()
    {
        IsMouseOver = false;
    }

    #endregion

    #region Settings Persistence

    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return;

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<PetSettings>(json);

            if (settings == null) return;

            PetName = settings.PetName;
            Happiness = settings.Happiness;
            Energy = settings.Energy;
            Hunger = settings.Hunger;
            AgeMinutes = settings.AgeMinutes;
            TimesFed = settings.TimesFed;
            TimesPlayed = settings.TimesPlayed;
            TimesPetted = settings.TimesPetted;
            SpeedMultiplier = settings.SpeedMultiplier;
            CanWalk = settings.CanWalk;
            ShowParticles = settings.ShowParticles;
            PetColorHex = settings.PetColor;
            PetX = settings.WindowX;
            PetY = settings.WindowY;

            try
            {
                PetBodyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.PetColor));
            }
            catch { }

            // Time-based decay
            var timeSinceLastSave = DateTime.Now - settings.LastSaveTime;
            var decayMinutes = timeSinceLastSave.TotalMinutes;

            if (decayMinutes > 1)
            {
                Hunger = Math.Max(0, Hunger - decayMinutes * 0.5);
                Energy = Math.Min(100, Energy + decayMinutes * 0.3);
                Happiness = Math.Max(0, Happiness - decayMinutes * 0.2);
                AgeMinutes += decayMinutes;
            }

            Log.Information("Settings loaded for pet: {Name}", PetName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings");
        }
    }

    private void SaveSettings()
    {
        try
        {
            var settings = new PetSettings
            {
                PetName = PetName,
                Happiness = Happiness,
                Energy = Energy,
                Hunger = Hunger,
                AgeMinutes = AgeMinutes,
                TimesFed = TimesFed,
                TimesPlayed = TimesPlayed,
                TimesPetted = TimesPetted,
                SpeedMultiplier = SpeedMultiplier,
                CanWalk = CanWalk,
                ShowParticles = ShowParticles,
                PetColor = PetColorHex,
                WindowX = PetX,
                WindowY = PetY,
                LastSaveTime = DateTime.Now
            };

            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings");
        }
    }

    #endregion
}
