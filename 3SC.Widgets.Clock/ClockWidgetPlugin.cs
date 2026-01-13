using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Clock;

/// <summary>
/// External Clock Widget implementation.
/// This class serves as the entry point for the dynamically loaded clock widget.
/// </summary>
public class ClockWidgetPlugin : IExternalWidget
{
    public string WidgetKey => "clock-external";
    public string DisplayName => "Clock (External)";
    public string Category => "General";
    public string Description => "Displays current time with timezone support - loaded from external DLL";
    public string Version => "1.0.0";

    public object CreateWidgetWindow(Guid widgetInstanceId, string? settingsJson)
    {
        // Parse settings from JSON if provided
        _3SC.Domain.ValueObjects.ClockWidgetSettings? settings = null;
        if (!string.IsNullOrEmpty(settingsJson))
        {
            try
            {
                settings = System.Text.Json.JsonSerializer.Deserialize<_3SC.Domain.ValueObjects.ClockWidgetSettings>(settingsJson);
            }
            catch
            {
                // Use defaults if deserialization fails
                settings = null;
            }
        }

        // Create and return the widget window
        return new ClockWidgetWindow(widgetInstanceId, settings);
    }
}
