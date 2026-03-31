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
        var amountInputVerifyRegion = new Rectangle(2767, 301, 148, 33);
        var amountPageFeatureRegion = new Rectangle(2720, 280, 240, 100);
        var amountPageFeatureTemplatePath = GetRunnerAssetPath("金额页特征.png");
        var amountPageProbeOptions = new AmountOcrOptions(
            true,
            true,
            160,
            false);
        var amountInputVerifyOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

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

        var numberActionVerifyOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var careTypeSelectedTextRegion = new Rectangle(640, 930, 300, 90);
        var careTypeSelectedTextOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var careTypeDropdownOptionRegion = new Rectangle(600, 780, 520, 320);
        var careTypeDropdownOptionOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var chargeItemSelectedTextRegion = new Rectangle(2140, -30, 520, 80);
        var chargeItemSelectedTextOcrOptions = new GeneralOcrOptions(
            true,
            true,
            180,
            true);

        var firstNumberActionVerifyRegion = new Rectangle(2207, 90, 100, 600);
        var secondNumberActionVerifyRegion = new Rectangle(2207, 90, 100, 600);

        var numberInputOptions = new KeyboardTextInputOptions(
            Mode: KeyboardTextInputMode.ClipboardPaste,
            RestoreClipboard: true);

        var settlementPopupSteps = new OcrPopupStep[]
        {
            // 先留空，后续把已知结算弹窗的 OCR 处理规则加在这里。
        };

        var steps = new IAutomationStep[]
        {
            new MouseMoveStep(1942, 112),
            new LeftClickStep(1942, 112),
            new DelayStep(1000),

            new VerifiedActionStep(
                "输入号码并确认页面切换成功",
                number,
                new IAutomationStep[]
                {
                    new LeftClickStep(2226, -56),
                    new DelayStep(500),
                    new DelegateStep("清空号码输入框", () => ClearInput(keyboard)),
                    new DelayStep(1500),
                    new TextInputStep(number, numberInputOptions),
                    new DelayStep(500),
                    new HotKeyStep(Keys.Enter),
                },
                new IVerificationCheck[]
                {
                    new OcrTextCheck(
                        "验证区域包含输入号码",
                        firstNumberActionVerifyRegion,
                        numberActionVerifyOcrOptions,
                        OcrTextMatchMode.Contains,
                        ctx => ctx.ActionValue),
                },
                maxRetryAttempts: 2,
                retryDelayMilliseconds: 1000,
                manualInterventionReason: "输入号码后校验失败。请确认号码是否已经正确加载，然后按 F3 继续，或按 F4 跳过。",
                postActionDelayMilliseconds: 2000),

            new WaitStep("请选择继续或跳过本次。"),
            new LeftClickStep(2222, -19),

            new ConditionalStep(
                "如果当前不是内科则输入 nk 并确认",
                _ =>
                {
                    var exists = HasImage(capture, matcher, departmentRegion, departmentTemplatePath);
                    Console.WriteLine($"内科标识存在：{exists}");
                    return !exists;
                },
                new IAutomationStep[]
                {
                    new TextInputStep("nk"),
                    new DelayStep(1000),
                    new HotKeyStep(Keys.Enter),
                    new DelayStep(1000),
                }),

            new EnsureConditionStep(
                "确保已勾选预结算",
                _ =>
                {
                    var checkedYjs = HasImage(capture, matcher, yjsRegion, yjsTemplatePath, useGrayscale: false);
                    Console.WriteLine($"预结算已勾选：{checkedYjs}");
                    return checkedYjs;
                },
                new IAutomationStep[]
                {
                    new MouseMoveStep(2403, 837),
                    new LeftClickStep(2403, 837),
                },
                maxRetryAttempts: 5,
                retryDelayMilliseconds: 800,
                manualInterventionReason: "请确认预结算已经勾选，然后按 F3 继续，或按 F4 跳过。"),

            new EnsureConditionStep(
                "确保已勾选社保 IC 卡",
                _ =>
                {
                    var exists = HasImage(capture, matcher, sbicRegion, sbicTemplatePath);
                    Console.WriteLine($"社保 IC 卡已勾选：{exists}");
                    return exists;
                },
                new IAutomationStep[]
                {
                    new MouseMoveStep(2652, 835),
                    new LeftClickStep(2652, 835),
                    new MouseMoveStep(2602, 788),
                    new LeftClickStep(2602, 788),
                },
                maxRetryAttempts: 5,
                retryDelayMilliseconds: 800,
                manualInterventionReason: "请确认社保 IC 卡已经勾选，然后按 F3 继续，或按 F4 跳过。"),

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
                manualInterventionReason: "点击结算后，既未命中已知弹窗，也尚未进入金额识别页。请手工处理后按 F3 继续，或按 F4 跳过。",
                iterationDelayMilliseconds: 1000),

            new DelegateStep("识别金额并保存", () =>
            {
                recognizedAmount = ReadAmount(capture, imagePreprocess, ocr, amountRegion, amountPageProbeOptions);

                if (recognizedAmount.Success)
                {
                    Console.WriteLine($"金额识别成功：{recognizedAmount.Amount}，标准文本：{recognizedAmount.NormalizedText}");
                    return;
                }

                Console.WriteLine($"金额 OCR 失败：{recognizedAmount.Message}");
                context.ExecutionController.WaitForContinue("金额 OCR 失败。请按 F3 在控制台手工输入金额，或按 F4 跳过。");

                var manualAmountInputStep = new ManualAmountInputStep(
                    "手工输入金额",
                    "请在控制台输入金额后按回车：",
                    result => recognizedAmount = result);
                manualAmountInputStep.Execute(context);

                Console.WriteLine($"手工录入金额：{recognizedAmount!.Amount}");
            }),

            new ConditionalStep(
                "如果关闭按钮存在则点击",
                _ =>
                {
                    var exists = HasImage(capture, matcher, closeButtonRegion, closeButtonTemplatePath);
                    Console.WriteLine($"关闭按钮存在：{exists}");
                    return exists;
                },
                new IAutomationStep[]
                {
                    new DelayStep(500),
                    new LeftClickStep(2607, 349),
                    new DelayStep(2000),
                }),

            new WaitStep("是否继续医保费用流程？F3 继续，F4 结束。"),

            new VerifiedActionStep(
                "再次输入号码并确认页面切换成功",
                number,
                new IAutomationStep[]
                {
                    new MouseMoveStep(1943, 16),
                    new LeftClickStep(1943, 16),
                    new DelayStep(1000),
                    new LeftClickStep(2234, -61),
                    new DelegateStep("再次清空号码输入框", () => ClearInput(keyboard)),
                    new DelayStep(1000),
                    new TextInputStep(number, numberInputOptions),
                    new HotKeyStep(Keys.Enter),
                    new OcrPopupStep(
                        "处理未保存编辑弹窗",
                        unsavedEditPopupTextRegion,
                        unsavedEditPopupTextOcrOptions,
                        new[] { "未保存", "继续", "下一个用户" },
                        unsavedEditPopupButtonRegion,
                        unsavedEditPopupButtonOcrOptions,
                        regions => ChooseTextCenterPoint(regions, "继续", "确定", "是"),
                        delayBeforeCheckMilliseconds: 1000,
                        delayAfterHandleMilliseconds: 1000,
                        recheckCount: 5),
                },
                new IVerificationCheck[]
                {
                    new OcrTextCheck(
                        "验证第二处区域包含输入号码",
                        secondNumberActionVerifyRegion,
                        numberActionVerifyOcrOptions,
                        OcrTextMatchMode.Contains,
                        ctx => ctx.ActionValue),
                },
                maxRetryAttempts: 2,
                retryDelayMilliseconds: 1000,
                manualInterventionReason: "再次输入号码后校验失败。请确认号码是否已经正确加载，然后按 F3 继续，或按 F4 跳过。",
                postActionDelayMilliseconds: 1000),

            new DelayStep(1000),

            new RightClickStep(2406, 243),
            new DelayStep(6000),
            new MouseMoveStep(2473, 377),
            new LeftClickStep(2473, 377),
            new DelayStep(3000),
            new HotKeyStep(Keys.Enter),

            new VerifiedActionStep(
                "选择居家护理费类型",
                "居家护理费",
                new IAutomationStep[]
                {
                    //TODO 这里具体如何点击哪里还需要识别一下
                    // new LeftClickStep(729, 973),
                    new DelayStep(800),
                    new DelegateStep(
                        "通过 OCR 点击居家护理费选项",
                        () => ClickTextInRegionByOcr(
                            capture,
                            generalOcr,
                            mouse,
                            careTypeDropdownOptionRegion,
                            careTypeDropdownOptionOcrOptions,
                            "居家护理费")),
                },
                new IVerificationCheck[]
                {
                    new OcrTextCheck(
                        "验证护理费类型文本",
                        careTypeSelectedTextRegion,
                        careTypeSelectedTextOcrOptions,
                        OcrTextMatchMode.Contains,
                        ctx => ctx.ActionValue),
                },
                maxRetryAttempts: 4,
                retryDelayMilliseconds: 1000,
                manualInterventionReason: "切换到居家护理费类型失败。请手工处理后按 F3 继续，或按 F4 跳过。",
                postActionDelayMilliseconds: 1200),

            new VerifiedActionStep(
                "输入收费项目并确认显示结果",
                "jjzh",
                new IAutomationStep[]
                {
                    new LeftClickStep(2357, 0),
                    new TextInputStep("jjzh", numberInputOptions),
                    new HotKeyStep(Keys.Enter),
                },
                new IVerificationCheck[]
                {
                    new OcrTextCheck(
                        "验证收费项目显示文本",
                        chargeItemSelectedTextRegion,
                        chargeItemSelectedTextOcrOptions,
                        OcrTextMatchMode.Contains,
                        ctx => ctx.ActionValue,
                        (ctx, observed, expected) => VerifyChargeItemSelection(ctx, observed, expected)),
                },
                maxRetryAttempts: 2,
                retryDelayMilliseconds: 1000,
                manualInterventionReason: "收费项目校验失败。请手工处理后按 F3 继续，或按 F4 跳过。",
                postActionDelayMilliseconds: 2500),

            new VerifiedActionStep(
                "输入金额并确认显示结果",
                recognizedAmount!.RawText,
                new IAutomationStep[]
                {
                    new LeftClickStep(2169, 46),
                    new DelegateStep("输入金额文本", () =>
                    {
                        context.Keyboard.HotKey(Keys.Back);
                        context.Keyboard.TextInput(recognizedAmount.RawText, numberInputOptions);
                    }),
                },
                new IVerificationCheck[]
                {
                    new OcrTextCheck(
                        "验证金额显示文本",
                        amountInputVerifyRegion,
                        amountInputVerifyOcrOptions,
                        OcrTextMatchMode.Contains,
                        ctx => ctx.ActionValue,
                        (ctx, observed, expected) => VerifyAmountDisplay(ctx, observed, expected)),
                },
                maxRetryAttempts: 2,
                retryDelayMilliseconds: 800,
                manualInterventionReason: "金额校验失败。请手工处理后按 F3 继续，或按 F4 跳过。",
                postActionDelayMilliseconds: 800),

            new DelegateStep("查找新增行", () =>
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
                    Console.WriteLine($"完成后预结算仍为勾选状态：{checkedYjs}");
                    return !checkedYjs;
                },
                new IAutomationStep[]
                {
                    new MouseMoveStep(2403, 837),
                    new LeftClickStep(2403, 837),
                },
                maxRetryAttempts: 3,
                retryDelayMilliseconds: 800),
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

    private static void ClickTextInRegionByOcr(
        ScreenCaptureService capture,
        IGeneralOcrService generalOcr,
        IMouseService mouse,
        Rectangle region,
        GeneralOcrOptions options,
        params string[] targetTexts)
    {
        using var source = capture.Capture(region);
        var result = generalOcr.RecognizeRegionsAsync(source, options).GetAwaiter().GetResult();
        if (!result.Success)
        {
            throw new InvalidOperationException($"OCR 读取下拉选项失败：{result.Message}");
        }

        var absoluteRegions = result.SafeRegions
            .Select(item => item with
            {
                Bounds = new Rectangle(item.Bounds.X + region.X, item.Bounds.Y + region.Y, item.Bounds.Width, item.Bounds.Height),
                Center = new GeneralOcrPoint(item.Center.X + region.X, item.Center.Y + region.Y)
            })
            .ToArray();

        var clickPoint = ChooseTextCenterPoint(absoluteRegions, targetTexts);
        if (clickPoint is null)
        {
            throw new InvalidOperationException($"OCR 未找到目标文本：{string.Join(", ", targetTexts)}");
        }

        mouse.MoveTo(clickPoint.Value.X, clickPoint.Value.Y);
        mouse.LeftClick(clickPoint.Value.X, clickPoint.Value.Y);
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
            Console.WriteLine($"金额页特征图匹配结果：{matched}");
            if (matched)
            {
                return true;
            }
        }
        else
        {
            Console.WriteLine($"金额页特征图不存在，回退到金额 OCR：{featureTemplatePath}");
        }

        var amountResult = ReadAmount(context.Capture, context.ImagePreprocess, context.Ocr, amountRegion, ocrOptions);
        var ready = amountResult.Success && amountResult.Amount is not null;
        Console.WriteLine($"金额 OCR 就绪：{ready}，原始文本：{amountResult.RawText}，标准文本：{amountResult.NormalizedText}");
        return ready;
    }

    private static Point? ChooseTextCenterPoint(IReadOnlyList<GeneralOcrRegion> regions, params string[] targetTexts)
    {
        if (regions.Count == 0)
        {
            return null;
        }

        var normalizedTargets = targetTexts
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

    private static VerificationResult VerifyChargeItemSelection(
        VerifiedActionContext context,
        string observedValue,
        string? expectedValue)
    {
        var expectedAliases = GetChargeItemExpectedAliases(context.ActionValue);
        var matchedAlias = expectedAliases.FirstOrDefault(alias =>
            observedValue.Contains(alias, StringComparison.OrdinalIgnoreCase));

        if (matchedAlias is not null)
        {
            return new VerificationResult(
                true,
                $"收费项目校验通过。动作值={context.ActionValue}，命中文本={matchedAlias}，识别结果={observedValue}",
                observedValue);
        }

        return new VerificationResult(
            false,
            $"收费项目校验失败。动作值={context.ActionValue}，期望别名={string.Join(" / ", expectedAliases)}，默认期望值={expectedValue}，识别结果={observedValue}",
            observedValue);
    }

    private static string[] GetChargeItemExpectedAliases(string actionValue)
    {
        var normalizedActionValue = GeneralTextNormalizer.NormalizeText(actionValue);
        return normalizedActionValue switch
        {
            "jjzh" => new[]
            {
                normalizedActionValue,
                GeneralTextNormalizer.NormalizeText("居家护理费"),
                GeneralTextNormalizer.NormalizeText("居家照护"),
                GeneralTextNormalizer.NormalizeText("居家照护费"),
            },
            _ => new[]
            {
                normalizedActionValue
            }
        };
    }

    private static VerificationResult VerifyAmountDisplay(
        VerifiedActionContext context,
        string observedValue,
        string? expectedValue)
    {
        if (TryParseDecimalFromText(observedValue, out var observedAmount) &&
            TryParseDecimalFromText(expectedValue, out var expectedAmount))
        {
            if (observedAmount == expectedAmount)
            {
                return new VerificationResult(
                    true,
                    $"金额校验通过。期望金额={expectedAmount}，识别金额={observedAmount}",
                    observedValue);
            }

            return new VerificationResult(
                false,
                $"金额校验失败。期望金额={expectedAmount}，识别金额={observedAmount}",
                observedValue);
        }

        return new VerificationResult(
            false,
            $"金额校验失败。动作值={context.ActionValue}，默认期望值={expectedValue}，识别结果={observedValue}",
            observedValue);
    }

    private static bool TryParseDecimalFromText(string? text, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = new string(text.Where(ch => char.IsDigit(ch) || ch == '.' || ch == '-').ToArray());
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        return decimal.TryParse(normalized, out value);
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
            Console.WriteLine($"命中屏幕坐标：{screenPoint.X}, {screenPoint.Y}");
        }

        return result.Success;
    }
}
