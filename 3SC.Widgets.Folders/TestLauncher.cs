using System;
using System.Windows;

namespace _3SC.Widgets.Folders
{
    public static class TestLauncher
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var win = new FoldersWindow(Guid.NewGuid(), 100, 100, 300, 200, false);
            app.Run(win);
        }
    }
}
