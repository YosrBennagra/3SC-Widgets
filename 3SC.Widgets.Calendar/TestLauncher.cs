using System;
using System.Windows;

namespace _3SC.Widgets.Calendar
{
    public static class TestLauncher
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var win = new CalendarWindow();
            app.Run(win);
        }
    }
}
