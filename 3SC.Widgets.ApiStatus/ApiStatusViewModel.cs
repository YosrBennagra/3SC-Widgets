using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace _3SC.Widgets.ApiStatus;

public partial class ApiStatusViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly DispatcherTimer _checkTimer;
    private ApiStatusSettings _settings;

    [ObservableProperty]
    private ObservableCollection<ApiEndpoint> _endpoints = new();

    [ObservableProperty]
    private ApiEndpoint? _selectedEndpoint;

    [ObservableProperty]
    private int _checkInterval = 60;

    [ObservableProperty]
    private bool _alertOnDowntime = true;

    [ObservableProperty]
    private string _lastCheckTime = "Never";

    [ObservableProperty]
    private bool _isChecking;

    public ApiStatusViewModel()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _checkTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(60)
        };
        _checkTimer.Tick += async (s, e) => await CheckAllEndpoints();

        _settings = ApiStatusSettings.Load();
        LoadSettings();

        _checkTimer.Start();

        // Initial check
        Task.Run(async () => await CheckAllEndpoints());
    }

    private void LoadSettings()
    {
        CheckInterval = _settings.CheckIntervalSeconds;
        AlertOnDowntime = _settings.AlertOnDowntime;

        foreach (var saved in _settings.Endpoints)
        {
            Endpoints.Add(new ApiEndpoint
            {
                Name = saved.Name,
                Url = saved.Url,
                StatusMessage = "Waiting for check..."
            });
        }
    }

    public void SaveSettings()
    {
        _settings.Endpoints = Endpoints.Select(e => new SavedEndpoint
        {
            Name = e.Name,
            Url = e.Url
        }).ToList();
        _settings.CheckIntervalSeconds = CheckInterval;
        _settings.AlertOnDowntime = AlertOnDowntime;
        _settings.Save();

        // Update timer interval
        _checkTimer.Interval = TimeSpan.FromSeconds(CheckInterval);
    }

    [RelayCommand]
    private async Task CheckAllEndpoints()
    {
        if (IsChecking || !Endpoints.Any()) return;

        IsChecking = true;
        LastCheckTime = DateTime.Now.ToString("HH:mm:ss");

        var tasks = Endpoints.Select(CheckEndpoint);
        await Task.WhenAll(tasks);

        IsChecking = false;
    }

    private async Task CheckEndpoint(ApiEndpoint endpoint)
    {
        var stopwatch = Stopwatch.StartNew();
        bool success = false;
        string statusMessage;

        try
        {
            var response = await _httpClient.GetAsync(endpoint.Url);
            stopwatch.Stop();

            success = response.IsSuccessStatusCode;
            var responseTime = stopwatch.Elapsed.TotalMilliseconds;

            endpoint.ResponseTime = Math.Round(responseTime, 2);
            endpoint.IsOnline = success;
            endpoint.LastChecked = DateTime.Now;
            endpoint.AddResponseTime(responseTime, success);

            statusMessage = success
                ? $"✓ {(int)response.StatusCode} - {responseTime:F0}ms"
                : $"✗ {(int)response.StatusCode} {response.ReasonPhrase}";
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            endpoint.IsOnline = false;
            endpoint.LastChecked = DateTime.Now;
            endpoint.AddResponseTime(0, false);
            statusMessage = $"✗ Connection failed: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            endpoint.IsOnline = false;
            endpoint.LastChecked = DateTime.Now;
            endpoint.AddResponseTime(0, false);
            statusMessage = "✗ Timeout";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            endpoint.IsOnline = false;
            endpoint.LastChecked = DateTime.Now;
            endpoint.AddResponseTime(0, false);
            statusMessage = $"✗ Error: {ex.Message}";
            Serilog.Log.Error(ex, "Failed to check endpoint: {Url}", endpoint.Url);
        }

        endpoint.StatusMessage = statusMessage;

        // Alert on downtime
        if (!success && AlertOnDowntime)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                // Simple notification - could be enhanced with toast notifications
                Serilog.Log.Warning("API endpoint down: {Name} ({Url})", endpoint.Name, endpoint.Url);
            });
        }
    }

    [RelayCommand]
    private void AddEndpoint()
    {
        var newEndpoint = new ApiEndpoint
        {
            Name = "New API",
            Url = "https://api.example.com/health",
            StatusMessage = "Not checked yet"
        };
        Endpoints.Add(newEndpoint);
        SelectedEndpoint = newEndpoint;
    }

    [RelayCommand]
    private void RemoveEndpoint(ApiEndpoint? endpoint)
    {
        if (endpoint != null && Endpoints.Contains(endpoint))
        {
            Endpoints.Remove(endpoint);
            SaveSettings();
        }
    }

    [RelayCommand]
    private async Task CheckNow()
    {
        await CheckAllEndpoints();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // Settings dialog will be opened from the window
    }

    public void Dispose()
    {
        _checkTimer?.Stop();
        _httpClient?.Dispose();
    }
}
