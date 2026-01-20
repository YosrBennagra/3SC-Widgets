using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using _3SC.Widgets.ThisDayInHistory.Data;
using _3SC.Widgets.ThisDayInHistory.Models;

namespace _3SC.Widgets.ThisDayInHistory;

public partial class ThisDayInHistoryViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<ThisDayInHistoryViewModel>();

    [ObservableProperty]
    private string _todayDate = string.Empty;

    [ObservableProperty]
    private ObservableCollection<HistoricalEvent> _events = new();

    [ObservableProperty]
    private string _eventCount = string.Empty;

    [ObservableProperty]
    private HistoricalEvent? _selectedEvent;

    private readonly Random _random = new();

    public ThisDayInHistoryViewModel()
    {
        Logger.Information("This Day in History ViewModel initialized");
        LoadTodaysEvents();
    }

    private void LoadTodaysEvents()
    {
        try
        {
            var today = DateTime.Now;
            TodayDate = today.ToString("MMMM dd, yyyy");

            var events = HistoricalDatabase.GetEventsForDate(today);

            Events.Clear();
            foreach (var evt in events)
            {
                Events.Add(evt);
            }

            EventCount = $"{Events.Count} Historical Events";

            Logger.Information("Loaded {Count} events for {Date}", Events.Count, today.ToShortDateString());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load historical events");
        }
    }

    [RelayCommand]
    private void RefreshEvents()
    {
        Logger.Information("Refreshing events");
        LoadTodaysEvents();
    }

    [RelayCommand]
    private void OpenWikipedia(HistoricalEvent? evt)
    {
        if (evt == null || string.IsNullOrEmpty(evt.WikipediaUrl))
        {
            return;
        }

        try
        {
            Logger.Information("Opening Wikipedia: {Url}", evt.WikipediaUrl);
            Process.Start(new ProcessStartInfo
            {
                FileName = evt.WikipediaUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open Wikipedia link");
            MessageBox.Show("Failed to open Wikipedia link", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ShareEvent(HistoricalEvent? evt)
    {
        if (evt == null)
        {
            return;
        }

        try
        {
            var shareText = $"📜 This Day in History ({evt.YearDisplay}):\n{evt.Description}";
            if (!string.IsNullOrEmpty(evt.WikipediaUrl))
            {
                shareText += $"\n\nLearn more: {evt.WikipediaUrl}";
            }

            Clipboard.SetText(shareText);
            Logger.Information("Shared event to clipboard");
            MessageBox.Show("Event copied to clipboard!", "Shared", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to share event");
        }
    }

    [RelayCommand]
    private void OpenRandomWikipedia()
    {
        try
        {
            var eventsWithLinks = Events.Where(e => !string.IsNullOrEmpty(e.WikipediaUrl)).ToList();
            if (eventsWithLinks.Any())
            {
                var randomEvent = eventsWithLinks[_random.Next(eventsWithLinks.Count)];
                OpenWikipedia(randomEvent);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open random Wikipedia");
        }
    }

    [RelayCommand]
    private void CopyAllEvents()
    {
        try
        {
            var allText = $"📜 This Day in History - {TodayDate}\n\n";

            foreach (var evt in Events)
            {
                allText += $"{evt.CategoryEmoji} {evt.YearDisplay}: {evt.Description}\n";
                if (!string.IsNullOrEmpty(evt.WikipediaUrl))
                {
                    allText += $"   {evt.WikipediaUrl}\n";
                }
                allText += "\n";
            }

            Clipboard.SetText(allText);
            Logger.Information("Copied all events to clipboard");
            MessageBox.Show("All events copied to clipboard!", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy all events");
        }
    }
}
