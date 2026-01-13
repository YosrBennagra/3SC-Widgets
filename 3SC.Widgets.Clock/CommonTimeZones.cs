using System.Collections.Immutable;

namespace _3SC.Data;

/// <summary>
/// Represents a timezone for display in UI.
/// </summary>
public class TimeZoneDisplay(string id, string shortName, string displayName)
{
    public string Id { get; init; } = id;
    public string ShortName { get; init; } = shortName;
    public string DisplayName { get; init; } = displayName;
    public string FullName { get; init; } = $"({shortName}) {displayName}";
}

/// <summary>
/// Common timezones for quick selection.
/// </summary>
public static class CommonTimeZones
{
    public static readonly ImmutableList<TimeZoneDisplay> Zones = ImmutableList.Create(
        // North America
        new TimeZoneDisplay("Eastern Standard Time", "EST", "Eastern Time (US & Canada)"),
        new TimeZoneDisplay("Central Standard Time", "CST", "Central Time (US & Canada)"),
        new TimeZoneDisplay("Mountain Standard Time", "MST", "Mountain Time (US & Canada)"),
        new TimeZoneDisplay("Pacific Standard Time", "PST", "Pacific Time (US & Canada)"),
        new TimeZoneDisplay("Alaskan Standard Time", "AKST", "Alaska"),
        new TimeZoneDisplay("Hawaiian Standard Time", "HST", "Hawaii"),

        // Europe
        new TimeZoneDisplay("GMT Standard Time", "GMT", "Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London"),
        new TimeZoneDisplay("Central European Standard Time", "CET", "Central European Time"),
        new TimeZoneDisplay("W. Europe Standard Time", "WET", "Western European Time"),
        new TimeZoneDisplay("E. Europe Standard Time", "EET", "Eastern European Time"),

        // Asia
        new TimeZoneDisplay("China Standard Time", "CST", "Beijing, Chongqing, Hong Kong, Urumqi"),
        new TimeZoneDisplay("Tokyo Standard Time", "JST", "Japan Standard Time"),
        new TimeZoneDisplay("India Standard Time", "IST", "India Standard Time"),
        new TimeZoneDisplay("Singapore Standard Time", "SGT", "Singapore"),
        new TimeZoneDisplay("Korea Standard Time", "KST", "Seoul"),

        // Australia
        new TimeZoneDisplay("AUS Eastern Standard Time", "AEST", "Australian Eastern Time"),
        new TimeZoneDisplay("AUS Central Standard Time", "ACST", "Australian Central Time"),
        new TimeZoneDisplay("W. Australia Standard Time", "AWST", "Australian Western Time"),

        // UTC
        new TimeZoneDisplay("UTC", "UTC", "Coordinated Universal Time")
    );

    /// <summary>
    /// Gets all system timezones (includes all available timezones on the system).
    /// </summary>
    public static List<TimeZoneDisplay> GetAllTimeZones()
    {
        var allZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new TimeZoneDisplay(
                tz.Id,
                tz.DisplayName.Contains('(') && tz.DisplayName.Contains(')')
                    ? tz.DisplayName.Substring(tz.DisplayName.IndexOf('(', StringComparison.Ordinal) + 1, tz.DisplayName.IndexOf(')', StringComparison.Ordinal) - tz.DisplayName.IndexOf('(', StringComparison.Ordinal) - 1)
                    : tz.StandardName,
                tz.DisplayName))
            .OrderBy(z => z.DisplayName)
            .ToList();

        return allZones;
    }
}
