using System;
using System.Collections.Generic;
using System.Linq;
using _3SC.Widgets.ThisDayInHistory.Models;

namespace _3SC.Widgets.ThisDayInHistory.Data;

public static class HistoricalDatabase
{
    private static readonly Dictionary<(int Month, int Day), List<HistoricalEvent>> _events = new();

    static HistoricalDatabase()
    {
        PopulateDatabase();
    }

    public static List<HistoricalEvent> GetEventsForDate(DateTime date)
    {
        var key = (date.Month, date.Day);
        if (_events.TryGetValue(key, out var events))
        {
            return events.OrderBy(_ => Random.Shared.Next()).Take(5).ToList();
        }
        return new List<HistoricalEvent>();
    }

    private static void AddEvent(int month, int day, int year, string description, string category = "Event", string wikipediaPath = "")
    {
        var key = (month, day);
        if (!_events.ContainsKey(key))
        {
            _events[key] = new List<HistoricalEvent>();
        }

        _events[key].Add(new HistoricalEvent
        {
            Year = year,
            Description = description,
            Category = category,
            WikipediaUrl = string.IsNullOrEmpty(wikipediaPath) ? "" : $"https://en.wikipedia.org/wiki/{wikipediaPath}"
        });
    }

    private static void PopulateDatabase()
    {
        // January 20
        AddEvent(1, 20, 1265, "First English parliament called into session by the Earl of Leicester", "Event", "Simon_de_Montfort's_Parliament");
        AddEvent(1, 20, 1649, "Trial of King Charles I of England for high treason and other high crimes began", "Event", "High_Court_of_Justice_for_the_trial_of_Charles_I");
        AddEvent(1, 20, 1783, "American Revolutionary War: Preliminary articles of peace ending the war signed in Paris", "Event", "Treaty_of_Paris_(1783)");
        AddEvent(1, 20, 1887, "The United States Senate approves the lease of Pearl Harbor to the Navy", "Event", "Pearl_Harbor");
        AddEvent(1, 20, 1936, "King George V of the United Kingdom dies; Edward VIII succeeds to the throne", "Event", "Edward_VIII");
        AddEvent(1, 20, 1942, "Nazi officials hold the Wannsee Conference in Berlin to organize the \"Final Solution\"", "Event", "Wannsee_Conference");
        AddEvent(1, 20, 1981, "Iran releases 52 American hostages twenty minutes after Ronald Reagan is inaugurated", "Event", "Iran_hostage_crisis");
        AddEvent(1, 20, 2009, "Barack Obama is inaugurated as the 44th President of the United States", "Event", "First_inauguration_of_Barack_Obama");
        AddEvent(1, 20, 2017, "Donald Trump is inaugurated as the 45th President of the United States", "Event", "Inauguration_of_Donald_Trump");

        AddEvent(1, 20, 1920, "Federico Fellini, Italian director and screenwriter (d. 1993)", "Birth", "Federico_Fellini");
        AddEvent(1, 20, 1946, "David Lynch, American director, producer, and screenwriter", "Birth", "David_Lynch");
        AddEvent(1, 20, 1971, "Gary Barlow, English singer-songwriter and producer (Take That)", "Birth", "Gary_Barlow");

        AddEvent(1, 20, 1993, "Audrey Hepburn, British-Dutch actress and humanitarian (b. 1929)", "Death", "Audrey_Hepburn");
        AddEvent(1, 20, 2021, "Hank Aaron, American baseball player (b. 1934)", "Death", "Hank_Aaron");

        // January 1
        AddEvent(1, 1, 1863, "The Emancipation Proclamation takes effect in Confederate territory", "Event", "Emancipation_Proclamation");
        AddEvent(1, 1, 1901, "The Commonwealth of Australia is formed", "Event", "Federation_of_Australia");
        AddEvent(1, 1, 1959, "Fidel Castro leads the Cuban Revolution to victory", "Event", "Cuban_Revolution");
        AddEvent(1, 1, 1984, "Brunei becomes independent from the United Kingdom", "Event", "Brunei");
        AddEvent(1, 1, 2002, "Euro banknotes and coins become legal tender in 12 European countries", "Event", "Euro");

        AddEvent(1, 1, 1879, "E. M. Forster, English novelist (d. 1970)", "Birth", "E._M._Forster");
        AddEvent(1, 1, 1919, "J. D. Salinger, American novelist (d. 2010)", "Birth", "J._D._Salinger");

        // December 25
        AddEvent(12, 25, 800, "Charlemagne is crowned the first Holy Roman Emperor by Pope Leo III", "Event", "Charlemagne");
        AddEvent(12, 25, 1066, "William the Conqueror is crowned King of England at Westminster Abbey", "Event", "William_the_Conqueror");
        AddEvent(12, 25, 1776, "George Washington crosses the Delaware River to attack Hessian forces", "Event", "George_Washington's_crossing_of_the_Delaware_River");
        AddEvent(12, 25, 1991, "Mikhail Gorbachev resigns as president of the Soviet Union", "Event", "Dissolution_of_the_Soviet_Union");

        AddEvent(12, 25, 1642, "Isaac Newton, English mathematician and physicist (d. 1727)", "Birth", "Isaac_Newton");
        AddEvent(12, 25, 1899, "Humphrey Bogart, American actor (d. 1957)", "Birth", "Humphrey_Bogart");

        // July 20
        AddEvent(7, 20, 1969, "Apollo 11: Neil Armstrong and Buzz Aldrin become the first humans to walk on the Moon", "Event", "Apollo_11");
        AddEvent(7, 20, 1944, "Claus von Stauffenberg fails to assassinate Adolf Hitler (July 20 plot)", "Event", "20_July_plot");

        // July 4
        AddEvent(7, 4, 1776, "United States Declaration of Independence adopted by Continental Congress", "Event", "United_States_Declaration_of_Independence");
        AddEvent(7, 4, 1826, "Both Thomas Jefferson and John Adams die on the 50th anniversary of the Declaration", "Event", "Thomas_Jefferson");

        // November 9
        AddEvent(11, 9, 1989, "The Berlin Wall falls, marking the symbolic end of the Cold War", "Event", "Fall_of_the_Berlin_Wall");

        // September 11
        AddEvent(9, 11, 2001, "Terrorist attacks in New York City, Washington D.C., and Pennsylvania", "Event", "September_11_attacks");

        // Add more days throughout the year with interesting events
        PopulateAdditionalDays();
    }

    private static void PopulateAdditionalDays()
    {
        // February 14
        AddEvent(2, 14, 1779, "Captain James Cook is killed in Hawaii", "Event", "James_Cook");
        AddEvent(2, 14, 1929, "Saint Valentine's Day Massacre in Chicago", "Event", "Saint_Valentine's_Day_Massacre");

        // March 15
        AddEvent(3, 15, -44, "Julius Caesar is assassinated by Roman senators", "Event", "Assassination_of_Julius_Caesar");

        // April 15
        AddEvent(4, 15, 1912, "RMS Titanic sinks in the North Atlantic", "Event", "Sinking_of_the_Titanic");
        AddEvent(4, 15, 1865, "Abraham Lincoln dies after being shot the previous evening", "Death", "Abraham_Lincoln");

        // May 29
        AddEvent(5, 29, 1953, "Edmund Hillary and Tenzing Norgay become the first to reach the summit of Mount Everest", "Event", "1953_British_Mount_Everest_expedition");

        // June 6
        AddEvent(6, 6, 1944, "D-Day: Allied forces invade Normandy in Operation Overlord", "Event", "Normandy_landings");

        // August 6
        AddEvent(8, 6, 1945, "Atomic bomb dropped on Hiroshima, Japan", "Event", "Atomic_bombings_of_Hiroshima_and_Nagasaki");

        // October 12
        AddEvent(10, 12, 1492, "Christopher Columbus's expedition makes landfall in the Caribbean", "Event", "Christopher_Columbus");

        // December 7
        AddEvent(12, 7, 1941, "Attack on Pearl Harbor brings the United States into World War II", "Event", "Attack_on_Pearl_Harbor");
    }
}
