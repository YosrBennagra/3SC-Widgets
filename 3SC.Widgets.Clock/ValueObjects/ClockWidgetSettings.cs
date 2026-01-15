namespace _3SC.Domain.ValueObjects;

/// <summary>
/// Settings for the Clock widget.
/// </summary>
public class ClockWidgetSettings
{
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;
    public bool Use24HourFormat { get; set; } = false;
    public bool ShowSeconds { get; set; } = true;
    public bool ShowTimeZoneLabel { get; set; } = true;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ClockWidgetSettings()
    {
    }

    /// <summary>
    /// Constructor with all properties.
    /// </summary>
    public ClockWidgetSettings(string timeZoneId, bool use24HourFormat, bool showSeconds, bool showTimeZoneLabel)
    {
        TimeZoneId = timeZoneId;
        Use24HourFormat = use24HourFormat;
        ShowSeconds = showSeconds;
        ShowTimeZoneLabel = showTimeZoneLabel;
    }

    /// <summary>
    /// Creates default settings with local timezone.
    /// </summary>
    public static ClockWidgetSettings Default()
    {
        return new ClockWidgetSettings
        {
            TimeZoneId = TimeZoneInfo.Local.Id,
            Use24HourFormat = false,
            ShowSeconds = true,
            ShowTimeZoneLabel = true
        };
    }
}
