using System;
using System.Windows;
using _3SC.Widgets.Clock;

namespace TestClockWidget
{
    /// <summary>
    /// Simple test launcher to run the Clock widget standalone
    /// </summary>
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            // Create WPF application
            var app = new System.Windows.Application();

            // Create the widget factory
            var factory = new ClockWidgetFactory();
            var widget = factory.CreateWidget();

            Console.WriteLine("=== Clock Widget Test ===");
            Console.WriteLine($"Widget Key: {widget.WidgetKey}");
            Console.WriteLine($"Display Name: {widget.DisplayName}");
            Console.WriteLine($"Version: {widget.Version}");
            Console.WriteLine($"Has Own Window: {widget.HasOwnWindow}");
            Console.WriteLine("========================\n");

            // Create widget window
            Console.WriteLine("Creating widget window...");
            var window = widget.CreateWindow();

            if (window != null)
            {
                widget.OnInitialize();
                app.Run(window);
            }
        }
    }
}
