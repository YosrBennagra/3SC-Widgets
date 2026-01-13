namespace _3SC.Widgets.Contracts;

/// <summary>
/// Interface that all external widgets must implement to be loaded by the widget system.
/// </summary>
public interface IExternalWidget
{
    /// <summary>
    /// Unique identifier for this widget type.
    /// </summary>
    string WidgetKey { get; }

    /// <summary>
    /// Display name shown in the widget picker.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Category for organizing widgets in the UI.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Description of what the widget does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Version of the widget.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Creates a new instance of the widget window.
    /// </summary>
    /// <param name="widgetInstanceId">Unique ID for this widget instance.</param>
    /// <param name="settingsJson">JSON-serialized settings for this widget instance.</param>
    /// <returns>The widget window (typically a WPF Window).</returns>
    object CreateWidgetWindow(Guid widgetInstanceId, string? settingsJson);
}
