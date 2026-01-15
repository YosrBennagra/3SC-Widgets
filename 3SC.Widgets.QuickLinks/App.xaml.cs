using System;

namespace _3SC.Widgets.QuickLinks
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            var wnd = new QuickLinksWidget();
            wnd.Show();
        }
    }
}
