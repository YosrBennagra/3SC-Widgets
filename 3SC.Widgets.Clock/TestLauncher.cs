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
            
            // Create the widget plugin
            var plugin = new ClockWidgetPlugin();
            
            Console.WriteLine("=== Clock Widget Test ===");
            Console.WriteLine($"Widget Key: {plugin.WidgetKey}");
            Console.WriteLine($"Display Name: {plugin.DisplayName}");
            Console.WriteLine($"Version: {plugin.Version}");
            Console.WriteLine($"Category: {plugin.Category}");
            Console.WriteLine("========================\n");
            
            // Test with default settings (no JSON)
            Console.WriteLine("Creating widget window with default settings...");
            var window = (Window)plugin.CreateWidgetWindow(Guid.NewGuid(), null);
            
            // Show the window
            app.Run(window);
        }
    }
}
