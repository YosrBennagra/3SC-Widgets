using System.Windows;

namespace _3SC.Widgets.GradientPlayground;

public partial class GradientPlaygroundWindow : WidgetWindowBase
{
    public GradientPlaygroundWindow()
    {
        InitializeComponent();
        DataContext = new GradientPlaygroundViewModel();
    }
}
