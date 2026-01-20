using System;

namespace _3SC.Widgets.MoonPhase.Helpers;

/// <summary>
/// Calculates moon phases using astronomical algorithms
/// Based on Jean Meeus' "Astronomical Algorithms"
/// </summary>
public static class MoonCalculator
{
    private const double SynodicMonth = 29.530588861; // Average length of lunar month in days

    /// <summary>
    /// Calculate the current moon phase (0 = New Moon, 0.5 = Full Moon, 1 = New Moon)
    /// </summary>
    public static double GetMoonPhase(DateTime date)
    {
        // Calculate Julian Day Number
        double jd = ToJulianDay(date);

        // Known new moon: January 6, 2000, 18:14 UTC (JD 2451550.26)
        double knownNewMoon = 2451550.26;

        // Calculate days since known new moon
        double daysSinceNewMoon = jd - knownNewMoon;

        // Calculate phase (0-1, where 0 = new moon, 0.5 = full moon)
        double phase = (daysSinceNewMoon % SynodicMonth) / SynodicMonth;

        return phase;
    }

    /// <summary>
    /// Get the illumination percentage (0-100%)
    /// </summary>
    public static double GetIlluminationPercentage(DateTime date)
    {
        double phase = GetMoonPhase(date);

        // Convert phase to illumination
        // Phase 0 = 0%, Phase 0.5 = 100%, Phase 1 = 0%
        double illumination = (1 - Math.Abs(phase - 0.5) * 2) * 100;

        return Math.Round(illumination, 1);
    }

    /// <summary>
    /// Get the moon phase name
    /// </summary>
    public static string GetPhaseName(DateTime date)
    {
        double phase = GetMoonPhase(date);

        return phase switch
        {
            < 0.033 => "New Moon",
            < 0.216 => "Waxing Crescent",
            < 0.283 => "First Quarter",
            < 0.466 => "Waxing Gibbous",
            < 0.533 => "Full Moon",
            < 0.716 => "Waning Gibbous",
            < 0.783 => "Last Quarter",
            < 0.966 => "Waning Crescent",
            _ => "New Moon"
        };
    }

    /// <summary>
    /// Get emoji representation of moon phase
    /// </summary>
    public static string GetPhaseEmoji(DateTime date)
    {
        double phase = GetMoonPhase(date);

        return phase switch
        {
            < 0.033 => "🌑", // New Moon
            < 0.216 => "🌒", // Waxing Crescent
            < 0.283 => "🌓", // First Quarter
            < 0.466 => "🌔", // Waxing Gibbous
            < 0.533 => "🌕", // Full Moon
            < 0.716 => "🌖", // Waning Gibbous
            < 0.783 => "🌗", // Last Quarter
            < 0.966 => "🌘", // Waning Crescent
            _ => "🌑"
        };
    }

    /// <summary>
    /// Calculate days until next full moon
    /// </summary>
    public static int DaysUntilFullMoon(DateTime date)
    {
        double phase = GetMoonPhase(date);
        double daysToFull;

        if (phase < 0.5)
        {
            // Waxing - moving towards full moon
            daysToFull = (0.5 - phase) * SynodicMonth;
        }
        else
        {
            // Waning - next full moon is in next cycle
            daysToFull = (1.5 - phase) * SynodicMonth;
        }

        return (int)Math.Round(daysToFull);
    }

    /// <summary>
    /// Calculate days until next new moon
    /// </summary>
    public static int DaysUntilNewMoon(DateTime date)
    {
        double phase = GetMoonPhase(date);
        double daysToNew;

        if (phase < 1.0)
        {
            daysToNew = (1.0 - phase) * SynodicMonth;
        }
        else
        {
            daysToNew = 0;
        }

        return (int)Math.Round(daysToNew);
    }

    /// <summary>
    /// Get the date of the next full moon
    /// </summary>
    public static DateTime GetNextFullMoon(DateTime date)
    {
        int days = DaysUntilFullMoon(date);
        return date.AddDays(days);
    }

    /// <summary>
    /// Get the date of the next new moon
    /// </summary>
    public static DateTime GetNextNewMoon(DateTime date)
    {
        int days = DaysUntilNewMoon(date);
        return date.AddDays(days);
    }

    /// <summary>
    /// Check if the moon is waxing (growing)
    /// </summary>
    public static bool IsWaxing(DateTime date)
    {
        double phase = GetMoonPhase(date);
        return phase < 0.5;
    }

    /// <summary>
    /// Convert DateTime to Julian Day Number
    /// </summary>
    private static double ToJulianDay(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;
        double hour = date.Hour + (date.Minute / 60.0) + (date.Second / 3600.0);

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        int a = year / 100;
        int b = 2 - a + (a / 4);

        double jd = Math.Floor(365.25 * (year + 4716)) +
                    Math.Floor(30.6001 * (month + 1)) +
                    day + (hour / 24.0) + b - 1524.5;

        return jd;
    }

    /// <summary>
    /// Get folklore information about what's favorable based on moon phase
    /// </summary>
    public static string GetFolkloreAdvice(DateTime date)
    {
        double phase = GetMoonPhase(date);
        bool waxing = IsWaxing(date);
        string phaseName = GetPhaseName(date);

        if (phaseName == "New Moon")
        {
            return "Best for: New beginnings, planting root crops, setting intentions";
        }
        else if (phaseName == "Full Moon")
        {
            return "Best for: Harvesting, fishing (fish are more active), finishing projects";
        }
        else if (waxing)
        {
            return "Best for: Planting above-ground crops, starting new habits, growth activities";
        }
        else
        {
            return "Best for: Pruning, weeding, letting go of bad habits, introspection";
        }
    }

    /// <summary>
    /// Get specific best days information
    /// </summary>
    public static (string planting, string fishing, string haircuts) GetBestDays(DateTime date)
    {
        string phaseName = GetPhaseName(date);
        bool waxing = IsWaxing(date);

        string planting;
        string fishing;
        string haircuts;

        if (phaseName == "New Moon")
        {
            planting = "✅ Plant root vegetables";
            fishing = "⚠️ Moderate activity";
            haircuts = "✅ For slower growth";
        }
        else if (phaseName == "Full Moon")
        {
            planting = "✅ Harvest crops";
            fishing = "✅ Excellent!";
            haircuts = "✅ For faster growth";
        }
        else if (phaseName == "First Quarter" || phaseName == "Waxing Crescent")
        {
            planting = "✅ Leafy greens, flowering plants";
            fishing = "✓ Good morning fishing";
            haircuts = "✅ For thickness & volume";
        }
        else if (phaseName == "Last Quarter" || phaseName == "Waning Crescent")
        {
            planting = "✅ Root crops, perennials";
            fishing = "⚠️ Fair conditions";
            haircuts = "✅ For strength at roots";
        }
        else if (waxing)
        {
            planting = "✅ Above-ground crops";
            fishing = "✓ Good conditions";
            haircuts = "✅ For fast growth";
        }
        else
        {
            planting = "✅ Pruning, weeding";
            fishing = "⚠️ Moderate activity";
            haircuts = "✅ For slow, strong growth";
        }

        return (planting, fishing, haircuts);
    }
}
