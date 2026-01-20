using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _3SC.Widgets.ApiStatus;

public partial class ApiEndpoint : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private bool _isOnline;

    [ObservableProperty]
    private double _responseTime;

    [ObservableProperty]
    private DateTime _lastChecked;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalChecks;

    [ObservableProperty]
    private int _successfulChecks;

    [ObservableProperty]
    private double _uptimePercentage;

    public List<ResponseTimeData> ResponseHistory { get; } = new();

    public void AddResponseTime(double responseTime, bool success)
    {
        TotalChecks++;
        if (success)
        {
            SuccessfulChecks++;
        }

        UptimePercentage = TotalChecks > 0
            ? Math.Round((double)SuccessfulChecks / TotalChecks * 100, 2)
            : 0;

        ResponseHistory.Add(new ResponseTimeData
        {
            Timestamp = DateTime.Now,
            ResponseTime = responseTime,
            Success = success
        });

        // Keep only last 50 data points for the graph
        if (ResponseHistory.Count > 50)
        {
            ResponseHistory.RemoveAt(0);
        }
    }

    public double GetAverageResponseTime()
    {
        var successfulResponses = ResponseHistory.Where(r => r.Success).ToList();
        return successfulResponses.Any()
            ? Math.Round(successfulResponses.Average(r => r.ResponseTime), 2)
            : 0;
    }
}

public class ResponseTimeData
{
    public DateTime Timestamp { get; set; }
    public double ResponseTime { get; set; }
    public bool Success { get; set; }
}
