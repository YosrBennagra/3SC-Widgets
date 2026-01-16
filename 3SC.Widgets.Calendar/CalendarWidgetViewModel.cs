using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.Calendar;

public partial class CalendarWidgetViewModel : ObservableObject, IDisposable
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CalendarWidgetViewModel>();

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _displayMonth = DateTime.Today;

    [ObservableProperty]
    private string _monthYearDisplay = string.Empty;

    [ObservableProperty]
    private string _todayDisplay = string.Empty;

    [ObservableProperty]
    private string _selectedDateLong = string.Empty;

    [ObservableProperty]
    private int _weekOfYear;

    [ObservableProperty]
    private int _dayOfYear;

    [ObservableProperty]
    private ObservableCollection<DayViewModel> _days = new();

    private bool _disposed;

    public CalendarWidgetViewModel()
    {
        UpdateDisplay();
        GenerateCalendar();
        Log.Debug("CalendarWidgetViewModel initialized, displaying {Month}", MonthYearDisplay);
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateDisplay();
        Log.Debug("Selected date changed to {Date}", value.ToShortDateString());

        foreach (var day in Days)
        {
            day.IsSelected = day.Date.Date == value.Date;
        }
    }

    partial void OnDisplayMonthChanged(DateTime value)
    {
        GenerateCalendar();
        Log.Debug("Display month changed to {Month}", value.ToString("MMMM yyyy"));
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        DisplayMonth = DisplayMonth.AddMonths(-1);
    }

    [RelayCommand]
    private void NextMonth()
    {
        DisplayMonth = DisplayMonth.AddMonths(1);
    }

    [RelayCommand]
    private void Today()
    {
        SelectedDate = DateTime.Today;
        DisplayMonth = DateTime.Today;
        Log.Debug("Navigated to today");
    }

    private void UpdateDisplay()
    {
        MonthYearDisplay = DisplayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        TodayDisplay = DateTime.Today.ToString("dddd, MMMM d, yyyy", CultureInfo.CurrentCulture);
        SelectedDateLong = SelectedDate.ToString("dddd, MMMM d", CultureInfo.CurrentCulture);

        var calendar = CultureInfo.CurrentCulture.Calendar;
        WeekOfYear = calendar.GetWeekOfYear(SelectedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        DayOfYear = SelectedDate.DayOfYear;
    }

    private void GenerateCalendar()
    {
        Days.Clear();

        var firstDayOfMonth = new DateTime(DisplayMonth.Year, DisplayMonth.Month, 1, 0, 0, 0, DateTimeKind.Local);

        var dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var startDate = firstDayOfMonth.AddDays(-daysToSubtract);

        for (int i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i);
            Days.Add(new DayViewModel
            {
                Date = date,
                Day = date.Day,
                IsCurrentMonth = date.Month == DisplayMonth.Month,
                IsToday = date.Date == DateTime.Today,
                IsSelected = date.Date == SelectedDate.Date,
                IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                SelectCommand = new RelayCommand(() => SelectedDate = date)
            });
        }

        UpdateDisplay();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Days.Clear();
            Log.Debug("CalendarWidgetViewModel disposed");
        }

        _disposed = true;
    }
}

public partial class DayViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private int _day;

    [ObservableProperty]
    private bool _isCurrentMonth;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isWeekend;

    public IRelayCommand? SelectCommand { get; set; }
}
