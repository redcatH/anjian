using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Anjian;

public sealed class ImagePreprocessService
{
    public Bitmap Preprocess(Bitmap source, AmountOcrOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);

        var processed = new Bitmap(source);
        if (options.UseGrayscale)
        {
            processed = ApplyTransform(processed, ToGrayscale);
        }

        if (options.UseBinarization)
        {
            processed = ApplyTransform(processed, color => ToBinary(color, options.BinarizationThreshold));
        }

        if (options.Scale2x)
        {
            processed = Scale(processed, 2);
        }

        return processed;
    }

    private static Bitmap ApplyTransform(Bitmap source, Func<Color, Color> transform)
    {
        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                result.SetPixel(x, y, transform(source.GetPixel(x, y)));
            }
        }

        source.Dispose();
        return result;
    }

    private static Bitmap Scale(Bitmap source, int factor)
    {
        var result = new Bitmap(source.Width * factor, source.Height * factor, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(result);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(source, new Rectangle(0, 0, result.Width, result.Height));
        source.Dispose();
        return result;
    }

    private static Color ToGrayscale(Color color)
    {
        var gray = (int)Math.Round(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
        return Color.FromArgb(gray, gray, gray);
    }

    private static Color ToBinary(Color color, int threshold)
    {
        var gray = (int)Math.Round(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
        return gray >= threshold ? Color.White : Color.Black;
    }
}
