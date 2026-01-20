using Serilog;
using System;
using System.Windows;
using _3SC.Widgets.Contracts;

namespace ThreeSC.Widgets.Pomodoro
{
    [Widget("pomodoro-timer", "Pomodoro+ Timer")]
    public class PomodoroWidgetFactory : IWidgetFactory
    {
        public IWidget CreateWidget()
        {
            return new PomodoroWidgetImpl();
        }
    }

    [Widget("pomodoro-timer", "Pomodoro+ Timer")]
    public class PomodoroWidgetImpl : IWidget
    {
        private static readonly ILogger Logger = Log.ForContext<PomodoroWidgetImpl>();
        private PomodoroWindow? _window;
        private PomodoroViewModel? _viewModel;

        public string WidgetKey => "pomodoro-timer";
        public string DisplayName => "Pomodoro+ Timer";
        public string Version => "1.0.0";
        public bool HasSettings => true;
        public bool HasOwnWindow => true;

        public Window? CreateWindow()
        {
            try
            {
                Logger.Information("Creating Pomodoro window");
                _viewModel = new PomodoroViewModel();
                _window = new PomodoroWindow(_viewModel);
                return _window;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create Pomodoro window");
                throw;
            }
        }

        public System.Windows.Controls.UserControl GetView()
        {
            throw new NotSupportedException("This widget provides its own window.");
        }

        public void OnInitialize()
        {
            Logger.Information("Initializing Pomodoro widget");
            _viewModel?.LoadSettings();
        }

        public void OnDispose()
        {
            Logger.Information("Disposing Pomodoro widget");
            _viewModel?.SaveSettings();
            _viewModel?.Dispose();
            _window?.Close();
        }

        public void ShowSettings()
        {
            Logger.Information("ShowSettings called (not implemented)");
        }

        public void OnSettingsChanged(string settingsJson)
        {
            Logger.Information("Settings changed: {Settings}", settingsJson);
        }
    }
}
