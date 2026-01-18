using System;
using System.Windows;
using _3SC.Widgets.Contracts;
using Serilog;

namespace _3SC.Widgets.GameVault;

[Widget("game-vault", "Game Vault")]
public class GameVaultWidgetFactory : IWidgetFactory
{
    private static readonly ILogger Log = Serilog.Log.ForContext<GameVaultWidgetFactory>();

    public IWidget CreateWidget()
    {
        Log.Debug("Creating GameVault widget instance");
        return new GameVaultWidget();
    }
}

[Widget("game-vault", "Game Vault")]
public class GameVaultWidget : IWidget
{
    private static readonly ILogger Log = Serilog.Log.ForContext<GameVaultWidget>();
    private GameVaultWindow? _window;

    public string WidgetKey => "game-vault";
    public string DisplayName => "Game Vault";
    public string Version => "1.1.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => true;

    public Window? CreateWindow()
    {
        try
        {
            if (_window == null)
            {
                // Provide default positioning/size when created outside host
                _window = new GameVaultWindow(Guid.Empty, 100, 100, 420, 340, false);
                _window.Owner = Application.Current?.MainWindow;
                Log.Information("GameVault window created successfully");
            }
            return _window;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create GameVault window");
            throw;
        }
    }

    public System.Windows.Controls.UserControl GetView() => new System.Windows.Controls.UserControl();

    public void OnInitialize()
    {
        Log.Debug("GameVault widget initialized");
    }

    public void OnDispose()
    {
        try
        {
            _window?.Close();
            _window = null;
            Log.Information("GameVault widget disposed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during GameVault widget disposal");
        }
    }

    public void ShowSettings()
    {
        Log.Information("ShowSettings called - not yet implemented");
    }
}
