using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        var generalOcr = context.GeneralOcr;
        var keyboard = context.Keyboard;
        var capture = context.Capture;
        var imagePreprocess = context.ImagePreprocess;

        var departmentRegion = new Rectangle(2175, -37, 100, 50);
        var departmentTemplatePath = GetRunnerAssetPath("内科.png");

        var yjsRegion = new Rectangle(2350, 800, 200, 100);
        var yjsTemplatePath = GetRunnerAssetPath("预结算.png");

        var sbicRegion = new Rectangle(2539, 813, 200, 100);
        var sbicTemplatePath = GetRunnerAssetPath("社保IC卡.png");

        var amountRegion = new Rectangle(2767, 301, 148, 33);
        var amountPageFeatureRegion = new Rectangle(2720, 280, 240, 100);
        var amountPageFeatureTemplatePath = GetRunnerAssetPath("金额页特征.png");
        var amountPageProbeOptions = new AmountOcrOptions(
            true,
            true,
            160,
            false);

        var zhOptions = new ImageMatchOptions(
            new Rectangle(0, 0, 60, 800),
            0.90,
            true,
            1,
            ImageMatchMode.First,
            ImageScanDirection.BottomToTop);

        var zhRegion = new Rectangle(2178, 5, 60, 800);
        var zhTemplatePath = GetRunnerAssetPath("最后.png");

        var closeButtonRegion = new Rectangle(2580, 320, 60, 60);
        var closeButtonTemplatePath = GetRunnerAssetPath("关闭按钮.png");

        var settlementDialogTextRegion = new Rectangle(2240, 180, 420, 220);
        var settlementDialogTextOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var unsavedEditPopupTextRegion = new Rectangle(2140, 220, 620, 220);
        var unsavedEditPopupTextOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var unsavedEditPopupButtonRegion = new Rectangle(2260, 540, 360, 140);
        var unsavedEditPopupButtonOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var settlementPopupSteps = new OcrPopupStep[]
        {
            // 示例：
            // new OcrPopupStep(
            //     "确认结算弹窗",
            //     settlementDialogTextRegion,
            //     settlementDialogTextOcrOptions,
            //     new[] { "确认结算", "是否继续" },
            //     new Rectangle(...),
            //     settlementDialogTextOcrOptions,
            //     regions => ChoosePopupButtonPoint(regions, "继续", "确定", "是"),
            //     delayAfterHandleMilliseconds: 1000),
        };
        var pdRegion = new Rectangle(2207, 90, 100, 600);
        var steps = new IAutomationStep[]
        {
            new MouseMoveStep(1942, 112),
            new LeftClickStep(1942, 112),
            new DelayStep(1000),

            new LeftClickStep(2226, -56),
            new DelayStep(500),
            new DelegateStep("清空", () => ClearInput(keyboard)),
            new DelayStep(1500),
            new TextInputStep(number),
            new DelayStep(500),
            new HotKeyStep(Keys.Enter),
            new DelayStep(2000),
            new DelegateStep("检查目标图片，不存在则跳过当前号码", () =>
            {
                var result = FindImage(
                    capture,
                    matcher,
                    zhTemplatePath,
                    zhOptions with { SearchRegion = pdRegion, UseGrayscale = false});

                if (!result.Success || result.Location is null)
                {
                    throw new SkipCurrentRunException("未找到目标图片，跳过当前号码。");
                }
            }),
            new WaitStep("选择继续或跳过本次。"),
            new LeftClickStep(2222, -19),

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

            new EnsureConditionStep(
                "确保已勾选预结算",
                _ =>
                {
                    var checkedYjs = HasImage(capture, matcher, yjsRegion, yjsTemplatePath, useGrayscale: false);
                    Console.WriteLine($"预结算：{(checkedYjs ? "已勾选" : "未勾选")}");
                    return checkedYjs;
                },
                new IAutomationStep[]
                {
                    new MouseMoveStep(2403, 837),
                    new LeftClickStep(2403, 837),
                },
                maxRetryAttempts: 5,
                retryDelayMilliseconds: 800,
                manualInterventionReason: "请确认“预结算”已勾选。若自动勾选失败，请手工处理后按 F3 继续，或按 F4 跳过本次。"),

            new EnsureConditionStep(
                "确保已勾选社保 IC 卡",
                _ =>
                {
                    var exists = HasImage(capture, matcher, sbicRegion, sbicTemplatePath);
                    Console.WriteLine($"社保IC卡：{(exists ? "已勾选" : "未勾选")}");
                    return exists;
                },
                new IAutomationStep[]
                {
                    new MouseMoveStep(2652, 835),
                    new LeftClickStep(2652, 835),
                    new MouseMoveStep(2602, 788),
                    new LeftClickStep(2602, 788)
                },
                maxRetryAttempts: 5,
                retryDelayMilliseconds: 800,
                manualInterventionReason: "请确认“社保IC卡”已勾选。若自动勾选失败，请手工处理后按 F3 继续，或按 F4 跳过本次。"),

            new SettlementNavigationStep(
                "点击结算后进入金额识别页",
                new IAutomationStep[]
                {
                    new MouseMoveStep(2515, 831),
                    new LeftClickStep(2515, 831),
                },
                ctx => IsAmountRecognitionPageReady(
                    ctx,
                    amountPageFeatureRegion,
                    amountPageFeatureTemplatePath,
                    amountRegion,
                    amountPageProbeOptions),
                settlementPopupSteps,
                maxIterations: 12,
                manualInterventionReason: "结算后未识别到已知弹窗，也尚未进入金额识别页。请手工处理当前弹窗或页面后按 F3 继续，或按 F4 跳过本次。",
                iterationDelayMilliseconds: 1000),

            new DelegateStep("识别金额并保存", () =>
            {
                recognizedAmount = ReadAmount(capture, imagePreprocess, ocr, amountRegion, amountPageProbeOptions);

                if (recognizedAmount.Success)
                {
                    Console.WriteLine($"金额：{recognizedAmount.Amount}，标准文本：{recognizedAmount.NormalizedText}");
                    return;
                }

                Console.WriteLine($"金额识别失败：{recognizedAmount.Message}");
                context.ExecutionController.WaitForContinue("金额识别失败。请确认现场后按 F3 在控制台手工输入金额，或按 F4 跳过本次。");

                var manualAmountInputStep = new ManualAmountInputStep(
                    "手工输入金额",
                    "请在控制台输入本次金额，然后按回车：",
                    result => recognizedAmount = result);
                manualAmountInputStep.Execute(context);

                Console.WriteLine($"金额：{recognizedAmount!.Amount}，来源：手工输入");
            }),
            //点击关闭按钮（如果存在）
            new ConditionalStep(
                "如果关闭按钮存在则点击",
                _ =>
                {
                    var exists = HasImage(capture, matcher, closeButtonRegion, closeButtonTemplatePath);
                    Console.WriteLine($"关闭按钮：{(exists ? "存在" : "不存在")}");
                    return exists;
                },
                new IAutomationStep[]
                {
                    new DelayStep(500),
                    new LeftClickStep(2607, 349),
                    new DelayStep(2000),
                }),
            new WaitStep("是否继续医保费用,F3继续 F4结束"),
            //继续第二步骤
            new MouseMoveStep(1943, 16),
            new LeftClickStep(1943, 16),
            new DelayStep(1000),
            new LeftClickStep(2234, -61),
            new DelegateStep("清空 input", () => ClearInput(keyboard)),
            new DelayStep(1000),
            new TextInputStep(number),
            new HotKeyStep(Keys.Enter),

            new OcrPopupStep(
                "处理未保存编辑继续弹窗",
                unsavedEditPopupTextRegion,
                unsavedEditPopupTextOcrOptions,
                new[] { "未保存", "继续", "下一个用户" },
                unsavedEditPopupButtonRegion,
                unsavedEditPopupButtonOcrOptions,
                regions => ChoosePopupButtonPoint(regions, "继续", "确定", "是"),
                delayBeforeCheckMilliseconds: 1000,
                delayAfterHandleMilliseconds: 1000,
                recheckCount: 5),

            new DelayStep(1000),

            //创建右键新的条目
            new RightClickStep(2406, 243),
            new DelayStep(6000),
            new MouseMoveStep(2406 + 67, 243 + 134),
            new LeftClickStep(2406 + 67, 243 + 134),
            new DelayStep(3000),
            new HotKeyStep(Keys.Enter),

            new LeftClickStep(729, 973),
            new WaitStep("请确认后手工录入本床护理费，或跳过本次。"),

            new LeftClickStep(2357, 0),
            new TextInputStep("jjzh"),
            new HotKeyStep(Keys.Enter),
            new DelayStep(2500),

            new LeftClickStep(2169, 46),
            new DelegateStep("输入金额", () =>
            {
                context.Keyboard.HotKey(Keys.Back);
                context.Keyboard.TextInput(recognizedAmount!.RawText);
            }),

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
                    mouse.MoveTo(result.Location.Value.X + 251, result.Location.Value.Y + 43);
                    mouse.LeftClick(result.Location.Value.X + 251, result.Location.Value.Y + 43);
                    mouse.MoveTo(result.Location.Value.X + 251, result.Location.Value.Y + 43);
                    mouse.LeftClick(result.Location.Value.X + 387, result.Location.Value.Y + 123);
                }
            }),
            new DelayStep(1500),

            new MouseMoveStep(1942, 112),
            new LeftClickStep(1942, 112),
            new DelayStep(1000),

            new EnsureConditionStep(
                "确保预结算已取消勾选",
                _ =>
                {
                    var checkedYjs = HasImage(capture, matcher, yjsRegion, yjsTemplatePath);
                    Console.WriteLine($"预结算：{(checkedYjs ? "已勾选" : "未勾选")}");
                    return !checkedYjs;
                },
                new IAutomationStep[]
                {
                    new MouseMoveStep(2403, 837),
                    new LeftClickStep(2403, 837),
                },
                maxRetryAttempts: 3,
                retryDelayMilliseconds: 800),

            //正式结算
            new SettlementNavigationStep(
                "点击结算后进入金额识别页",
                new IAutomationStep[]
                {
                    new MouseMoveStep(2515, 831),
                    new LeftClickStep(2515, 831),
                },
                ctx => IsAmountRecognitionPageReady(
                    ctx,
                    amountPageFeatureRegion,
                    amountPageFeatureTemplatePath,
                    amountRegion,
                    amountPageProbeOptions),
                settlementPopupSteps,
                maxIterations: 12,
                manualInterventionReason: "结算后未识别到已知弹窗，也尚未进入金额识别页。请手工处理当前弹窗或页面后按 F3 继续，或按 F4 跳过本次。",
                iterationDelayMilliseconds: 1000),
        };

        new AutomationRunner().Run(steps, context);
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

    private static string GetRunnerAssetPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, fileName);
    }

    private static bool IsAmountRecognitionPageReady(
        AutomationContext context,
        Rectangle featureRegion,
        string featureTemplatePath,
        Rectangle amountRegion,
        AmountOcrOptions ocrOptions)
    {
        if (File.Exists(featureTemplatePath))
        {
            var matched = HasImage(context.Capture, context.Matcher, featureRegion, featureTemplatePath);
            Console.WriteLine($"金额页模板判定：{(matched ? "命中" : "未命中")}，模板：{featureTemplatePath}");
            if (matched)
            {
                return true;
            }
        }
        else
        {
            Console.WriteLine($"金额页模板不存在，回退到金额 OCR 探测：{featureTemplatePath}");
        }

        var amountResult = ReadAmount(context.Capture, context.ImagePreprocess, context.Ocr, amountRegion, ocrOptions);
        var ready = amountResult.Success && amountResult.Amount is not null;
        Console.WriteLine(
            $"金额 OCR 探测：{(ready ? "已就绪" : "未就绪")}，原始文本：{amountResult.RawText}，标准文本：{amountResult.NormalizedText}");
        return ready;
    }

    private static GeneralOcrResult ReadText(
        ScreenCaptureService capture,
        IGeneralOcrService generalOcr,
        Rectangle region,
        GeneralOcrOptions options)
    {
        using var source = capture.Capture(region);
        return generalOcr.RecognizeTextAsync(source, options).GetAwaiter().GetResult();
    }

    private static Point? ChoosePopupButtonPoint(IReadOnlyList<GeneralOcrRegion> regions, params string[] buttonTexts)
    {
        if (regions.Count == 0)
        {
            return null;
        }

        var normalizedTargets = buttonTexts
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(GeneralTextNormalizer.NormalizeText)
            .ToArray();

        foreach (var target in normalizedTargets)
        {
            var candidate = regions
                .Where(region => region.NormalizedText.Contains(target, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(region => region.Center.Y)
                .ThenByDescending(region => region.Center.X)
                .FirstOrDefault();

            if (candidate is not null)
            {
                return new Point((int)Math.Round(candidate.Center.X), (int)Math.Round(candidate.Center.Y));
            }
        }

        return null;
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