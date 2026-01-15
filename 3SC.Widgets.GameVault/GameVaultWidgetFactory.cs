using System;
using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.GameVault;

public class GameVaultWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget() => new GameVaultWidget();
}

public class GameVaultWidget : IWidget
{
    private GameVaultWindow? _window;

    public string WidgetKey => "game-vault";
    public string DisplayName => "Game Vault";
    public string Version => "1.0.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => true;

    public Window? CreateWindow()
    {
        if (_window == null)
        {
            _window = new GameVaultWindow();
            _window.Owner = Application.Current?.MainWindow;
        }
        return _window;
    }

    public System.Windows.Controls.UserControl GetView() => new System.Windows.Controls.UserControl();

    public void OnInitialize() { }

    public void OnDispose()
    {
        _window?.Close();
        _window = null;
    }
}
