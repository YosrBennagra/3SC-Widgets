using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace _3SC.Widgets.SystemPulse;

public partial class SystemPulseViewModel : ObservableObject, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<SystemPulseViewModel>();

    private readonly DispatcherTimer _updateTimer;
    private readonly PerformanceCounter? _cpuCounter;
    private readonly List<double> _cpuHistory = new();
    private readonly List<double> _ramHistory = new();
    private bool _isDisposed;
    private const int MaxHistoryPoints = 60;

    #region Observable Properties

    [ObservableProperty]
    private double _cpuUsage;

    [ObservableProperty]
    private double _ramUsage;

    [ObservableProperty]
    private double _diskUsage;

    [ObservableProperty]
    private string _cpuText = "0%";

    [ObservableProperty]
    private string _ramText = "0 / 0 GB";

    [ObservableProperty]
    private string _diskText = "0 / 0 GB";

    [ObservableProperty]
    private double _cpuPulseScale = 1.0;

    [ObservableProperty]
    private double _ramPulseScale = 1.0;

    [ObservableProperty]
    private Brush _cpuBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));

    [ObservableProperty]
    private Brush _ramBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243));

    [ObservableProperty]
    private Brush _diskBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0));

    [ObservableProperty]
    private PointCollection _cpuGraphPoints = new();

    [ObservableProperty]
    private PointCollection _ramGraphPoints = new();

    [ObservableProperty]
    private string _uptimeText = "0:00:00";

    [ObservableProperty]
    private int _processCount;

    #endregion

    public SystemPulseViewModel()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue(); // First call returns 0
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not create CPU performance counter");
        }

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    public void Initialize()
    {
        _updateTimer.Start();
        UpdateStats();
        Log.Information("System Pulse initialized");
    }

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        UpdateStats();
        AnimatePulse();
    }

    private void UpdateStats()
    {
        try
        {
            // CPU Usage
            if (_cpuCounter != null)
            {
                CpuUsage = Math.Min(100, _cpuCounter.NextValue());
                CpuText = $"{CpuUsage:F0}%";
                UpdateCpuColor();
            }

            // RAM Usage
            var ramInfo = GC.GetGCMemoryInfo();
            var totalRam = ramInfo.TotalAvailableMemoryBytes / (1024.0 * 1024 * 1024);
            var usedRam = (ramInfo.TotalAvailableMemoryBytes - ramInfo.HighMemoryLoadThresholdBytes) / (1024.0 * 1024 * 1024);

            // Use WMI for more accurate RAM info
            try
            {
                var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                var totalPhysical = computerInfo.TotalPhysicalMemory / (1024.0 * 1024 * 1024);
                var availablePhysical = computerInfo.AvailablePhysicalMemory / (1024.0 * 1024 * 1024);
                usedRam = totalPhysical - availablePhysical;
                totalRam = totalPhysical;
            }
            catch { }

            RamUsage = (usedRam / totalRam) * 100;
            RamText = $"{usedRam:F1} / {totalRam:F1} GB";

            // Disk Usage (C: drive)
            var drive = new DriveInfo("C");
            var totalDisk = drive.TotalSize / (1024.0 * 1024 * 1024);
            var usedDisk = (drive.TotalSize - drive.AvailableFreeSpace) / (1024.0 * 1024 * 1024);
            DiskUsage = (usedDisk / totalDisk) * 100;
            DiskText = $"{usedDisk:F0} / {totalDisk:F0} GB";

            // Process count
            ProcessCount = Process.GetProcesses().Length;

            // Uptime
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            UptimeText = uptime.Days > 0
                ? $"{uptime.Days}d {uptime.Hours}:{uptime.Minutes:D2}:{uptime.Seconds:D2}"
                : $"{uptime.Hours}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";

            // Update history for graphs
            UpdateHistory();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating system stats");
        }
    }

    private void UpdateCpuColor()
    {
        CpuBrush = CpuUsage switch
        {
            > 90 => new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Red
            > 70 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Orange
            > 50 => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // Yellow
            _ => new SolidColorBrush(Color.FromRgb(76, 175, 80))       // Green
        };
    }

    private void UpdateHistory()
    {
        _cpuHistory.Add(CpuUsage);
        _ramHistory.Add(RamUsage);

        if (_cpuHistory.Count > MaxHistoryPoints) _cpuHistory.RemoveAt(0);
        if (_ramHistory.Count > MaxHistoryPoints) _ramHistory.RemoveAt(0);

        CpuGraphPoints = CreateGraphPoints(_cpuHistory);
        RamGraphPoints = CreateGraphPoints(_ramHistory);
    }

    private PointCollection CreateGraphPoints(List<double> history)
    {
        var points = new PointCollection();
        if (history.Count < 2) return points;

        var width = 200.0;
        var height = 40.0;
        var step = width / (MaxHistoryPoints - 1);

        // Start at bottom left
        points.Add(new System.Windows.Point(0, height));

        for (int i = 0; i < history.Count; i++)
        {
            var x = i * step;
            var y = height - (history[i] / 100.0 * height);
            points.Add(new System.Windows.Point(x, y));
        }

        // End at bottom right
        points.Add(new System.Windows.Point((history.Count - 1) * step, height));

        return points;
    }

    private void AnimatePulse()
    {
        // Pulse effect based on CPU usage
        var intensity = 1.0 + (CpuUsage / 100.0) * 0.15;
        CpuPulseScale = intensity;

        // Reset after pulse
        Task.Delay(100).ContinueWith(_ =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                CpuPulseScale = 1.0;
            });
        });
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _updateTimer.Stop();
        _cpuCounter?.Dispose();
        Log.Information("System Pulse disposed");
    }
}
