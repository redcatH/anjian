using System;
using System.Drawing;
using System.Windows.Forms;

namespace Anjian;

public sealed class ScreenCaptureService
{
    public Bitmap Capture(Rectangle region)
    {
        if (region.Width <= 0 || region.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(region), "Capture region must be larger than zero.");
        }

        var bitmap = new Bitmap(region.Width, region.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(region.Location, Point.Empty, region.Size);
        return bitmap;
    }

    public static Rectangle GetVirtualScreenBounds()
    {
        return SystemInformation.VirtualScreen;
    }
}
