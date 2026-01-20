using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace _3SC.Widgets.CronExpressionBuilder;

public partial class CronExpressionBuilderViewModel : ObservableObject
{
    #region Fields & Properties

    [ObservableProperty]
    private string _cronExpression = "* * * * *";

    [ObservableProperty]
    private string _humanReadable = "Every minute";

    [ObservableProperty]
    private ObservableCollection<string> _nextRunTimes = new();

    [ObservableProperty]
    private string _quartzExpression = "0 * * ? * *";

    [ObservableProperty]
    private bool _isQuartzFormat;

    // Minute dropdowns (0-59)
    [ObservableProperty]
    private string _selectedMinute = "*";

    // Hour dropdowns (0-23)
    [ObservableProperty]
    private string _selectedHour = "*";

    // Day of month (1-31)
    [ObservableProperty]
    private string _selectedDayOfMonth = "*";

    // Month (1-12)
    [ObservableProperty]
    private string _selectedMonth = "*";

    // Day of week (0-6, Sunday=0)
    [ObservableProperty]
    private string _selectedDayOfWeek = "*";

    public ObservableCollection<string> MinuteOptions { get; } = new();
    public ObservableCollection<string> HourOptions { get; } = new();
    public ObservableCollection<string> DayOfMonthOptions { get; } = new();
    public ObservableCollection<string> MonthOptions { get; } = new();
    public ObservableCollection<string> DayOfWeekOptions { get; } = new();

    public ObservableCollection<CronPattern> CommonPatterns { get; } = new();

    #endregion

    public CronExpressionBuilderViewModel()
    {
        InitializeDropdowns();
        InitializeCommonPatterns();
        UpdateExpression();
    }

    #region Initialization

    private void InitializeDropdowns()
    {
        // Minutes
        MinuteOptions.Add("*");
        MinuteOptions.Add("*/5");
        MinuteOptions.Add("*/10");
        MinuteOptions.Add("*/15");
        MinuteOptions.Add("*/30");
        for (int i = 0; i < 60; i++)
            MinuteOptions.Add(i.ToString());

        // Hours
        HourOptions.Add("*");
        HourOptions.Add("*/2");
        HourOptions.Add("*/3");
        HourOptions.Add("*/6");
        HourOptions.Add("*/12");
        for (int i = 0; i < 24; i++)
            HourOptions.Add(i.ToString());

        // Day of Month
        DayOfMonthOptions.Add("*");
        DayOfMonthOptions.Add("*/2");
        DayOfMonthOptions.Add("*/3");
        DayOfMonthOptions.Add("1,15");
        for (int i = 1; i <= 31; i++)
            DayOfMonthOptions.Add(i.ToString());

        // Month
        MonthOptions.Add("*");
        MonthOptions.Add("1");  // January
        MonthOptions.Add("2");  // February
        MonthOptions.Add("3");  // March
        MonthOptions.Add("4");  // April
        MonthOptions.Add("5");  // May
        MonthOptions.Add("6");  // June
        MonthOptions.Add("7");  // July
        MonthOptions.Add("8");  // August
        MonthOptions.Add("9");  // September
        MonthOptions.Add("10"); // October
        MonthOptions.Add("11"); // November
        MonthOptions.Add("12"); // December
        MonthOptions.Add("*/3"); // Quarterly
        MonthOptions.Add("*/6"); // Semi-annually

        // Day of Week
        DayOfWeekOptions.Add("*");
        DayOfWeekOptions.Add("0"); // Sunday
        DayOfWeekOptions.Add("1"); // Monday
        DayOfWeekOptions.Add("2"); // Tuesday
        DayOfWeekOptions.Add("3"); // Wednesday
        DayOfWeekOptions.Add("4"); // Thursday
        DayOfWeekOptions.Add("5"); // Friday
        DayOfWeekOptions.Add("6"); // Saturday
        DayOfWeekOptions.Add("1-5"); // Weekdays
        DayOfWeekOptions.Add("0,6"); // Weekends
    }

    private void InitializeCommonPatterns()
    {
        CommonPatterns.Add(new CronPattern("Every minute", "* * * * *"));
        CommonPatterns.Add(new CronPattern("Every 5 minutes", "*/5 * * * *"));
        CommonPatterns.Add(new CronPattern("Every 15 minutes", "*/15 * * * *"));
        CommonPatterns.Add(new CronPattern("Every 30 minutes", "*/30 * * * *"));
        CommonPatterns.Add(new CronPattern("Every hour", "0 * * * *"));
        CommonPatterns.Add(new CronPattern("Every 6 hours", "0 */6 * * *"));
        CommonPatterns.Add(new CronPattern("Every day at midnight", "0 0 * * *"));
        CommonPatterns.Add(new CronPattern("Every day at noon", "0 12 * * *"));
        CommonPatterns.Add(new CronPattern("Every weekday at 9 AM", "0 9 * * 1-5"));
        CommonPatterns.Add(new CronPattern("Every Monday at 9 AM", "0 9 * * 1"));
        CommonPatterns.Add(new CronPattern("First day of month at midnight", "0 0 1 * *"));
        CommonPatterns.Add(new CronPattern("Every Sunday at midnight", "0 0 * * 0"));
        CommonPatterns.Add(new CronPattern("Twice daily (9 AM & 5 PM)", "0 9,17 * * *"));
    }

    #endregion

    #region Property Change Handlers

    partial void OnSelectedMinuteChanged(string value) => UpdateExpression();
    partial void OnSelectedHourChanged(string value) => UpdateExpression();
    partial void OnSelectedDayOfMonthChanged(string value) => UpdateExpression();
    partial void OnSelectedMonthChanged(string value) => UpdateExpression();
    partial void OnSelectedDayOfWeekChanged(string value) => UpdateExpression();

    #endregion

    #region Commands

    [RelayCommand]
    private void CopyExpression()
    {
        try
        {
            var textToCopy = IsQuartzFormat ? QuartzExpression : CronExpression;
            Clipboard.SetText(textToCopy);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to copy expression to clipboard");
        }
    }

    [RelayCommand]
    private void ApplyPattern(CronPattern pattern)
    {
        if (pattern == null) return;

        var parts = pattern.Expression.Split(' ');
        if (parts.Length >= 5)
        {
            SelectedMinute = parts[0];
            SelectedHour = parts[1];
            SelectedDayOfMonth = parts[2];
            SelectedMonth = parts[3];
            SelectedDayOfWeek = parts[4];
        }
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedMinute = "*";
        SelectedHour = "*";
        SelectedDayOfMonth = "*";
        SelectedMonth = "*";
        SelectedDayOfWeek = "*";
    }

    #endregion

    #region Cron Expression Logic

    private void UpdateExpression()
    {
        // Build Unix cron expression (5 fields)
        CronExpression = $"{SelectedMinute} {SelectedHour} {SelectedDayOfMonth} {SelectedMonth} {SelectedDayOfWeek}";

        // Build Quartz cron expression (6 fields - adds seconds at start)
        QuartzExpression = $"0 {SelectedMinute} {SelectedHour} {SelectedDayOfMonth} {SelectedMonth} {SelectedDayOfWeek}";

        // Generate human-readable description
        HumanReadable = GenerateHumanReadable();

        // Calculate next run times
        CalculateNextRunTimes();
    }

    private string GenerateHumanReadable()
    {
        try
        {
            var parts = new List<string>();

            // Minute
            if (SelectedMinute == "*")
                parts.Add("every minute");
            else if (SelectedMinute.StartsWith("*/"))
                parts.Add($"every {SelectedMinute.Substring(2)} minutes");
            else
                parts.Add($"at minute {SelectedMinute}");

            // Hour
            if (SelectedHour != "*")
            {
                if (SelectedHour.StartsWith("*/"))
                    parts.Add($"every {SelectedHour.Substring(2)} hours");
                else
                {
                    var hour = int.Parse(SelectedHour);
                    var ampm = hour >= 12 ? "PM" : "AM";
                    var displayHour = hour == 0 ? 12 : (hour > 12 ? hour - 12 : hour);
                    parts.Add($"at {displayHour} {ampm}");
                }
            }

            // Day of month
            if (SelectedDayOfMonth != "*")
            {
                if (SelectedDayOfMonth.StartsWith("*/"))
                    parts.Add($"every {SelectedDayOfMonth.Substring(2)} days");
                else if (SelectedDayOfMonth == "1")
                    parts.Add("on the 1st");
                else if (SelectedDayOfMonth.Contains(","))
                    parts.Add($"on days {SelectedDayOfMonth}");
                else
                    parts.Add($"on day {SelectedDayOfMonth}");
            }

            // Month
            if (SelectedMonth != "*")
            {
                var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                if (SelectedMonth.StartsWith("*/"))
                    parts.Add($"every {SelectedMonth.Substring(2)} months");
                else if (int.TryParse(SelectedMonth, out int monthNum) && monthNum >= 1 && monthNum <= 12)
                    parts.Add($"in {monthNames[monthNum - 1]}");
            }

            // Day of week
            if (SelectedDayOfWeek != "*")
            {
                var dayNames = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                if (SelectedDayOfWeek == "1-5")
                    parts.Add("on weekdays");
                else if (SelectedDayOfWeek == "0,6")
                    parts.Add("on weekends");
                else if (int.TryParse(SelectedDayOfWeek, out int dayNum) && dayNum >= 0 && dayNum <= 6)
                    parts.Add($"on {dayNames[dayNum]}");
            }

            return string.Join(", ", parts).Trim();
        }
        catch
        {
            return "Invalid expression";
        }
    }

    private void CalculateNextRunTimes()
    {
        NextRunTimes.Clear();

        try
        {
            var expression = Cronos.CronExpression.Parse(CronExpression, CronFormat.Standard);
            var now = DateTime.UtcNow;
            var nextOccurrences = expression.GetOccurrences(now, now.AddDays(30), TimeZoneInfo.Local, true).Take(5);

            foreach (var occurrence in nextOccurrences)
            {
                NextRunTimes.Add(occurrence.ToLocalTime().ToString("ddd, MMM dd yyyy HH:mm:ss"));
            }

            if (NextRunTimes.Count == 0)
            {
                NextRunTimes.Add("No upcoming runs in next 30 days");
            }
        }
        catch (Exception ex)
        {
            NextRunTimes.Add($"Invalid expression: {ex.Message}");
            Serilog.Log.Warning(ex, "Failed to calculate next run times for expression: {Expression}", CronExpression);
        }
    }

    #endregion
}

public class CronPattern
{
    public string Name { get; set; }
    public string Expression { get; set; }

    public CronPattern(string name, string expression)
    {
        Name = name;
        Expression = expression;
    }
}
