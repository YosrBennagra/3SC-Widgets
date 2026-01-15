using System.Windows;
using _3SC.Widgets;

namespace _3SC.Widgets.QuickLinks.TestLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var wnd = new QuickLinksWidget();
            wnd.Show();
        }
    }
}
