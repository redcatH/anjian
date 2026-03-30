using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Anjian;

public sealed class TemplateMatcher : IImageMatcher
{
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

        var watch = Stopwatch.StartNew();
        var hits = new List<ImageMatchHit>();
        var bestScore = double.MinValue;
        Point? bestLocation = null;

        var maxX = options.SearchRegion.Right - template.Width;
        var maxY = options.SearchRegion.Bottom - template.Height;

        var xPositions = BuildAxis(options.SearchRegion.Left, maxX, options.Step);
        var yPositions = BuildAxis(options.SearchRegion.Top, maxY, options.Step);

        var columnFirst = IsColumnFirst(options.ScanDirection);
        var orderedXPositions = columnFirst ? OrderAxis(xPositions, options.ScanDirection) : xPositions;
        var orderedYPositions = columnFirst ? yPositions : OrderAxis(yPositions, options.ScanDirection);

        // 通过扫描方向调整遍历顺序；在“只找第一个”模式下会直接影响提前命中的速度。
        if (columnFirst)
        {
            foreach (var x in orderedXPositions)
            {
                foreach (var y in orderedYPositions)
                {
                    if (TryHandleCandidate(source, template, options, hits, x, y, ref bestScore, ref bestLocation, watch, out var successResult))
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
                    if (TryHandleCandidate(source, template, options, hits, x, y, ref bestScore, ref bestLocation, watch, out var successResult))
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
        Bitmap source,
        Bitmap template,
        ImageMatchOptions options,
        ICollection<ImageMatchHit> hits,
        int x,
        int y,
        ref double bestScore,
        ref Point? bestLocation,
        Stopwatch watch,
        out ImageMatchResult successResult)
    {
        successResult = default!;

        var score = CalculateScore(source, template, x, y, options.UseGrayscale);
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

    private static double CalculateScore(Bitmap source, Bitmap template, int startX, int startY, bool useGrayscale)
    {
        double totalDifference = 0d;
        var pixelCount = template.Width * template.Height;
        var denominator = useGrayscale ? 255d : 255d * 3d;

        for (var y = 0; y < template.Height; y++)
        {
            for (var x = 0; x < template.Width; x++)
            {
                var sourceColor = source.GetPixel(startX + x, startY + y);
                var templateColor = template.GetPixel(x, y);

                totalDifference += useGrayscale
                    ? Math.Abs(ToGray(sourceColor) - ToGray(templateColor))
                    : Math.Abs(sourceColor.R - templateColor.R) +
                      Math.Abs(sourceColor.G - templateColor.G) +
                      Math.Abs(sourceColor.B - templateColor.B);
            }
        }

        // 将像素差异归一化到 0-1，再转成“越大越相似”的分数。
        var normalizedDifference = totalDifference / (pixelCount * denominator);
        return 1d - normalizedDifference;
    }

    private static int ToGray(Color color)
    {
        return (int)Math.Round(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
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
}
