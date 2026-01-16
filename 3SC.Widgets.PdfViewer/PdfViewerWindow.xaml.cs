using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace _3SC.Widgets.PdfViewer;

public partial class PdfViewerWindow : Window
{
    private readonly PdfWidgetViewModel _viewModel;

    public PdfViewerWindow()
    {
        InitializeComponent();

        _viewModel = new PdfWidgetViewModel();
        DataContext = _viewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnInitialize();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // Settings functionality - could show settings dialog
        MessageBox.Show("Settings not yet implemented", "Image Viewer", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Win32 interop for window drag
    private new void DragMove()
    {
        try
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                base.DragMove();
            }
        }
        catch
        {
            // Ignore exceptions during drag
        }
    }
}
