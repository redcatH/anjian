using System.Drawing;
using System.Globalization;
using System.IO;

namespace Anjian;

public static class CodeSnippetBuilder
{
    public static CodeSnippetResult BuildMouseMove(int x, int y)
    {
        var code = $$"""
            var mouse = new Win32MouseService();
            mouse.MoveTo({{x}}, {{y}});
            """;

        return new CodeSnippetResult("鼠标移动代码", code, "将鼠标移动到指定屏幕坐标。");
    }

    public static CodeSnippetResult BuildMouseClick(int x, int y)
    {
        var code = $$"""
            var mouse = new Win32MouseService();
            mouse.LeftClick({{x}}, {{y}});
            """;

        return new CodeSnippetResult("鼠标单击代码", code, "移动到目标坐标后执行一次左键单击。");
    }

    public static CodeSnippetResult BuildMouseDoubleClick(int x, int y, int intervalMilliseconds = 80)
    {
        var code = $$"""
            var mouse = new Win32MouseService();
            mouse.LeftDoubleClick({{x}}, {{y}}, {{intervalMilliseconds}});
            """;

        return new CodeSnippetResult("鼠标双击代码", code, "移动到目标坐标后执行一次左键双击。");
    }

    public static CodeSnippetResult BuildImageMatch(
        Rectangle region,
        string templatePath,
        ImageMatchOptions options)
    {
        var path = EscapePath(templatePath);
        var threshold = options.Threshold.ToString("0.00", CultureInfo.InvariantCulture);
        var code = $$"""
            using var sourceBitmap = new ScreenCaptureService().Capture(new Rectangle({{region.X}}, {{region.Y}}, {{region.Width}}, {{region.Height}}));
            using var templateBitmap = new Bitmap(@"{{path}}");

            var matcher = new TemplateMatcher();
            var matchOptions = new ImageMatchOptions(
                new Rectangle(0, 0, {{region.Width}}, {{region.Height}}),
                {{threshold}},
                {{ToBooleanLiteral(options.UseGrayscale)}},
                {{options.Step}},
                ImageMatchMode.{{options.MatchMode}},
                ImageScanDirection.{{options.ScanDirection}});

            var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
            if (result.Success)
            {
                var screenPoint = new Point(result.Location!.Value.X + {{region.X}}, result.Location.Value.Y + {{region.Y}});
                Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
            }
            """;

        return new CodeSnippetResult("找图代码", code, "已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。");
    }

    public static CodeSnippetResult BuildAmountOcr(Rectangle region, AmountOcrOptions options)
    {
        var code = $$"""
            var captureService = new ScreenCaptureService();
            var preprocessService = new ImagePreprocessService();
            var ocrService = new TesseractAmountOcrService();

            using var sourceBitmap = captureService.Capture(new Rectangle({{region.X}}, {{region.Y}}, {{region.Width}}, {{region.Height}}));
            var ocrOptions = new AmountOcrOptions(
                {{ToBooleanLiteral(options.UseGrayscale)}},
                {{ToBooleanLiteral(options.UseBinarization)}},
                {{options.BinarizationThreshold}},
                {{ToBooleanLiteral(options.Scale2x)}});

            using var processedBitmap = preprocessService.Preprocess(sourceBitmap, ocrOptions);
            var result = await ocrService.RecognizeAmountAsync(processedBitmap);
            if (result.Success)
            {
                Console.WriteLine($"金额: {result.Amount}");
            }
            """;

        return new CodeSnippetResult("金额识别代码", code, "已包含固定区域截图、预处理参数和金额识别调用。");
    }

    private static string EscapePath(string path)
    {
        return Path.GetFullPath(path).Replace("\"", "\"\"");
    }

    private static string ToBooleanLiteral(bool value)
    {
        return value ? "true" : "false";
    }
}
