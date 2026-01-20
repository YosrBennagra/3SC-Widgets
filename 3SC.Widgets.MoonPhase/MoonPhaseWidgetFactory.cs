using _3SC.Widgets.Contracts;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace _3SC.Widgets.MoonPhase;

public class MoonPhaseWidgetFactory : IWidgetFactory
{
    private static readonly ILogger Logger = Log.ForContext<MoonPhaseWidgetFactory>();

    public string WidgetKey => "moon-phase";
    public string WidgetName => "Moon Phase";
    public string Description => "Current moon phase with folklore and lunar calendar";

    public IWidget CreateWidget()
    {
        Logger.Information("Creating Moon Phase widget");
        return new MoonPhaseWidget();
    }
}

public class MoonPhaseWidget : IWidget
{
    private static readonly ILogger Logger = Log.ForContext<MoonPhaseWidget>();

    public string WidgetKey => "moon-phase";
    public string DisplayName => "Moon Phase";
    public string Version => "1.0.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => false;

    public void OnInitialize()
    {
        Logger.Information("Moon Phase widget initialized");
    }

    public UserControl GetView()
    {
        throw new NotSupportedException("Moon Phase widget has its own window");
    }

    public Window CreateWindow()
    {
        Logger.Information("Creating Moon Phase window");
        return new MoonPhaseWindow();
    }

    public void ShowSettings()
    {
        // No settings for this widget
    }

    public void OnDispose()
    {
        Logger.Information("Moon Phase widget disposed");
    }
}
