using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using _3SC.Widgets.MoonPhase.Helpers;

namespace _3SC.Widgets.MoonPhase;

public partial class MoonPhaseWindow : WidgetWindowBase
{
    private readonly MoonPhaseViewModel _viewModel;

    public MoonPhaseWindow()
    {
        InitializeComponent();
        _viewModel = new MoonPhaseViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateMoonVisualization();

        // Subscribe to property changes to update visualization
        _viewModel.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(MoonPhaseViewModel.MoonPhase))
            {
                UpdateMoonVisualization();
            }
        };
    }

    private void UpdateMoonVisualization()
    {
        try
        {
            double phase = _viewModel.MoonPhase;

            // Clear existing shadow
            MoonShadowCanvas.Children.Clear();

            // Create shadow based on phase
            if (phase < 0.01 || phase > 0.99)
            {
                // New Moon - full shadow
                var fullShadow = new Ellipse
                {
                    Width = 150,
                    Height = 150,
                    Fill = new SolidColorBrush(Color.FromArgb(230, 10, 14, 39))
                };
                MoonShadowCanvas.Children.Add(fullShadow);
            }
            else if (phase > 0.49 && phase < 0.51)
            {
                // Full Moon - no shadow
                // Leave canvas empty
            }
            else if (phase < 0.5)
            {
                // Waxing - shadow on left, moving right
                CreateWaxingShadow(phase);
            }
            else
            {
                // Waning - shadow on right, moving left
                CreateWaningShadow(phase);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating moon visualization: {ex.Message}");
        }
    }

    private void CreateWaxingShadow(double phase)
    {
        // Phase 0-0.5: Shadow shrinks from left
        // At 0, full shadow. At 0.5, no shadow

        double shadowWidth = 150 * (1 - (phase * 2)); // 150 to 0

        var shadow = new Ellipse
        {
            Width = shadowWidth,
            Height = 150,
            Fill = new SolidColorBrush(Color.FromArgb(230, 10, 14, 39)),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // Clip to create curved edge
        var geometryGroup = new GeometryGroup();
        
        // Create ellipse for curved shadow edge
        double ellipseWidth = shadowWidth * 2;
        var ellipseGeometry = new EllipseGeometry
        {
            Center = new Point(shadowWidth / 2, 75),
            RadiusX = ellipseWidth / 2,
            RadiusY = 75
        };

        geometryGroup.Children.Add(ellipseGeometry);
        shadow.Clip = geometryGroup;

        MoonShadowCanvas.Children.Add(shadow);
    }

    private void CreateWaningShadow(double phase)
    {
        // Phase 0.5-1.0: Shadow grows from right
        // At 0.5, no shadow. At 1.0, full shadow

        double shadowWidth = 150 * ((phase - 0.5) * 2); // 0 to 150

        var shadow = new Ellipse
        {
            Width = shadowWidth,
            Height = 150,
            Fill = new SolidColorBrush(Color.FromArgb(230, 10, 14, 39)),
            HorizontalAlignment = HorizontalAlignment.Right
        };

        // Clip to create curved edge
        var geometryGroup = new GeometryGroup();
        
        // Create ellipse for curved shadow edge
        double ellipseWidth = shadowWidth * 2;
        var ellipseGeometry = new EllipseGeometry
        {
            Center = new Point(shadowWidth / 2, 75),
            RadiusX = ellipseWidth / 2,
            RadiusY = 75
        };

        geometryGroup.Children.Add(ellipseGeometry);
        shadow.Clip = geometryGroup;

        // Position on right side
        System.Windows.Controls.Canvas.SetLeft(shadow, 150 - shadowWidth);
        MoonShadowCanvas.Children.Add(shadow);
    }
}
