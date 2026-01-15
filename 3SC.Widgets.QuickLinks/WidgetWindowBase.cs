using System.Threading.Tasks;
using System.Windows;

namespace _3SC.Widgets
{
    public class WidgetWindowBase : Window
    {
        protected virtual Task OnWidgetLoadedAsync() => Task.CompletedTask;
        protected virtual Task OnWidgetClosingAsync() => Task.CompletedTask;
        protected virtual bool IsDragBlocked(DependencyObject? source) => false;

        protected override async void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            await OnWidgetLoadedAsync();
        }

        protected override async void OnClosed(System.EventArgs e)
        {
            await OnWidgetClosingAsync();
            base.OnClosed(e);
        }
    }
}
