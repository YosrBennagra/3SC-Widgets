using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace _3SC.Widgets.GradientPlayground.Models;

public partial class ColorStop : ObservableObject
{
    [ObservableProperty]
    private Color _color;

    [ObservableProperty]
    private double _offset; // 0.0 to 1.0

    public ColorStop(Color color, double offset)
    {
        _color = color;
        _offset = Math.Clamp(offset, 0.0, 1.0);
    }

    public GradientStop ToGradientStop()
    {
        return new GradientStop(Color, Offset);
    }
}
