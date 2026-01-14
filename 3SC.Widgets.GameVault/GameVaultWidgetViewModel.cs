using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace _3SC.Widgets.GameVault;

public sealed partial class GameVaultWidgetViewModel : LauncherAppsViewModelBase<AppItem>
{
    private static readonly string AppsPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC",
        "launcher_apps.json");

    public GameVaultWidgetViewModel()
        : base("Game Vault")
    {
        Apps.Clear();
    }

    protected override string AppsFilePath => AppsPath;

    protected override string SettingsOpenedMessage => "Game Vault settings opened";
}
