using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Windows.Threading;
using _3SC.Widgets.MoonPhase.Helpers;

namespace _3SC.Widgets.MoonPhase;

public partial class MoonPhaseViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<MoonPhaseViewModel>();
    private readonly DispatcherTimer _updateTimer;

    [ObservableProperty]
    private string _phaseName = "";

    [ObservableProperty]
    private string _phaseEmoji = "🌕";

    [ObservableProperty]
    private double _illuminationPercentage;

    [ObservableProperty]
    private double _moonPhase; // 0-1 value

    [ObservableProperty]
    private string _waxingOrWaning = "";

    [ObservableProperty]
    private string _folkloreAdvice = "";

    [ObservableProperty]
    private string _plantingAdvice = "";

    [ObservableProperty]
    private string _fishingAdvice = "";

    [ObservableProperty]
    private string _haircutAdvice = "";

    [ObservableProperty]
    private int _daysUntilFullMoon;

    [ObservableProperty]
    private int _daysUntilNewMoon;

    [ObservableProperty]
    private string _nextFullMoonDate = "";

    [ObservableProperty]
    private string _nextNewMoonDate = "";

    [ObservableProperty]
    private DateTime _currentDate;

    [ObservableProperty]
    private string _currentDateString = "";

    [ObservableProperty]
    private string _currentTimeString = "";

    public MoonPhaseViewModel()
    {
        Logger.Information("Moon Phase ViewModel initialized");

        CurrentDate = DateTime.Now;
        UpdateMoonData();

        // Update every minute
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(60)
        };
        _updateTimer.Tick += (s, e) => UpdateMoonData();
        _updateTimer.Start();
    }

    private void UpdateMoonData()
    {
        try
        {
            CurrentDate = DateTime.Now;
            CurrentDateString = CurrentDate.ToString("dddd, MMMM dd, yyyy");
            CurrentTimeString = CurrentDate.ToString("hh:mm tt");

            // Calculate moon phase
            MoonPhase = MoonCalculator.GetMoonPhase(CurrentDate);
            PhaseName = MoonCalculator.GetPhaseName(CurrentDate);
            PhaseEmoji = MoonCalculator.GetPhaseEmoji(CurrentDate);
            IlluminationPercentage = MoonCalculator.GetIlluminationPercentage(CurrentDate);

            // Waxing or waning
            bool isWaxing = MoonCalculator.IsWaxing(CurrentDate);
            WaxingOrWaning = isWaxing ? "Waxing (Growing)" : "Waning (Shrinking)";

            // Countdown
            DaysUntilFullMoon = MoonCalculator.DaysUntilFullMoon(CurrentDate);
            DaysUntilNewMoon = MoonCalculator.DaysUntilNewMoon(CurrentDate);

            DateTime nextFull = MoonCalculator.GetNextFullMoon(CurrentDate);
            DateTime nextNew = MoonCalculator.GetNextNewMoon(CurrentDate);

            NextFullMoonDate = nextFull.ToString("MMM dd, yyyy");
            NextNewMoonDate = nextNew.ToString("MMM dd, yyyy");

            // Folklore
            FolkloreAdvice = MoonCalculator.GetFolkloreAdvice(CurrentDate);

            var bestDays = MoonCalculator.GetBestDays(CurrentDate);
            PlantingAdvice = bestDays.planting;
            FishingAdvice = bestDays.fishing;
            HaircutAdvice = bestDays.haircuts;

            Logger.Debug("Moon data updated: {Phase} - {Illumination}%", PhaseName, IlluminationPercentage);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to update moon data");
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        Logger.Information("Manual refresh triggered");
        UpdateMoonData();
    }

    [RelayCommand]
    private void ViewLunarCalendar()
    {
        Logger.Information("Lunar calendar requested");
        // Future enhancement: Show full lunar calendar window
    }
}
