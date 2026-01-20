using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Serilog;

namespace _3SC.Widgets.AmbientSounds;

public partial class AmbientSoundsViewModel : ObservableObject, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AmbientSoundsViewModel>();

    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new();
    private WaveOutEvent? _waveOut;
    private ISampleProvider? _currentSound;
    private bool _isDisposed;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<SoundPreset> _presets = new();

    [ObservableProperty]
    private SoundPreset? _selectedPreset;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private double _volume = 0.5;

    [ObservableProperty]
    private string _currentIcon = "🌧️";

    [ObservableProperty]
    private string _statusText = "Select a sound";

    [ObservableProperty]
    private Brush _backgroundBrush = new SolidColorBrush(Color.FromRgb(26, 26, 46));

    // Animation properties
    [ObservableProperty]
    private double _particle1X;

    [ObservableProperty]
    private double _particle1Y;

    [ObservableProperty]
    private double _particle2X;

    [ObservableProperty]
    private double _particle2Y;

    [ObservableProperty]
    private double _particle3X;

    [ObservableProperty]
    private double _particle3Y;

    [ObservableProperty]
    private double _waveOffset;

    [ObservableProperty]
    private double _animationOpacity = 0.6;

    #endregion

    public AmbientSoundsViewModel()
    {
        InitializePresets();

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTick;
    }

    private void InitializePresets()
    {
        Presets = new ObservableCollection<SoundPreset>
        {
            new() { Name = "Rain", Icon = "🌧️", Type = SoundType.Rain,
                    Description = "Gentle rain on a window", GradientStart = "#1a1a2e", GradientEnd = "#2d3748" },
            new() { Name = "Thunder", Icon = "⛈️", Type = SoundType.Thunder,
                    Description = "Thunderstorm with rain", GradientStart = "#1a1a2e", GradientEnd = "#2d3748" },
            new() { Name = "Ocean", Icon = "🌊", Type = SoundType.Ocean,
                    Description = "Waves on a beach", GradientStart = "#0f3460", GradientEnd = "#16213e" },
            new() { Name = "Forest", Icon = "🌲", Type = SoundType.Forest,
                    Description = "Birds and rustling leaves", GradientStart = "#1b4332", GradientEnd = "#2d6a4f" },
            new() { Name = "Fire", Icon = "🔥", Type = SoundType.Fire,
                    Description = "Crackling fireplace", GradientStart = "#3d1c02", GradientEnd = "#5c2e0a" },
            new() { Name = "Wind", Icon = "💨", Type = SoundType.Wind,
                    Description = "Gentle wind through trees", GradientStart = "#2d3748", GradientEnd = "#4a5568" },
            new() { Name = "Café", Icon = "☕", Type = SoundType.Cafe,
                    Description = "Coffee shop ambiance", GradientStart = "#3d2914", GradientEnd = "#5c4033" },
            new() { Name = "White Noise", Icon = "📻", Type = SoundType.WhiteNoise,
                    Description = "Pure white noise", GradientStart = "#1a1a2e", GradientEnd = "#2d3748" },
            new() { Name = "Brown Noise", Icon = "🟤", Type = SoundType.BrownNoise,
                    Description = "Deep brown noise", GradientStart = "#2d1b0e", GradientEnd = "#4a3728" },
            new() { Name = "Pink Noise", Icon = "🩷", Type = SoundType.PinkNoise,
                    Description = "Soft pink noise", GradientStart = "#4a1942", GradientEnd = "#6b2c5c" }
        };
    }

    public void Initialize()
    {
        _animationTimer.Start();
        Log.Information("Ambient Sounds initialized");
    }

    #region Commands

    [RelayCommand]
    private void SelectPreset(SoundPreset? preset)
    {
        if (preset == null) return;

        SelectedPreset = preset;
        CurrentIcon = preset.Icon;
        StatusText = preset.Description;

        // Update background gradient
        try
        {
            var startColor = (Color)ColorConverter.ConvertFromString(preset.GradientStart);
            var endColor = (Color)ColorConverter.ConvertFromString(preset.GradientEnd);
            BackgroundBrush = new LinearGradientBrush(startColor, endColor, 90);
        }
        catch { }

        if (IsPlaying)
        {
            StopSound();
            PlaySound();
        }
    }

    [RelayCommand]
    private void TogglePlay()
    {
        if (IsPlaying)
        {
            StopSound();
        }
        else
        {
            PlaySound();
        }
    }

    [RelayCommand]
    private void SetVolume(double volume)
    {
        Volume = Math.Clamp(volume, 0, 1);
        if (_waveOut != null)
        {
            _waveOut.Volume = (float)Volume;
        }
    }

    #endregion

    #region Sound Generation

    private void PlaySound()
    {
        if (SelectedPreset == null)
        {
            SelectedPreset = Presets.FirstOrDefault();
            if (SelectedPreset != null)
            {
                SelectPreset(SelectedPreset);
            }
        }

        if (SelectedPreset == null) return;

        try
        {
            _currentSound = CreateSoundForType(SelectedPreset.Type);

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_currentSound);
            _waveOut.Volume = (float)Volume;
            _waveOut.Play();

            IsPlaying = true;
            StatusText = $"Playing: {SelectedPreset.Name}";
            Log.Information("Playing sound: {Type}", SelectedPreset.Type);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to play sound");
            StatusText = "Error playing sound";
        }
    }

    private void StopSound()
    {
        try
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;

            IsPlaying = false;
            StatusText = SelectedPreset?.Description ?? "Select a sound";
            Log.Information("Sound stopped");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error stopping sound");
        }
    }

    private ISampleProvider CreateSoundForType(SoundType type)
    {
        return type switch
        {
            SoundType.WhiteNoise => new NoiseGenerator(NoiseType.White),
            SoundType.BrownNoise => new NoiseGenerator(NoiseType.Brown),
            SoundType.PinkNoise => new NoiseGenerator(NoiseType.Pink),
            SoundType.Rain => new RainSoundGenerator(),
            SoundType.Thunder => new ThunderSoundGenerator(),
            SoundType.Ocean => new OceanSoundGenerator(),
            SoundType.Forest => new ForestSoundGenerator(),
            SoundType.Fire => new FireSoundGenerator(),
            SoundType.Wind => new WindSoundGenerator(),
            SoundType.Cafe => new CafeSoundGenerator(),
            _ => new NoiseGenerator(NoiseType.White)
        };
    }

    #endregion

    #region Animation

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        // Animate particles based on sound type
        WaveOffset = (WaveOffset + 2) % 360;

        // Floating particle animation
        Particle1Y = Math.Sin(WaveOffset * Math.PI / 180) * 10;
        Particle2Y = Math.Sin((WaveOffset + 120) * Math.PI / 180) * 8;
        Particle3Y = Math.Sin((WaveOffset + 240) * Math.PI / 180) * 12;

        Particle1X = Math.Cos(WaveOffset * Math.PI / 180) * 5;
        Particle2X = Math.Cos((WaveOffset + 90) * Math.PI / 180) * 6;
        Particle3X = Math.Cos((WaveOffset + 180) * Math.PI / 180) * 4;

        // Pulse animation when playing
        if (IsPlaying)
        {
            AnimationOpacity = 0.6 + Math.Sin(WaveOffset * Math.PI / 90) * 0.3;
        }
    }

    #endregion

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _animationTimer.Stop();
        StopSound();

        Log.Information("Ambient Sounds disposed");
    }
}

#region Sound Generators

public enum NoiseType { White, Pink, Brown }

public class NoiseGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private readonly NoiseType _type;
    private double _lastValue;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public NoiseGenerator(NoiseType type)
    {
        _type = type;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float sample = _type switch
            {
                NoiseType.White => (float)(_random.NextDouble() * 2 - 1) * 0.3f,
                NoiseType.Brown => GenerateBrownNoise(),
                NoiseType.Pink => GeneratePinkNoise(),
                _ => 0
            };
            buffer[offset + i] = sample;
        }
        return count;
    }

    private float GenerateBrownNoise()
    {
        double white = _random.NextDouble() * 2 - 1;
        _lastValue = (_lastValue + (0.02 * white)) / 1.02;
        return (float)(_lastValue * 3.5 * 0.3);
    }

    private float GeneratePinkNoise()
    {
        double white = _random.NextDouble() * 2 - 1;
        _lastValue = (_lastValue * 0.99) + (white * 0.1);
        return (float)(_lastValue * 0.3);
    }
}

// Rain sound - white noise filtered to sound like rain
public class RainSoundGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private double _filter;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            double white = _random.NextDouble() * 2 - 1;
            _filter = _filter * 0.95 + white * 0.05;
            float sample = (float)(_filter * 0.4);
            buffer[offset + i] = sample;
        }
        return count;
    }
}

// Thunder - occasional rumbles over rain
public class ThunderSoundGenerator : ISampleProvider
{
    private readonly RainSoundGenerator _rain = new();
    private readonly Random _random = new();
    private int _thunderCountdown;
    private double _thunderIntensity;

    public WaveFormat WaveFormat => _rain.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        _rain.Read(buffer, offset, count);

        // Random thunder
        if (_thunderCountdown <= 0 && _random.Next(44100 * 10) < 1)
        {
            _thunderCountdown = 44100 / 2; // 0.5 sec thunder
            _thunderIntensity = _random.NextDouble() * 0.5 + 0.3;
        }

        if (_thunderCountdown > 0)
        {
            for (int i = 0; i < count && _thunderCountdown > 0; i++)
            {
                double rumble = (_random.NextDouble() * 2 - 1) * _thunderIntensity;
                buffer[offset + i] += (float)(rumble * (_thunderCountdown / 22050.0));
                _thunderCountdown--;
            }
        }

        return count;
    }
}

// Ocean waves - modulated low-pass noise
public class OceanSoundGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private double _wavePhase;
    private double _filter;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _wavePhase += 0.00005;
            double waveIntensity = (Math.Sin(_wavePhase) + 1) * 0.5;

            double white = _random.NextDouble() * 2 - 1;
            _filter = _filter * 0.98 + white * 0.02;
            float sample = (float)(_filter * waveIntensity * 0.5);
            buffer[offset + i] = sample;
        }
        return count;
    }
}

// Forest - bird chirps over light wind
public class ForestSoundGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private double _windFilter;
    private int _birdCountdown;
    private double _birdFreq;
    private double _birdPhase;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Light wind
            double white = _random.NextDouble() * 2 - 1;
            _windFilter = _windFilter * 0.99 + white * 0.01;
            float sample = (float)(_windFilter * 0.15);

            // Bird chirps
            if (_birdCountdown <= 0 && _random.Next(44100 * 3) < 1)
            {
                _birdCountdown = _random.Next(2000, 8000);
                _birdFreq = _random.Next(2000, 4000);
            }

            if (_birdCountdown > 0)
            {
                _birdPhase += _birdFreq / 44100.0 * 2 * Math.PI;
                sample += (float)(Math.Sin(_birdPhase) * 0.1 * (_birdCountdown / 8000.0));
                _birdCountdown--;
            }

            buffer[offset + i] = sample;
        }
        return count;
    }
}

// Fire crackling
public class FireSoundGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private double _filter;
    private int _crackleCountdown;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Base crackle
            double white = _random.NextDouble() * 2 - 1;
            _filter = _filter * 0.8 + white * 0.2;
            float sample = (float)(_filter * 0.2);

            // Random pops
            if (_crackleCountdown <= 0 && _random.Next(1000) < 3)
            {
                _crackleCountdown = _random.Next(50, 200);
            }

            if (_crackleCountdown > 0)
            {
                sample += (float)((_random.NextDouble() * 2 - 1) * 0.4 * (_crackleCountdown / 200.0));
                _crackleCountdown--;
            }

            buffer[offset + i] = sample;
        }
        return count;
    }
}

// Wind - modulated filtered noise
public class WindSoundGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private double _windPhase;
    private double _filter;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _windPhase += 0.0001;
            double intensity = (Math.Sin(_windPhase) + Math.Sin(_windPhase * 2.3) + 2) * 0.25;

            double white = _random.NextDouble() * 2 - 1;
            _filter = _filter * 0.97 + white * 0.03;
            float sample = (float)(_filter * intensity * 0.4);
            buffer[offset + i] = sample;
        }
        return count;
    }
}

// Cafe ambiance - murmurs and occasional sounds
public class CafeSoundGenerator : ISampleProvider
{
    private readonly Random _random = new();
    private double _murmurFilter;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            double white = _random.NextDouble() * 2 - 1;
            _murmurFilter = _murmurFilter * 0.95 + white * 0.05;

            // Multiple layers of filtered noise
            float sample = (float)(_murmurFilter * 0.25);
            buffer[offset + i] = sample;
        }
        return count;
    }
}

#endregion
