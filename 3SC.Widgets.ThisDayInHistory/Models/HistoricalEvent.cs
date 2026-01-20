using System;

namespace _3SC.Widgets.ThisDayInHistory.Models;

public class HistoricalEvent
{
    public int Year { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Event, Birth, Death
    public string WikipediaUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public string YearDisplay => Year < 0 ? $"{Math.Abs(Year)} BCE" : Year.ToString();

    public string CategoryEmoji => Category switch
    {
        "Birth" => "🎂",
        "Death" => "🕯️",
        _ => "📅"
    };
}
