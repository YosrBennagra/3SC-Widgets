using _3SC.Widgets.Contracts;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace _3SC.Widgets.ThisDayInHistory;

public class ThisDayInHistoryWidgetFactory : IWidgetFactory
{
    private static readonly ILogger Logger = Log.ForContext<ThisDayInHistoryWidgetFactory>();

    public string WidgetKey => "this-day-in-history";
    public string WidgetName => "This Day in History";
    public string Description => "Discover what happened on this day throughout history";

    public IWidget CreateWidget()
    {
        Logger.Information("Creating This Day in History widget");
        return new ThisDayInHistoryWidget();
    }
}

public class ThisDayInHistoryWidget : IWidget
{
    private static readonly ILogger Logger = Log.ForContext<ThisDayInHistoryWidget>();

    public string WidgetKey => "this-day-in-history";
    public string DisplayName => "This Day in History";
    public string Version => "1.0.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => false;

    public void OnInitialize()
    {
        Logger.Information("This Day in History widget initialized");
    }

    public UserControl GetView()
    {
        throw new NotSupportedException("This Day in History widget has its own window");
    }

    public Window CreateWindow()
    {
        Logger.Information("Creating This Day in History window");
        return new ThisDayInHistoryWindow();
    }

    public void ShowSettings()
    {
        // No settings for this widget
    }

    public void OnDispose()
    {
        Logger.Information("This Day in History widget disposed");
    }
}
