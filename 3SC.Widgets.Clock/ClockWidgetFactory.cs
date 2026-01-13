using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Clock;

public class ClockWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new ClockWidget();
    }
}
