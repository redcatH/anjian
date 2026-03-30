using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Anjian;

internal sealed class SettlementWorkflow
{
    public void Run(string number, AutomationContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentNullException.ThrowIfNull(context);

        AmountOcrResult? recognizedAmount = null;

        var mouse = context.Mouse;
        var matcher = context.Matcher;
        var ocr = context.Ocr;
        var keyboard = context.Keyboard;
        var capture = context.Capture;
        var imagePreprocess = context.ImagePreprocess;

        // 已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。
        // var searchRegion = new Rectangle(2000, 397, 800, 600);
        //
        // using var sourceBitmap = new ScreenCaptureService().Capture(searchRegion);
        // using var templateBitmap = new Bitmap(@"C:\Users\Administrator\Documents\ShareX\Screenshots\2026-03\wps_peqwB6lR7K.png");
        //
        // var matchOptions = new ImageMatchOptions(
        //     new Rectangle(0, 0, 800, 600),
        //     0.90,
        //     true,
        //     2,
        //     ImageMatchMode.First,
        //     ImageScanDirection.TopToBottom);
        //
        // var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        // if (result.Success)
        // {
        //     var screenPoint = new Point(result.Location!.Value.X + searchRegion.X, result.Location.Value.Y + searchRegion.Y);
        //     Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        // }

        var departmentRegion = new Rectangle(2175, -37, 100, 50);
        var departmentTemplatePath = @"D:\work\anjian\Anjian.Runner\内科.png";

        // 已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。
        // using var sourceBitmap = new ScreenCaptureService().Capture(new Rectangle(2175, -37, 100, 50));
        // using var templateBitmap = new Bitmap(@"C:\Users\Administrator\Documents\ShareX\Screenshots\2026-03\内科.png");
        //
        // var matcher = new TemplateMatcher();
        // var matchOptions = new ImageMatchOptions(
        //     new Rectangle(0, 0, 100, 50),
        //     0.90,
        //     true,
        //     1,
        //     ImageMatchMode.First,
        //     ImageScanDirection.TopToBottom);
        //
        // var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        // if (result.Success)
        // {
        //     var screenPoint = new Point(result.Location!.Value.X + 2175, result.Location.Value.Y - 37);
        //     Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        // }

        var yjsRegion = new Rectangle(2350, 800, 200, 100);
        var yjsTemplatePath = @"D:\work\anjian\Anjian.Runner\预结算.png";

        // 已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。
        // using var sourceBitmap = new ScreenCaptureService().Capture(new Rectangle(2350, 800, 200, 100));
        // using var templateBitmap = new Bitmap(@"C:\Users\Administrator\Documents\ShareX\Screenshots\2026-03\预结算.png");
        //
        // var matcher = new TemplateMatcher();
        // var matchOptions = new ImageMatchOptions(
        //     new Rectangle(0, 0, 200, 100),
        //     0.90,
        //     true,
        //     1,
        //     ImageMatchMode.First,
        //     ImageScanDirection.TopToBottom);
        //
        // var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        // if (result.Success)
        // {
        //     var screenPoint = new Point(result.Location!.Value.X + 2350, result.Location.Value.Y + 800);
        //     Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        // }

        var sbicRegion = new Rectangle(2539, 813, 200, 100);
        var sbicTemplatePath = @"D:\work\anjian\Anjian.Runner\社保IC卡.png";

        // 已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。
        // using var sourceBitmap = new ScreenCaptureService().Capture(new Rectangle(2539, 813, 200, 100));
        // using var templateBitmap = new Bitmap(@"C:\Users\Administrator\Documents\ShareX\Screenshots\2026-03\社保IC卡.png");
        //
        // var matcher = new TemplateMatcher();
        // var matchOptions = new ImageMatchOptions(
        //     new Rectangle(0, 0, 200, 100),
        //     0.90,
        //     true,
        //     1,
        //     ImageMatchMode.First,
        //     ImageScanDirection.TopToBottom);
        //
        // var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        // if (result.Success)
        // {
        //     var screenPoint = new Point(result.Location!.Value.X + 2539, result.Location.Value.Y + 813);
        //     Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        // }

        // 已包含搜索区域、模板路径、阈值、匹配模式和扫描方向。
        // using var sourceBitmap = new ScreenCaptureService().Capture(new Rectangle(2178, 5, 60, 800));
        // using var templateBitmap = new Bitmap(@"C:\Users\Administrator\Documents\ShareX\Screenshots\2026-03\mstsc_HrLGqbKqrN.png");
        //
        // var matcher = new TemplateMatcher();
        var zhOptions = new ImageMatchOptions(
            new Rectangle(0, 0, 60, 800),
            0.90,
            true,
            1,
            ImageMatchMode.First,
            ImageScanDirection.BottomToTop);
        //
        // var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        // if (result.Success)
        // {
        //     var screenPoint = new Point(result.Location!.Value.X + 2178, result.Location.Value.Y + 5);
        //     Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        // }
        var zhRegion = new Rectangle(2178, 5, 60, 800);
        var pdRegion = new Rectangle(2207, 90, 100, 600);
        
        
        var zhTemplatePath = @"D:\work\anjian\Anjian.Runner\最后.png";

        // 已包含固定区域截图、预处理参数和金额识别调用。
        // var captureService = new ScreenCaptureService();
        // var preprocessService = new ImagePreprocessService();
        // var ocrService = new TesseractAmountOcrService();
        //
        // using var sourceBitmap = captureService.Capture(new Rectangle(2755, 303, 400, 60));
        // var ocrOptions = new AmountOcrOptions(
        //     true,
        //     false,
        //     160,
        //     false);
        //
        // using var processedBitmap = preprocessService.Preprocess(sourceBitmap, ocrOptions);
        // var result = await ocrService.RecognizeAmountAsync(processedBitmap);
        // if (result.Success)
        // {
        //     Console.WriteLine($"金额: {result.Amount}");
        // }
        var amountRegion = new Rectangle(2755, 303, 400, 60);
        var amountOptions = new AmountOcrOptions(
            true,
            false,
            160,
            false);

        var steps = new IAutomationStep[]
        {
            //点击住院结算
            new MouseMoveStep(1942, 112),
            new LeftClickStep(1942, 112),
            new DelayStep(1000),
            
            //点击病例编号input
            new LeftClickStep(2226, -56),
            new DelayStep(500),
            new DelegateStep("清空", () => { ClearInput(keyboard); }),
            new DelayStep(1500),
            new TextInputStep(number),
            new DelayStep(500),
            //点击出院科室 会等待5000毫秒，如果病人有科室 就自动出现，如果没有后续会输入nk 然后回车
            new LeftClickStep(2222, -19),
            new DelegateStep("检查目标图片，不存在则跳过当前号码", () =>
            {
                var result = FindImage(
                    capture,
                    matcher,
                    zhTemplatePath,
                    zhOptions with { SearchRegion = pdRegion });

                if (!result.Success || result.Location is null)
                {
                    throw new SkipCurrentRunException("未找到目标图片，跳过当前号码。");
                }
            }),
            new DelayStep(5000),
            new LeftClickStep(2222, -19),
            
            // 输入 nk 按回车会快速联想，所以需要等待。如果有内科就不需要输入了
            new ConditionalStep(
                "如果未找到内科则输入 nk 并回车",
                _ =>
                {
                    var exists = HasImage(capture, matcher, departmentRegion, departmentTemplatePath);
                    Console.WriteLine($"内科：{(exists ? "存在" : "不存在")}");
                    return !exists;
                },
                new IAutomationStep[]
                {
                    new TextInputStep("nk"),
                    new DelayStep(1000),
                    new HotKeyStep(Keys.Enter),
                    new DelayStep(1000)
                }),
            
            //需要勾选预结算
            new ConditionalStep(
                "如果没有勾选预结算",
                _ =>
                {
                    var checkedYjs = HasImage(capture, matcher, yjsRegion, yjsTemplatePath);
                    Console.WriteLine($"预结算：{(checkedYjs ? "已勾选" : "未勾选")}");
                    return !checkedYjs;
                },
                new IAutomationStep[]
                {
                    // 勾选预结算
                    new MouseMoveStep(2403, 837),
                    new LeftClickStep(2403, 837),
                }),
            new LeftClickStep(729, 973),
            new WaitStep("按F3 检查勾选"),
            //需要勾选社保IC卡M
            new ConditionalStep(
                "如果没有勾选选择社保IC卡",
                _ =>
                {
                    var exists = HasImage(capture, matcher, sbicRegion, sbicTemplatePath);
                    Console.WriteLine($"社保IC卡：{(exists ? "存在" : "不存在")}");
                    return !exists;
                },
                new IAutomationStep[]
                {
                    // 点击出现下拉框
                    new MouseMoveStep(2652, 835),
                    new LeftClickStep(2652, 835),
                    // 勾选社保IC卡
                    new MouseMoveStep(2602, 788),
                    new LeftClickStep(2602, 788)
                }),
            //TODO: 这里需要打钩
            // new WaitStep("请确认所有勾选，F3会点击联网结算，确认后按 F3 继续。"),
            // new MouseMoveStep(2515, 831),
            // new LeftClickStep(2515, 831),
            // new DelayStep(5000),
            // new HotKeyStep(Keys.Enter),
            // new DelayStep(4000),
            // new HotKeyStep(Keys.Enter),
            // new DelayStep(4000),
            
            new LeftClickStep(729, 973),
            new WaitStep("按F3 开始识别金额"),
            
            new DelegateStep("识别金额并保存", () =>
            {
            
                using var sourceBitmap = capture.Capture(new Rectangle(2767, 301, 148, 33));
                var ocrOptions = new AmountOcrOptions(
                    true,
                    true,
                    160,
                    false);
            
                using var processedBitmap = imagePreprocess.Preprocess(sourceBitmap, ocrOptions);
                recognizedAmount = ocr.RecognizeAmountAsync(processedBitmap).Result;
                
                if (!recognizedAmount.Success)
                {
                    throw new InvalidOperationException($"金额识别失败：{recognizedAmount.Message}");
                }
            
                Console.WriteLine($"金额：{recognizedAmount.Amount}，标准文本：{recognizedAmount.NormalizedText}");
            }),
            // new WaitStep("识别成功后请按F3 继续"),
            new DelayStep(500),
            new LeftClickStep(2607, 349),
            // 2607, 349
            new DelayStep(2000),
            //1943, 16  医保费用
            new MouseMoveStep(1943, 16),
            new LeftClickStep(1943, 16),
            new DelayStep(1000),
            new LeftClickStep(2234, -61),
            //清空 然后再输入
            new DelegateStep("清空input", () => { ClearInput(keyboard); }),
            new DelayStep(1000),
            new TextInputStep(number),
            new HotKeyStep(Keys.Enter),
            new DelayStep(2000),
            // new HotKeyStep(Keys.Enter),
            // new DelayStep(5000),
            //右键出现菜单 点击新增记录
            new RightClickStep(2406, 243),
            new DelayStep(6000),
            new MouseMoveStep(2406 + 67, 243 + 134),
            new LeftClickStep(2406+67,243+134),//TODO 点击新增记录
            new DelayStep(3000),
            new HotKeyStep(Keys.Enter),
            //选择本院费用类型
            //TODO 新增条目后 需要先 选择左上角 手工录入本床护理费
            new LeftClickStep(729, 973),
            new WaitStep("按F3 手工录入本床护理费"),
            // new DelayStep(1500),//2225, -20
            // new MouseMoveStep(2225, -20),
            // new LeftClickStep(2225, -20),
            // new DelayStep(1000),
            // new LeftClickStep(2225, -20+58),
            // new DelayStep(1500),
            //TODO 本院编码输入 jjzh 回车, 显示居家照护
            new LeftClickStep(2357, 0),
            new TextInputStep("jjzh"),
            new HotKeyStep(Keys.Enter),
            new DelayStep(2500),
            //TODO 然后输入 预结算记录的数字，可直接使用 recognizedAmount.NormalizedText 或 recognizedAmount.Amount
            new LeftClickStep(2169, 46),
            new DelegateStep("输入金额", () =>
            {
                context.Keyboard.HotKey(Keys.Back);
                context.Keyboard.TextInput(recognizedAmount!.RawText);
            }),
            
            //这里需要增加 查询图片 如果图片不存在 就 跳过 整个流程 直接进入下一个号码
            //为了找到新增的条目，然后改变费用类型
            new DelegateStep("查找新增条目", () =>
            {
                var result = FindImage(
                    capture,
                    matcher,
                    zhTemplatePath,
                    zhOptions with { SearchRegion = zhRegion });

                if (result.Success && result.Location is not null)
                {
                    mouse.MoveTo(result.Location.Value.X + 181, result.Location.Value.Y + 30);
                    mouse.LeftClick(result.Location.Value.X + 181, result.Location.Value.Y + 30);
                    mouse.RightClick(result.Location.Value.X + 181, result.Location.Value.Y + 30);
                    mouse.MoveTo(result.Location.Value.X + 181 + 70, result.Location.Value.Y + 30 + 13);
                    mouse.LeftClick(result.Location.Value.X + 181 + 70, result.Location.Value.Y + 30 + 13);
                    mouse.MoveTo(result.Location.Value.X + 181 + 70, result.Location.Value.Y + 30 + 13);
                    mouse.LeftClick(result.Location.Value.X +181+70+136, result.Location.Value.Y+30+13+80); //TODO 暂时不点击
                    
                }
            }),
            new DelayStep(1500),

            //点击住院结算
            new MouseMoveStep(1942, 112),
            new LeftClickStep(1942, 112),
            new DelayStep(1000),

            //取消预勾选
            new ConditionalStep(
                "如果已经勾选预结算",
                _ =>
                {
                    var checkedYjs = HasImage(capture, matcher, yjsRegion, yjsTemplatePath);
                    Console.WriteLine($"预结算：{(checkedYjs ? "已勾选" : "未勾选")}");
                    return checkedYjs;
                },
                new IAutomationStep[]
                {
                    // 取消
                    new MouseMoveStep(2403, 837),
                    new LeftClickStep(2403, 837),
                }),
            //点击结算即可完成一个人的结算
            new MouseMoveStep(2515, 831),
            new LeftClickStep(2515, 831)//TODO 暂时不开放点击
        };

        var runner = new AutomationRunner();
        runner.Run(steps, context);
    }

    private static void ClearInput(IKeyboardService keyboard)
    {
        keyboard.HotKey(Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back);
        Thread.Sleep(500);
        keyboard.HotKey(Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back);
        Thread.Sleep(500);
        keyboard.HotKey(Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back, Keys.Back);
        Thread.Sleep(500);
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

    private static void ClickMatch(IMouseService mouse, ImageMatchResult result, Rectangle searchRegion)
    {
        ArgumentNullException.ThrowIfNull(mouse);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Success || result.Location is null)
        {
            throw new InvalidOperationException($"找图失败，无法点击。消息：{result.Message}");
        }

        if (searchRegion.Width <= 0 || searchRegion.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(searchRegion), "搜索区域宽高必须大于 0。");
        }

        var screenPoint = new Point(
            result.Location.Value.X + searchRegion.X,
            result.Location.Value.Y + searchRegion.Y);

        mouse.LeftClick(screenPoint.X, screenPoint.Y);
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

    private static bool HasImage(
        ScreenCaptureService capture,
        IImageMatcher matcher,
        Rectangle captureRegion,
        string templatePath,
        double threshold = 0.90,
        bool useGrayscale = true,
        int step = 1,
        ImageMatchMode matchMode = ImageMatchMode.First,
        ImageScanDirection scanDirection = ImageScanDirection.TopToBottom)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(matcher);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("未找到模板图片。", templatePath);
        }

        if (captureRegion.Width <= 0 || captureRegion.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(captureRegion), "截图区域宽高必须大于 0。");
        }

        using var sourceBitmap = capture.Capture(captureRegion);
        using var templateBitmap = new Bitmap(templatePath);

        var matchOptions = new ImageMatchOptions(
            new Rectangle(0, 0, captureRegion.Width, captureRegion.Height),
            threshold,
            useGrayscale,
            step,
            matchMode,
            scanDirection);

        var result = matcher.Find(sourceBitmap, templateBitmap, matchOptions);
        if (result.Success && result.Location is not null)
        {
            var screenPoint = new Point(
                result.Location.Value.X + captureRegion.X,
                result.Location.Value.Y + captureRegion.Y);
            Console.WriteLine($"命中坐标: {screenPoint.X}, {screenPoint.Y}");
        }

        return result.Success;
    }
}
