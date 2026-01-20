# ⏲️ Cron Expression Builder Widget

Visual cron schedule creator for 3SC.

## Features

- **Visual Builder** - Create cron expressions using intuitive dropdown menus
- **Dual Format Support** - Generate both Unix Cron and Quartz expressions
- **Human-Readable Output** - See exactly what your expression means in plain English
- **Next Run Times Preview** - View the next 5 execution times for your schedule
- **Common Patterns Library** - Quick access to frequently used cron patterns
- **Copy to Clipboard** - One-click copy for easy integration

## Usage

### Building Expressions

Use the dropdown selectors to build your schedule:

- **Minute (0-59)** - When in the hour to run (supports *, */5, */10, etc.)
- **Hour (0-23)** - What hour of the day (supports *, */2, */6, etc.)
- **Day of Month (1-31)** - Which day of the month
- **Month (1-12)** - Which month(s) to run
- **Day of Week (0-6)** - Which day(s) of the week (0=Sunday)

### Common Patterns

Click any pattern in the "Common Patterns" library to instantly apply it:

- Every minute
- Every 5/15/30 minutes
- Every hour
- Daily at midnight/noon
- Weekday schedules
- Monthly schedules
- And more!

### Expression Formats

The widget generates two formats:

**Unix Cron** (5 fields):
```
* * * * *
│ │ │ │ │
│ │ │ │ └─── Day of week (0-6)
│ │ │ └───── Month (1-12)
│ │ └─────── Day of month (1-31)
│ └───────── Hour (0-23)
└─────────── Minute (0-59)
```

**Quartz Cron** (6 fields):
```
0 * * * * *
│ │ │ │ │ │
│ │ │ │ │ └─── Day of week (0-6)
│ │ │ │ └───── Month (1-12)
│ │ │ └─────── Day of month (1-31)
│ │ └───────── Hour (0-23)
│ └─────────── Minute (0-59)
└───────────── Second (always 0)
```

## Special Characters

- `*` - All values (every minute, every hour, etc.)
- `*/n` - Every nth value (*/5 = every 5 minutes)
- `n-m` - Range (1-5 = Monday through Friday)
- `n,m` - List (1,15 = 1st and 15th of month)

## Examples

| Description | Cron Expression |
|------------|-----------------|
| Every minute | `* * * * *` |
| Every 15 minutes | `*/15 * * * *` |
| Daily at 9 AM | `0 9 * * *` |
| Weekdays at 9 AM | `0 9 * * 1-5` |
| First day of month | `0 0 1 * *` |
| Twice daily (9 AM & 5 PM) | `0 9,17 * * *` |

## Technical Details

- **Dependencies**: Cronos 0.8.4 (cron parsing library)
- **Category**: Utilities
- **Version**: 1.0.0
- **Default Size**: 450×600
- **Min Size**: 400×500
- **Max Size**: 600×800

## Widget Key

`cronexpressionbuilder`

## Version History

- **1.0.0** - Initial release with full cron building capabilities

---

*Part of the 3SC Widget Collection*
