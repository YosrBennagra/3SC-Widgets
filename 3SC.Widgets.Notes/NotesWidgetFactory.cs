using System;
using System.Windows;
using _3SC.Widgets.Contracts;

namespace _3SC.Widgets.Notes;

public class NotesWidgetFactory : IWidgetFactory
{
    public IWidget CreateWidget()
    {
        return new NotesWidget();
    }
}

public class NotesWidget : IWidget
{
    private NotesWindow? _window;
    private NotesWidgetViewModel? _viewModel;

    public string WidgetKey => "notes";
    public string DisplayName => "Notes";
    public string Version => "1.0.0";
    public bool HasOwnWindow => true;
    public bool HasSettings => false;

    public Window? CreateWindow()
    {
        _window = new NotesWindow();
        _viewModel = (_window.DataContext as NotesWidgetViewModel)!;
        return _window;
    }

    public System.Windows.Controls.UserControl? GetView()
    {
        return null;
    }

    public void OnInitialize()
    {
        _viewModel?.OnInitialize();
    }

    public void OnDispose()
    {
        _viewModel?.OnDispose();
        _window?.Close();
        _window = null;
        _viewModel = null;
    }

    public void ShowSettings()
    {
        // No settings for this widget
    }
}
