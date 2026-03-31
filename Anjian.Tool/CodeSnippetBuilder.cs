using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Anjian;

public static class CodeSnippetBuilder
{
    public static string FormatRectangleDeclaration(string variableName, Rectangle region)
    {
        var safeVariableName = string.IsNullOrWhiteSpace(variableName) ? "rect" : variableName.Trim();
        return $"var {safeVariableName} = new Rectangle({region.X}, {region.Y}, {region.Width}, {region.Height});";
    }

    public static CodeSnippetResult BuildRectangleDeclaration(string variableName, Rectangle region)
    {
        var code = FormatRectangleDeclaration(variableName, region);

        return new CodeSnippetResult("区域定义代码", code, "生成可直接用于截图、找图或 OCR 的 Rectangle 定义。");
    }

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

        return new CodeSnippetResult("鼠标左键代码", code, "移动到目标坐标后执行一次鼠标左键单击。");
    }

    public static CodeSnippetResult BuildMouseRightClick(int x, int y)
    {
        var code = $$"""
            var mouse = new Win32MouseService();
            mouse.RightClick({{x}}, {{y}});
            """;

        return new CodeSnippetResult("鼠标右键代码", code, "移动到目标坐标后执行一次鼠标右键单击。");
    }

    public static CodeSnippetResult BuildMouseDoubleClick(int x, int y, int intervalMilliseconds = 80)
    {
        var code = $$"""
            var mouse = new Win32MouseService();
            mouse.LeftDoubleClick({{x}}, {{y}}, {{intervalMilliseconds}});
            """;

        return new CodeSnippetResult("鼠标双击代码", code, "移动到目标坐标后执行一次鼠标左键双击。");
    }

    public static CodeSnippetResult BuildKeyboardTextInput(string text, KeyboardTextInputOptions? options = null)
    {
        var actualOptions = options ?? new KeyboardTextInputOptions();
        var escapedText = EscapeString(text);
        string code;

        if (actualOptions.Mode == KeyboardTextInputMode.SendInput)
        {
            code = $$"""
                var keyboard = new Win32KeyboardService();
                keyboard.TextInput(
                    "{{escapedText}}",
                    new KeyboardTextInputOptions(
                        KeyboardTextInputMode.SendInput,
                        perCharacterDelayMs: {{actualOptions.PerCharacterDelayMs}}));
                """;
        }
        else
        {
            code = $$"""
                var keyboard = new Win32KeyboardService();
                keyboard.TextInput(
                    "{{escapedText}}",
                    new KeyboardTextInputOptions(
                        KeyboardTextInputMode.ClipboardPaste,
                        restoreClipboard: {{ToBooleanLiteral(actualOptions.RestoreClipboard)}},
                        pasteMode: KeyboardPasteMode.{{actualOptions.PasteMode}},
                        clipboardSettleDelayMs: {{actualOptions.ClipboardSettleDelayMs}},
                        pasteSettleDelayMs: {{actualOptions.PasteSettleDelayMs}}));
                """;
        }

        var title = actualOptions.Mode == KeyboardTextInputMode.ClipboardPaste
            ? "剪贴板输入代码"
            : "键盘文本输入代码";
        var description = actualOptions.Mode == KeyboardTextInputMode.ClipboardPaste
            ? "通过剪贴板和粘贴动作发送整段文本，适合中文和远程桌面。"
            : "向当前焦点控件逐字发送文本。";

        return new CodeSnippetResult(title, code, description);
    }

    public static CodeSnippetResult BuildKeyboardKeyPress(Keys key)
    {
        var code = $$"""
            var keyboard = new Win32KeyboardService();
            keyboard.KeyPress(Keys.{{key}});
            """;

        return new CodeSnippetResult("键盘按键代码", code, $"发送一次 {key} 按键。");
    }

    public static CodeSnippetResult BuildKeyboardHotKey(IEnumerable<Keys> keys)
    {
        var keyList = keys.ToArray();
        var joinedKeys = string.Join(", ", keyList.Select(key => $"Keys.{key}"));
        var code = $$"""
            var keyboard = new Win32KeyboardService();
            keyboard.HotKey({{joinedKeys}});
            """;

        return new CodeSnippetResult("键盘组合键代码", code, $"发送组合键：{string.Join("+", keyList)}");
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

    public static CodeSnippetResult BuildTextOcr(Rectangle region, GeneralOcrOptions options)
    {
        var code = $$"""
            var captureService = new ScreenCaptureService();
            var imagePreprocess = new ImagePreprocessService();
            var runtimeOptions = new GeneralOcrRuntimeOptions(
                Provider: OcrEngineType.PaddleSharp,
                Device: GeneralOcrDeviceType.CpuMkl);
            var ocrService = GeneralOcrServiceFactory.Create(imagePreprocess, runtimeOptions);

            using var sourceBitmap = captureService.Capture(new Rectangle({{region.X}}, {{region.Y}}, {{region.Width}}, {{region.Height}}));
            var ocrOptions = new GeneralOcrOptions(
                {{ToBooleanLiteral(options.UseGrayscale)}},
                {{ToBooleanLiteral(options.UseBinarization)}},
                {{options.BinarizationThreshold}},
                {{ToBooleanLiteral(options.Scale2x)}});

            var result = await ocrService.RecognizeRegionsAsync(sourceBitmap, ocrOptions);
            if (result.Success)
            {
                Console.WriteLine(result.NormalizedText);
                foreach (var region in result.SafeRegions)
                {
                    Console.WriteLine($"{region.Text} @ ({region.Center.X}, {region.Center.Y})");
                }
            }
            """;

        return new CodeSnippetResult("文字识别代码", code, "已包含区域截图、通用 OCR 参数和文字识别调用。");
    }

    private static string EscapePath(string path)
    {
        return Path.GetFullPath(path).Replace("\"", "\"\"");
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static string ToBooleanLiteral(bool value)
    {
        return value ? "true" : "false";
    }
}
