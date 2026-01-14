using System;
using System.Windows;

namespace _3SC.Widgets.GameVault;

public static class TestLauncher
{
    [STAThread]
    public static void Main()
    {
        var app = new Application();
        var win = new GameVaultWindow();
        app.Run(win);
    }
}
