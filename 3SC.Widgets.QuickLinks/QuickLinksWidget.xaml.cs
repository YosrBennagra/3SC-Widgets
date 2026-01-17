using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using _3SC.Widgets.QuickLinks.ViewModels;
using Serilog;

namespace _3SC.Widgets.QuickLinks;

public partial class QuickLinksWidget : Window
{
    private readonly ILogger _logger = Log.ForContext<QuickLinksWidget>();
    private bool _isDragging;
    private System.Windows.Point _clickPosition;

    public QuickLinksWidget()
        : this(Guid.Empty, 0, 0, 300, 250, false)
    {
    }

    public QuickLinksWidget(Guid widgetInstanceId, double left, double top, double width, double height, bool isLocked)
    {
        _logger.Debug("QuickLinksWidget constructor called with InstanceId={InstanceId}", widgetInstanceId);

        InitializeComponent();

        var viewModel = new QuickLinksWidgetViewModel();
        DataContext = viewModel;

        _logger.Information("QuickLinksWidget initialized successfully");
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(this);
            (sender as Border)?.CaptureMouse();
        }
    }

    private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPosition = e.GetPosition(this);
            var offset = currentPosition - _clickPosition;
            Left += offset.X;
            Top += offset.Y;
        }
    }

    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            (sender as Border)?.ReleaseMouseCapture();
        }
    }

    private void ItemBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is QuickLink link)
        {
            if (DataContext is QuickLinksWidgetViewModel vm)
            {
                vm.OpenLinkCommand.Execute(link);
                e.Handled = true;
            }
        }
    }

    private void RootBorder_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) || e.Data.GetDataPresent(System.Windows.DataFormats.Text))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void RootBorder_Drop(object sender, System.Windows.DragEventArgs e)
    {
        try
        {
            if (DataContext is not QuickLinksWidgetViewModel vm) return;

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var f in files)
                {
                    if (string.IsNullOrWhiteSpace(f)) continue;
                    vm.AddLink(System.IO.Path.GetFileNameWithoutExtension(f), new Uri(f).AbsoluteUri);
                }
            }
            else if (e.Data.GetDataPresent(System.Windows.DataFormats.Text))
            {
                var text = (string?)e.Data.GetData(System.Windows.DataFormats.Text);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var candidate = text.Trim();
                    if (!candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        candidate = "https://" + candidate;
                    vm.AddLink(string.Empty, candidate);
                }
            }
        }
        catch { }
        e.Handled = true;
    }
}
