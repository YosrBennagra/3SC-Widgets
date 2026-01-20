using System.Windows;
using System.Windows.Input;

namespace _3SC.Widgets.ThisDayInHistory;

public class WidgetWindowBase : Window
{
    private Point _dragStartPoint;

    public WidgetWindowBase()
    {
        MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            _dragStartPoint = e.GetPosition(this);
            DragMove();
        }
    }
}
