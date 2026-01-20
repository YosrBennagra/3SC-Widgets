using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.CronExpressionBuilder;

/// <summary>
/// Factory for creating Cron Expression Builder widget instances.
/// </summary>
[Widget("cronexpressionbuilder", "Cron Expression Builder")]
public class CronExpressionBuilderWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new CronExpressionBuilderWidget();
    }
}

/// <summary>
/// Cron Expression Builder widget implementation.
/// </summary>
[Widget("cronexpressionbuilder", "Cron Expression Builder")]
public class CronExpressionBuilderWidget : IWidget
{
    private CronExpressionBuilderWindow? _window;

    public string WidgetKey => "cronexpressionbuilder";
    public string DisplayName => "Cron Expression Builder";
    public string Version => "1.0.0";
    public bool HasSettings => false;
    public bool HasOwnWindow => true;

    public Window? CreateWindow()
    {
        _window = new CronExpressionBuilderWindow();
        return _window;
    }

    public System.Windows.Controls.UserControl GetView()
    {
        throw new NotSupportedException("This widget provides its own window.");
    }

    public void OnInitialize()
    {
        // Window handles its own initialization
    }

    public void OnDispose()
    {
        _window?.Close();
        _window = null;
    }

    public void ShowSettings()
    {
        // No settings for this widget
    }
}
