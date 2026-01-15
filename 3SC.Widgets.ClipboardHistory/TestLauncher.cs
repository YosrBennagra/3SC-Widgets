using System;
using System.Windows;

namespace _3SC.Widgets.ClipboardHistory
{
    public static class TestLauncher
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var win = new ClipboardHistoryWindow();
            app.Run(win);
        }
    }
}
