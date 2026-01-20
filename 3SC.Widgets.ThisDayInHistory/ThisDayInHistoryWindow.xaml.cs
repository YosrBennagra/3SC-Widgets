using System.Windows;

namespace _3SC.Widgets.ThisDayInHistory
{
    public partial class ThisDayInHistoryWindow : WidgetWindowBase
    {
        public ThisDayInHistoryWindow()
        {
            InitializeComponent();
            DataContext = new ThisDayInHistoryViewModel();
        }
    }
}
