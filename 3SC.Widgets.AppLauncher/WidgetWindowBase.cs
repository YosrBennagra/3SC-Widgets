using System.Threading.Tasks;
using System.Windows;

namespace _3SC.Widgets.AppLauncher
{
    public class WidgetWindowBase : Window
    {
        public WidgetWindowBase()
        {
        }

        protected virtual Task OnWidgetLoadedAsync() => Task.CompletedTask;
        protected virtual Task OnWidgetClosingAsync() => Task.CompletedTask;
        protected virtual bool IsDragBlocked(DependencyObject? source) => false;
        protected virtual double MinWidgetWidth => 100;
        protected virtual double MinWidgetHeight => 80;

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
