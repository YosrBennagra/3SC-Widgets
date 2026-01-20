using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using _3SC.Widgets.GradientPlayground.Models;

namespace _3SC.Widgets.GradientPlayground;

public partial class GradientPlaygroundViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<GradientPlaygroundViewModel>();

    [ObservableProperty]
    private ObservableCollection<ColorStop> _colorStops;

    [ObservableProperty]
    private GradientType _gradientType;

    [ObservableProperty]
    private double _angle; // For linear gradients (0-360)

    [ObservableProperty]
    private Brush _previewBrush;

    [ObservableProperty]
    private string _cssOutput = "";

    [ObservableProperty]
    private string _svgOutput = "";

    public GradientPlaygroundViewModel()
    {
        // Initialize with a beautiful default gradient
        _colorStops = new ObservableCollection<ColorStop>
        {
            new ColorStop(Color.FromRgb(255, 0, 128), 0.0),   // Pink
            new ColorStop(Color.FromRgb(128, 0, 255), 1.0)    // Purple
        };

        _gradientType = GradientType.Linear;
        _angle = 45.0;
        _previewBrush = CreateBrush();

        // Subscribe to changes
        _colorStops.CollectionChanged += (s, e) => UpdatePreview();
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GradientType) || e.PropertyName == nameof(Angle))
            {
                UpdatePreview();
            }
        };
    }

    [RelayCommand]
    private void AddColorStop()
    {
        if (ColorStops.Count >= 10)
        {
            Logger.Information("Maximum 10 color stops reached");
            return;
        }

        // Add new color stop in the middle with a random color
        var random = new Random();
        var newColor = Color.FromRgb(
            (byte)random.Next(256),
            (byte)random.Next(256),
            (byte)random.Next(256));

        var newOffset = ColorStops.Count > 0
            ? ColorStops.Max(c => c.Offset) / 2 + 0.25
            : 0.5;

        var newStop = new ColorStop(newColor, Math.Clamp(newOffset, 0, 1));
        newStop.PropertyChanged += (s, e) => UpdatePreview();

        ColorStops.Add(newStop);
        UpdatePreview();
        Logger.Information("Added color stop at offset {Offset}", newOffset);
    }

    [RelayCommand]
    private void RemoveColorStop(ColorStop stop)
    {
        if (ColorStops.Count <= 2)
        {
            Logger.Information("Minimum 2 color stops required");
            return;
        }

        ColorStops.Remove(stop);
        UpdatePreview();
        Logger.Information("Removed color stop");
    }

    [RelayCommand]
    private void RandomizeColors()
    {
        var random = new Random();
        foreach (var stop in ColorStops)
        {
            stop.Color = Color.FromRgb(
                (byte)random.Next(256),
                (byte)random.Next(256),
                (byte)random.Next(256));
        }
        UpdatePreview();
        Logger.Information("Randomized gradient colors");
    }

    [RelayCommand]
    private void ReverseGradient()
    {
        foreach (var stop in ColorStops)
        {
            stop.Offset = 1.0 - stop.Offset;
        }
        UpdatePreview();
        Logger.Information("Reversed gradient direction");
    }

    [RelayCommand]
    private void LoadPreset(string presetName)
    {
        ColorStops.Clear();

        switch (presetName)
        {
            case "Sunset":
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 94, 77), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(245, 140, 92), 0.5));
                ColorStops.Add(new ColorStop(Color.FromRgb(199, 121, 208), 1.0));
                break;

            case "Ocean":
                ColorStops.Add(new ColorStop(Color.FromRgb(0, 180, 216), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(0, 119, 182), 1.0));
                break;

            case "Forest":
                ColorStops.Add(new ColorStop(Color.FromRgb(52, 211, 153), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(16, 185, 129), 0.5));
                ColorStops.Add(new ColorStop(Color.FromRgb(5, 150, 105), 1.0));
                break;

            case "Fire":
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 223, 0), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 95, 31), 0.5));
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 0, 0), 1.0));
                break;

            case "Cotton Candy":
                ColorStops.Add(new ColorStop(Color.FromRgb(252, 231, 243), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(219, 234, 254), 1.0));
                break;

            case "Aurora":
                ColorStops.Add(new ColorStop(Color.FromRgb(64, 224, 208), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(138, 43, 226), 0.5));
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 105, 180), 1.0));
                break;

            case "Midnight":
                ColorStops.Add(new ColorStop(Color.FromRgb(0, 0, 70), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(28, 0, 81), 0.5));
                ColorStops.Add(new ColorStop(Color.FromRgb(56, 0, 92), 1.0));
                break;

            case "Peachy":
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 218, 185), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 160, 122), 1.0));
                break;

            default:
                // Default gradient
                ColorStops.Add(new ColorStop(Color.FromRgb(255, 0, 128), 0.0));
                ColorStops.Add(new ColorStop(Color.FromRgb(128, 0, 255), 1.0));
                break;
        }

        foreach (var stop in ColorStops)
        {
            stop.PropertyChanged += (s, e) => UpdatePreview();
        }

        UpdatePreview();
        Logger.Information("Loaded preset: {Preset}", presetName);
    }

    [RelayCommand]
    private void CopyCss()
    {
        try
        {
            Clipboard.SetText(CssOutput);
            Logger.Information("CSS copied to clipboard");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy CSS");
        }
    }

    [RelayCommand]
    private void CopySvg()
    {
        try
        {
            Clipboard.SetText(SvgOutput);
            Logger.Information("SVG copied to clipboard");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy SVG");
        }
    }

    private void UpdatePreview()
    {
        PreviewBrush = CreateBrush();
        GenerateCss();
        GenerateSvg();
    }

    private Brush CreateBrush()
    {
        var sortedStops = ColorStops.OrderBy(c => c.Offset).ToList();

        if (GradientType == GradientType.Linear)
        {
            var angleRad = Angle * Math.PI / 180.0;
            var endPoint = new Point(
                0.5 + Math.Cos(angleRad) * 0.5,
                0.5 + Math.Sin(angleRad) * 0.5);
            var startPoint = new Point(
                0.5 - Math.Cos(angleRad) * 0.5,
                0.5 - Math.Sin(angleRad) * 0.5);

            var brush = new LinearGradientBrush
            {
                StartPoint = startPoint,
                EndPoint = endPoint
            };

            foreach (var stop in sortedStops)
            {
                brush.GradientStops.Add(stop.ToGradientStop());
            }

            return brush;
        }
        else // Radial
        {
            var brush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
            };

            foreach (var stop in sortedStops)
            {
                brush.GradientStops.Add(stop.ToGradientStop());
            }

            return brush;
        }
    }

    private void GenerateCss()
    {
        var sortedStops = ColorStops.OrderBy(c => c.Offset).ToList();
        var colorStopsStr = string.Join(", ", sortedStops.Select(s =>
        {
            var color = s.Color;
            var percent = (int)(s.Offset * 100);
            return $"rgb({color.R}, {color.G}, {color.B}) {percent}%";
        }));

        if (GradientType == GradientType.Linear)
        {
            CssOutput = $"background: linear-gradient({Angle:F0}deg, {colorStopsStr});";
        }
        else
        {
            CssOutput = $"background: radial-gradient(circle, {colorStopsStr});";
        }
    }

    private void GenerateSvg()
    {
        var sortedStops = ColorStops.OrderBy(c => c.Offset).ToList();
        var id = "gradient" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (GradientType == GradientType.Linear)
        {
            var angleRad = Angle * Math.PI / 180.0;
            var x1 = 50 - Math.Cos(angleRad) * 50;
            var y1 = 50 - Math.Sin(angleRad) * 50;
            var x2 = 50 + Math.Cos(angleRad) * 50;
            var y2 = 50 + Math.Sin(angleRad) * 50;

            var stops = string.Join("\n    ", sortedStops.Select(s =>
            {
                var color = s.Color;
                var percent = (int)(s.Offset * 100);
                return $"<stop offset=\"{percent}%\" style=\"stop-color:rgb({color.R},{color.G},{color.B});stop-opacity:1\" />";
            }));

            SvgOutput = $@"<svg width=""200"" height=""200"" xmlns=""http://www.w3.org/2000/svg"">
  <defs>
    <linearGradient id=""{id}"" x1=""{x1:F1}%"" y1=""{y1:F1}%"" x2=""{x2:F1}%"" y2=""{y2:F1}%"">
    {stops}
    </linearGradient>
  </defs>
  <rect width=""200"" height=""200"" fill=""url(#{id})"" />
</svg>";
        }
        else
        {
            var stops = string.Join("\n    ", sortedStops.Select(s =>
            {
                var color = s.Color;
                var percent = (int)(s.Offset * 100);
                return $"<stop offset=\"{percent}%\" style=\"stop-color:rgb({color.R},{color.G},{color.B});stop-opacity:1\" />";
            }));

            SvgOutput = $@"<svg width=""200"" height=""200"" xmlns=""http://www.w3.org/2000/svg"">
  <defs>
    <radialGradient id=""{id}"" cx=""50%"" cy=""50%"" r=""50%"">
    {stops}
    </radialGradient>
  </defs>
  <rect width=""200"" height=""200"" fill=""url(#{id})"" />
</svg>";
        }
    }
}
