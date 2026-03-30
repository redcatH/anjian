using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Anjian;

internal static class Program
{
    private static void Main()
    {
        const ExecutionMode mode = ExecutionMode.RunUntilWait;

        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Anjian.Runner 已启动。");
        Console.WriteLine($"当前执行模式：{GetModeText(mode)}");
        Console.WriteLine("调试控制：全局按 F3 继续。");

        var mouse = new Win32MouseService();
        var matcher = new TemplateMatcher();
        var ocr = new TesseractAmountOcrService();
        var keyboard = new Win32KeyboardService();
        var capture = new ScreenCaptureService();
        var imagePreprocess = new ImagePreprocessService();
        using var continueSignal = new GlobalF3ContinueSignal();

        var context = new AutomationContext(
            mouse,
            keyboard,
            matcher,
            ocr,
            capture,
            imagePreprocess,
            new ConsoleExecutionController(mode, continueSignal));

        ImageMatchResult? submitButton = null;
        AmountOcrResult? amount = null;

        // 已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。
        using var sourceBitmap = new ScreenCaptureService().Capture(new Rectangle(2000, 397, 800, 600));
        using var templateBitmap = new Bitmap(@"C:\Users\Administrator\Documents\ShareX\Screenshots\2026-03\wps_peqwB6lR7K.png");

        // var matcher = new TemplateMatcher();
        var matchOptions = new ImageMatchOptions(
            new Rectangle(0, 0, 800, 600),
            0.90,
            true,
            2,
            ImageMatchMode.First,
            ImageScanDirection.TopToBottom);

        var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        if (result.Success)
        {
            var screenPoint = new Point(result.Location!.Value.X + 2000, result.Location.Value.Y + 397);
            Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        }
        
        var steps = new IAutomationStep[]
        {
            // new MouseMoveStep(2203, 386),
            // new DelayStep(1000),
            // new LeftDoubleClickStep(2203, 386, 80),
            // new DelayStep(1000),
            // new LeftClickStep(2292, 443),
            // new TextInputStep("1233333"),
            // new WaitStep("请检查业务系统当前页面，确认后按 F3 继续。"),
            // new DelegateStep("查找提交按钮", () => submitButton = FindImage(
            //     capture,
            //     matcher,
            //     @"D:\templates\submit.png",
            //     new ImageMatchOptions(new Rectangle(0, 0, 400, 300), 0.90))),
            

            new DelegateStep("点击提交按钮", () => ClickMatch(mouse, result!)),
            // new DelegateStep("识别金额", () => amount = ReadAmount(
            //     capture,
            //     imagePreprocess,
            //     ocr,
            //     new Rectangle(0, 0, 220, 70),
            //     new AmountOcrOptions())),
            // new DelegateStep("输入金额", () => keyboard.TextInput(amount!.NormalizedText)),
            new WaitStep("请检查业务系统当前页面，确认后按 F3 继续。"),
            new TextInputStep("6666"),
            new HotKeyStep(Keys.Enter)
        };

        try
        {
            var runner = new AutomationRunner();
            runner.Run(steps, context);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 全部步骤执行完成。");
        }
        catch (AutomationStepExecutionException ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 执行失败：步骤 {ex.StepIndex + 1}，名称={ex.StepName}");
            Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 执行失败：{ex.Message}");
        }

        Console.WriteLine("可用服务：Win32MouseService、TemplateMatcher、TesseractAmountOcrService、Win32KeyboardService");
        Console.WriteLine("示例区域：{0}", new Rectangle(0, 0, 300, 120));
    }

    private static ImageMatchResult FindImage(
        ScreenCaptureService capture,
        IImageMatcher matcher,
        string templatePath,
        ImageMatchOptions options)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("未找到模板图片。", templatePath);
        }

        using var source = capture.Capture(options.SearchRegion);
        using var template = (Bitmap)Image.FromFile(templatePath);

        var localOptions = options with
        {
            SearchRegion = new Rectangle(0, 0, source.Width, source.Height)
        };

        var result = matcher.Find(source, template, localOptions);
        if (result.Location is null)
        {
            return result;
        }

        var absoluteLocation = new Point(
            result.Location.Value.X + options.SearchRegion.X,
            result.Location.Value.Y + options.SearchRegion.Y);

        return result with { Location = absoluteLocation };
    }

    private static void ClickMatch(IMouseService mouse, ImageMatchResult result)
    {
        ArgumentNullException.ThrowIfNull(mouse);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Success || result.Location is null)
        {
            throw new InvalidOperationException($"找图失败，无法点击。消息：{result.Message}");
        }

        mouse.LeftClick(result.Location.Value.X, result.Location.Value.Y);
    }

    private static AmountOcrResult ReadAmount(
        ScreenCaptureService capture,
        ImagePreprocessService imagePreprocess,
        IOcrService ocr,
        Rectangle region,
        AmountOcrOptions options)
    {
        using var source = capture.Capture(region);
        using var processed = imagePreprocess.Preprocess(source, options);
        return ocr.RecognizeAmountAsync(processed).GetAwaiter().GetResult();
    }

    private static string GetModeText(ExecutionMode mode)
    {
        return mode switch
        {
            ExecutionMode.StepByStep => "逐步模式",
            ExecutionMode.RunUntilWait => "运行到 Wait 模式",
            _ => mode.ToString()
        };
    }
}
