using System.Windows;

namespace _3SC.Widgets.Calendar
{
    public partial class CalendarWindow : Window
    {
        public CalendarWindow()
        {
            InitializeComponent();
            DataContext ??= new CalendarWidgetViewModel();
        }
    }
}
