using System.Windows;

namespace ThreeSC.Widgets.Pomodoro
{
    public partial class PomodoroWindow : WidgetWindowBase
    {
        public PomodoroWindow(PomodoroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
