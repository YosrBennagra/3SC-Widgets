using System;
using _3SC.Widgets.Contracts;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace _3SC.Widgets.Clock;

public class ClockWidget : IWidget
{
    private ClockWidgetView? _view;

    public string WidgetKey => "clock";
    public string DisplayName => "Digital Clock";
    public string Version => "1.0.0";
    public bool HasSettings => true;

    public WpfUserControl GetView()
    {
        _view ??= new ClockWidgetView();
        return _view;
    }

    public void OnInitialize()
    {
        // Widget initialization - view will handle its own timer
    }

    public void OnDispose()
    {
        // Clean up resources
        _view?.Dispose();
        _view = null;
    }

    public void ShowSettings()
    {
        if (_view != null)
        {
            var settingsWindow = new ClockSettingsWindow(_view.GetCurrentSettings())
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };

            if (settingsWindow.ShowDialog() == true && settingsWindow.UpdatedSettings != null)
            {
                _view.ApplySettings(settingsWindow.UpdatedSettings);
            }
        }
    }
}
