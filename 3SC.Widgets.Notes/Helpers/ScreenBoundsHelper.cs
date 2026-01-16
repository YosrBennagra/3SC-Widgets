using System;
using System.Drawing;
using System.Windows.Forms;

namespace _3SC.Widgets.Notes.Helpers;

public static class ScreenBoundsHelper
{
    public static Point ConstrainToScreenBounds(int x, int y, int width, int height)
    {
        var screens = Screen.AllScreens;
        var targetScreen = Screen.PrimaryScreen ?? screens[0];
        var minDistance = int.MaxValue;

        foreach (var screen in screens)
        {
            var screenBounds = screen.WorkingArea;
            var centerX = x + width / 2;
            var centerY = y + height / 2;
            var screenCenterX = screenBounds.Left + screenBounds.Width / 2;
            var screenCenterY = screenBounds.Top + screenBounds.Height / 2;
            var distance = (int)Math.Sqrt(Math.Pow(centerX - screenCenterX, 2) + Math.Pow(centerY - screenCenterY, 2));
            if (distance < minDistance)
            {
                minDistance = distance;
                targetScreen = screen;
            }
        }

        var bounds = targetScreen.WorkingArea;
        return new Point(
            Math.Max(bounds.Left, Math.Min(x, bounds.Right - width)),
            Math.Max(bounds.Top, Math.Min(y, bounds.Bottom - height)));
    }
}
