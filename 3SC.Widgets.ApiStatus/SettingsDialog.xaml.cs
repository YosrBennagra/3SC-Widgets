using System.Windows;

namespace _3SC.Widgets.ApiStatus;

public partial class SettingsDialog : Window
{
    private ApiStatusViewModel _viewModel;

    public SettingsDialog(ApiStatusViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveSettings();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
