using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Anjian;

public sealed class TemplateMatcher : IImageMatcher
{
    private const int SamplePointCount = 12;

    public ImageMatchResult Find(Bitmap source, Bitmap template, ImageMatchOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(template);

        if (options.Step <= 0)
        {
            return CreateFailure("Step 必须大于 0。");
        }

        if (options.SearchRegion.Width <= 0 || options.SearchRegion.Height <= 0)
        {
            return CreateFailure("搜索区域必须大于 0。");
        }

        if (options.SearchRegion.Left < 0 ||
            options.SearchRegion.Top < 0 ||
            options.SearchRegion.Right > source.Width ||
            options.SearchRegion.Bottom > source.Height)
        {
            return CreateFailure("搜索区域超出了源图片范围。");
        }

        if (template.Width > options.SearchRegion.Width || template.Height > options.SearchRegion.Height)
        {
            return CreateFailure("模板图片大于搜索区域。");
        }

        using var normalizedSource = ConvertToArgb32(source);
        using var normalizedTemplate = ConvertToArgb32(template);

        var watch = Stopwatch.StartNew();

        var sourcePixels = PixelBuffer.Create(normalizedSource, options.UseGrayscale);
        var templatePixels = PixelBuffer.Create(normalizedTemplate, options.UseGrayscale);
        var maxAllowedDifference = CalculateMaxAllowedDifference(templatePixels, options.UseGrayscale, options.Threshold);
        var samplePoints = BuildSamplePoints(templatePixels.Width, templatePixels.Height);

        var hits = new List<ImageMatchHit>();
        var bestScore = double.MinValue;
        Point? bestLocation = null;

        var maxX = options.SearchRegion.Right - templatePixels.Width;
        var maxY = options.SearchRegion.Bottom - templatePixels.Height;

        var xPositions = BuildAxis(options.SearchRegion.Left, maxX, options.Step);
        var yPositions = BuildAxis(options.SearchRegion.Top, maxY, options.Step);

        var columnFirst = IsColumnFirst(options.ScanDirection);
        var orderedXPositions = columnFirst ? OrderAxis(xPositions, options.ScanDirection) : xPositions;
        var orderedYPositions = columnFirst ? yPositions : OrderAxis(yPositions, options.ScanDirection);

        if (columnFirst)
        {
            foreach (var x in orderedXPositions)
            {
                foreach (var y in orderedYPositions)
                {
                    if (TryHandleCandidate(
                            sourcePixels,
                            templatePixels,
                            options,
                            samplePoints,
                            maxAllowedDifference,
                            hits,
                            x,
                            y,
                            ref bestScore,
                            ref bestLocation,
                            watch,
                            out var successResult))
                    {
                        return successResult;
                    }
                }
            }
        }
        else
        {
            foreach (var y in orderedYPositions)
            {
                foreach (var x in orderedXPositions)
                {
                    if (TryHandleCandidate(
                            sourcePixels,
                            templatePixels,
                            options,
                            samplePoints,
                            maxAllowedDifference,
                            hits,
                            x,
                            y,
                            ref bestScore,
                            ref bestLocation,
                            watch,
                            out var successResult))
                    {
                        return successResult;
                    }
                }
            }
        }

        watch.Stop();

        if (options.MatchMode == ImageMatchMode.All && hits.Count > 0)
        {
            hits.Sort(static (left, right) => right.Score.CompareTo(left.Score));
            return new ImageMatchResult(
                true,
                hits[0].Location,
                hits[0].Score,
                watch.ElapsedMilliseconds,
                $"共找到 {hits.Count} 个匹配结果，扫描方向：{GetScanDirectionText(options.ScanDirection)}。",
                hits);
        }

        if (bestLocation is null)
        {
            return new ImageMatchResult(false, null, 0d, watch.ElapsedMilliseconds, "没有可评估的候选位置。", Array.Empty<ImageMatchHit>());
        }

        return new ImageMatchResult(
            false,
            bestLocation,
            bestScore,
            watch.ElapsedMilliseconds,
            $"最佳分数 {bestScore:F4} 低于阈值 {options.Threshold:F4}，扫描方向：{GetScanDirectionText(options.ScanDirection)}。",
            Array.Empty<ImageMatchHit>());
    }

    private static bool TryHandleCandidate(
        PixelBuffer source,
        PixelBuffer template,
        ImageMatchOptions options,
        IReadOnlyList<SamplePoint> samplePoints,
        double maxAllowedDifference,
        ICollection<ImageMatchHit> hits,
        int x,
        int y,
        ref double bestScore,
        ref Point? bestLocation,
        Stopwatch watch,
        out ImageMatchResult successResult)
    {
        successResult = default!;

        if (!PassesSampleFilter(source, template, x, y, samplePoints, maxAllowedDifference))
        {
            return false;
        }

        var score = CalculateScore(source, template, x, y, maxAllowedDifference);
        if (score < 0d)
        {
            return false;
        }

        if (score > bestScore)
        {
            bestScore = score;
            bestLocation = new Point(x, y);
        }

        if (score < options.Threshold)
        {
            return false;
        }

        var hit = new ImageMatchHit(new Point(x, y), score);
        if (options.MatchMode == ImageMatchMode.First)
        {
            watch.Stop();
            successResult = CreateSuccess(hit, watch.ElapsedMilliseconds, $"找到首个匹配结果，扫描方向：{GetScanDirectionText(options.ScanDirection)}。");
            return true;
        }

        AddIfDistinct(hits, hit, template.Size);
        return false;
    }

    private static bool PassesSampleFilter(
        PixelBuffer source,
        PixelBuffer template,
        int startX,
        int startY,
        IReadOnlyList<SamplePoint> samplePoints,
        double maxAllowedDifference)
    {
        if (samplePoints.Count == 0)
        {
            return true;
        }

        double sampledDifference = 0d;
        foreach (var point in samplePoints)
        {
            sampledDifference += GetDifference(source, template, startX + point.X, startY + point.Y, point.X, point.Y);
            if (sampledDifference > point.CutoffDifference * maxAllowedDifference)
            {
                return false;
            }
        }

        return true;
    }

    private static double CalculateScore(
        PixelBuffer source,
        PixelBuffer template,
        int startX,
        int startY,
        double maxAllowedDifference)
    {
        double totalDifference = 0d;
        for (var y = 0; y < template.Height; y++)
        {
            var sourceRowOffset = source.GetOffset(startX, startY + y);
            var templateRowOffset = template.GetOffset(0, y);

            if (template.IsGrayscale)
            {
                for (var x = 0; x < template.Width; x++)
                {
                    totalDifference += Math.Abs(source.Buffer[sourceRowOffset + x] - template.Buffer[templateRowOffset + x]);
                    if (totalDifference > maxAllowedDifference)
                    {
                        return -1d;
                    }
                }
            }
            else
            {
                for (var x = 0; x < template.Width; x++)
                {
                    var sourceIndex = sourceRowOffset + x * 4;
                    var templateIndex = templateRowOffset + x * 4;

                    totalDifference +=
                        Math.Abs(source.Buffer[sourceIndex] - template.Buffer[templateIndex]) +
                        Math.Abs(source.Buffer[sourceIndex + 1] - template.Buffer[templateIndex + 1]) +
                        Math.Abs(source.Buffer[sourceIndex + 2] - template.Buffer[templateIndex + 2]);

                    if (totalDifference > maxAllowedDifference)
                    {
                        return -1d;
                    }
                }
            }
        }

        var normalizedDifference = totalDifference / (template.PixelCount * template.ChannelDenominator);
        return 1d - normalizedDifference;
    }

    private static double GetDifference(
        PixelBuffer source,
        PixelBuffer template,
        int sourceX,
        int sourceY,
        int templateX,
        int templateY)
    {
        var sourceOffset = source.GetOffset(sourceX, sourceY);
        var templateOffset = template.GetOffset(templateX, templateY);

        if (template.IsGrayscale)
        {
            return Math.Abs(source.Buffer[sourceOffset] - template.Buffer[templateOffset]);
        }

        return
            Math.Abs(source.Buffer[sourceOffset] - template.Buffer[templateOffset]) +
            Math.Abs(source.Buffer[sourceOffset + 1] - template.Buffer[templateOffset + 1]) +
            Math.Abs(source.Buffer[sourceOffset + 2] - template.Buffer[templateOffset + 2]);
    }

    private static double CalculateMaxAllowedDifference(PixelBuffer template, bool useGrayscale, double threshold)
    {
        var normalizedThreshold = Math.Clamp(threshold, 0d, 1d);
        return (1d - normalizedThreshold) * template.PixelCount * template.ChannelDenominator;
    }

    private static List<SamplePoint> BuildSamplePoints(int width, int height)
    {
        var points = new List<SamplePoint>();
        var seen = new HashSet<(int X, int Y)>();

        var gridSize = Math.Max(2, (int)Math.Ceiling(Math.Sqrt(SamplePointCount)));
        var index = 0;
        for (var gridY = 0; gridY < gridSize; gridY++)
        {
            for (var gridX = 0; gridX < gridSize; gridX++)
            {
                if (index >= SamplePointCount)
                {
                    break;
                }

                var x = width == 1 ? 0 : (int)Math.Round(gridX * (width - 1d) / Math.Max(1, gridSize - 1));
                var y = height == 1 ? 0 : (int)Math.Round(gridY * (height - 1d) / Math.Max(1, gridSize - 1));
                if (seen.Add((x, y)))
                {
                    index++;
                    points.Add(new SamplePoint(x, y, index / (double)SamplePointCount));
                }
            }
        }

        if (seen.Add((width / 2, height / 2)))
        {
            points.Add(new SamplePoint(width / 2, height / 2, 1d));
        }

        return points;
    }

    private static Bitmap ConvertToArgb32(Bitmap source)
    {
        if (source.PixelFormat == PixelFormat.Format32bppArgb)
        {
            return (Bitmap)source.Clone();
        }

        var converted = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(converted);
        graphics.DrawImage(source, new Rectangle(0, 0, converted.Width, converted.Height));
        return converted;
    }

    private static List<int> BuildAxis(int start, int end, int step)
    {
        var values = new List<int>();
        for (var value = start; value <= end; value += step)
        {
            values.Add(value);
        }

        return values;
    }

    private static IEnumerable<int> OrderAxis(IReadOnlyList<int> values, ImageScanDirection scanDirection)
    {
        return scanDirection switch
        {
            ImageScanDirection.BottomToTop => values.Reverse(),
            ImageScanDirection.RightToLeft => values.Reverse(),
            ImageScanDirection.CenterOutVertical => OrderFromCenter(values),
            ImageScanDirection.CenterOutHorizontal => OrderFromCenter(values),
            _ => values
        };
    }

    private static IEnumerable<int> OrderFromCenter(IReadOnlyList<int> values)
    {
        if (values.Count == 0)
        {
            yield break;
        }

        var centerIndex = values.Count / 2;
        yield return values[centerIndex];

        for (var offset = 1; offset < values.Count; offset++)
        {
            var leftIndex = centerIndex - offset;
            var rightIndex = centerIndex + offset;

            if (leftIndex >= 0)
            {
                yield return values[leftIndex];
            }

            if (rightIndex < values.Count)
            {
                yield return values[rightIndex];
            }
        }
    }

    private static bool IsColumnFirst(ImageScanDirection scanDirection)
    {
        return scanDirection is ImageScanDirection.LeftToRight
            or ImageScanDirection.RightToLeft
            or ImageScanDirection.CenterOutHorizontal;
    }

    private static void AddIfDistinct(ICollection<ImageMatchHit> hits, ImageMatchHit candidate, Size templateSize)
    {
        foreach (var hit in hits)
        {
            var deltaX = Math.Abs(hit.Location.X - candidate.Location.X);
            var deltaY = Math.Abs(hit.Location.Y - candidate.Location.Y);
            if (deltaX < templateSize.Width && deltaY < templateSize.Height)
            {
                return;
            }
        }

        hits.Add(candidate);
    }

    private static ImageMatchResult CreateSuccess(ImageMatchHit hit, long elapsedMilliseconds, string message)
    {
        return new ImageMatchResult(true, hit.Location, hit.Score, elapsedMilliseconds, message, new[] { hit });
    }

    private static ImageMatchResult CreateFailure(string message)
    {
        return new ImageMatchResult(false, null, 0d, 0L, message, Array.Empty<ImageMatchHit>());
    }

    private static string GetScanDirectionText(ImageScanDirection scanDirection)
    {
        return scanDirection switch
        {
            ImageScanDirection.BottomToTop => "从下到上",
            ImageScanDirection.CenterOutVertical => "从中间向上下扩散",
            ImageScanDirection.LeftToRight => "从左到右，按列扫描",
            ImageScanDirection.RightToLeft => "从右到左，按列扫描",
            ImageScanDirection.CenterOutHorizontal => "从中间向两侧扩散，按列扫描",
            _ => "从上到下"
        };
    }

    private sealed class PixelBuffer
    {
        private PixelBuffer(byte[] buffer, int width, int height, int stride, bool isGrayscale)
        {
            Buffer = buffer;
            Width = width;
            Height = height;
            Stride = stride;
            IsGrayscale = isGrayscale;
            ChannelDenominator = isGrayscale ? 255d : 255d * 3d;
            Size = new Size(width, height);
            PixelCount = width * height;
        }

        public byte[] Buffer { get; }

        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public bool IsGrayscale { get; }

        public double ChannelDenominator { get; }

        public int PixelCount { get; }

        public Size Size { get; }

        public int GetOffset(int x, int y)
        {
            return IsGrayscale ? y * Stride + x : y * Stride + x * 4;
        }

        public static PixelBuffer Create(Bitmap bitmap, bool grayscale)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var raw = new byte[Math.Abs(data.Stride) * data.Height];
                Marshal.Copy(data.Scan0, raw, 0, raw.Length);

                if (!grayscale)
                {
                    return new PixelBuffer(raw, bitmap.Width, bitmap.Height, Math.Abs(data.Stride), false);
                }

                var gray = new byte[bitmap.Width * bitmap.Height];
                var grayIndex = 0;
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var rowOffset = y * Math.Abs(data.Stride);
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var pixelOffset = rowOffset + x * 4;
                        var blue = raw[pixelOffset];
                        var green = raw[pixelOffset + 1];
                        var red = raw[pixelOffset + 2];
                        gray[grayIndex++] = (byte)Math.Round(red * 0.299 + green * 0.587 + blue * 0.114);
                    }
                }

                return new PixelBuffer(gray, bitmap.Width, bitmap.Height, bitmap.Width, true);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }

    private readonly record struct SamplePoint(int X, int Y, double CutoffDifference);
}
