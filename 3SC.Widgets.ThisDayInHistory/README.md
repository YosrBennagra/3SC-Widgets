# 📜 This Day in History Widget

A desktop widget that displays significant historical events, births, and deaths that occurred on the current date throughout history. Perfect for history enthusiasts and learners who want to discover fascinating facts while working.

## Features

- **Daily Historical Events**: Shows 5 random events that happened on today's date
- **Categorized Events**: 
  - 📅 Historical Events
  - 🎂 Famous Births
  - 🕯️ Notable Deaths
- **Wikipedia Integration**: Direct links to Wikipedia for deep dives into each event
- **Share Functionality**: Copy individual events or all events to clipboard
- **Clean Design**: Simple, minimalist interface matching the 3SC design language
- **Refresh Button**: Get a new random selection of events for the day

## Widget Details

- **Category**: Information & Education
- **Default Size**: 400×550 pixels
- **Resizable**: Yes
- **Has Settings**: No
- **Requires Internet**: No (built-in historical database)

## How to Use

1. **View Today's Events**: The widget automatically loads events for the current date
2. **Learn More**: Click the "🔗 Wikipedia" link on any event to read more
3. **Share**: Click "📤 Share" to copy an event to your clipboard
4. **Refresh**: Click the 🔄 button to get a different random selection of events
5. **Copy All**: Click "📋 Copy All" in the footer to copy all displayed events

## Historical Database

The widget includes a curated database of significant events spanning from ancient times to the modern era, including:

- Major historical events (wars, treaties, discoveries)
- Famous births and deaths
- Scientific breakthroughs
- Cultural milestones
- Political events
- And much more!

## Examples

**January 20**
- 1649: King Charles I of England goes on trial for treason
- 1920: Federico Fellini, Italian film director, born
- 1981: Iran releases 52 American hostages
- 2009: Barack Obama inaugurated as the first African American President

**July 20**
- 1969: Apollo 11 astronauts Neil Armstrong and Buzz Aldrin become the first humans to walk on the Moon
- 1944: Claus von Stauffenberg attempts to assassinate Adolf Hitler

**September 11**
- 2001: Terrorist attacks on the World Trade Center and Pentagon

## Technical Details

- **Framework**: .NET 8.0 (Windows)
- **UI**: WPF with MVVM pattern
- **Dependencies**: 
  - CommunityToolkit.Mvvm 8.2.2
  - Serilog 3.1.1
- **Widget Key**: `this-day-in-history`

## Building

```powershell
# Build for release
dotnet build -c Release

# Package as .3scwidget
.\Build-And-Package-ThisDayInHistory.ps1
```

## Installation

1. Download the `.3scwidget` file
2. Double-click to install via 3SC host application
3. Or manually extract to `%APPDATA%\3SC\Widgets\Community\this-day-in-history\`

## Educational Value

This widget serves as a:
- Daily reminder of historical significance
- Learning tool for history enthusiasts
- Conversation starter
- Way to contextualize current events within historical patterns
- Gateway to deeper historical research via Wikipedia

## Accuracy Note

The historical events are curated from reliable sources, but we recommend verifying important facts through the Wikipedia links provided. Historical dates, especially for events before the Gregorian calendar (1582), should be considered approximate.

## Future Enhancements

Potential features for future versions:
- [ ] Filter events by category
- [ ] Search for specific dates
- [ ] Save favorite events
- [ ] Daily notifications
- [ ] More detailed event information
- [ ] Images for major events
- [ ] Audio narration option
- [ ] Quiz mode to test historical knowledge

## License

Part of the 3SC Widget Ecosystem. See main repository for licensing details.

## Credits

Historical data sourced from public domain resources and Wikipedia.

---

**Made with ❤️ for history enthusiasts**
