using System.Windows;

namespace _3SC.Widgets.LogoSizeTester;

public partial class LogoSizeTesterWindow : WidgetWindowBase
{
    public LogoSizeTesterWindow()
    {
        InitializeComponent();
        DataContext = new LogoSizeTesterViewModel();
    }
}
