using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Anjian;

public sealed class MouseMoveStep : IAutomationStep
{
    public MouseMoveStep(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    public string Name => $"移动鼠标到 ({X}, {Y})";

    public void Execute(AutomationContext context)
    {
        context.Mouse.MoveTo(X, Y);
    }
}

public sealed class LeftClickStep : IAutomationStep
{
    public LeftClickStep(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    public string Name => $"左键单击 ({X}, {Y})";

    public void Execute(AutomationContext context)
    {
        context.Mouse.MoveTo(X, Y);
        context.Mouse.LeftClick(X, Y);
    }
}

public sealed class RightClickStep : IAutomationStep
{
    public RightClickStep(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    public string Name => $"右键单击 ({X}, {Y})";

    public void Execute(AutomationContext context)
    {
        context.Mouse.RightClick(X, Y);
    }
}

public sealed class LeftDoubleClickStep : IAutomationStep
{
    public LeftDoubleClickStep(int x, int y, int intervalMilliseconds = 80)
    {
        X = x;
        Y = y;
        IntervalMilliseconds = intervalMilliseconds;
    }

    public int X { get; }

    public int Y { get; }

    public int IntervalMilliseconds { get; }

    public string Name => $"左键双击 ({X}, {Y})";

    public void Execute(AutomationContext context)
    {
        context.Mouse.LeftDoubleClick(X, Y, IntervalMilliseconds);
    }
}

public sealed class TextInputStep : IAutomationStep
{
    public TextInputStep(string text, KeyboardTextInputOptions? options = null)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Options = options;
    }

    public string Text { get; }

    public KeyboardTextInputOptions? Options { get; }

    public string Name => $"输入文本：{Text}";

    public void Execute(AutomationContext context)
    {
        if (Options is null)
        {
            context.Keyboard.TextInput(Text);
            return;
        }

        context.Keyboard.TextInput(Text, Options);
    }
}

public sealed class HotKeyStep : IAutomationStep
{
    public HotKeyStep(params Keys[] keys)
    {
        Keys = keys ?? throw new ArgumentNullException(nameof(keys));
    }

    public Keys[] Keys { get; }

    public string Name => $"发送组合键：{string.Join(" + ", Keys)}";

    public void Execute(AutomationContext context)
    {
        context.Keyboard.HotKey(Keys);
    }
}

public sealed class DelayStep : IAutomationStep
{
    public DelayStep(int milliseconds)
    {
        Milliseconds = milliseconds;
    }

    public int Milliseconds { get; }

    public string Name => $"等待 {Milliseconds} ms";

    public void Execute(AutomationContext context)
    {
        _ = context;
        Thread.Sleep(Math.Max(0, Milliseconds));
    }
}

public sealed class WaitStep : IAutomationStep
{
    public WaitStep(string reason)
    {
        Reason = string.IsNullOrWhiteSpace(reason) ? "等待用户按 F3 继续" : reason;
    }

    public string Reason { get; }

    public string Name => $"Wait：{Reason}";

    public void Execute(AutomationContext context)
    {
        _ = context;
    }
}

public sealed class DelegateStep : IAutomationStep
{
    private readonly Action _action;

    public DelegateStep(string name, Action action)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "委托步骤" : name;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        _ = context;
        _action();
    }
}

public sealed class ConditionalStep : IAutomationStep
{
    private readonly Func<AutomationContext, bool> _predicate;
    private readonly IReadOnlyList<IAutomationStep> _whenTrueSteps;
    private readonly IReadOnlyList<IAutomationStep> _whenFalseSteps;

    public ConditionalStep(
        string name,
        Func<AutomationContext, bool> predicate,
        IReadOnlyList<IAutomationStep> whenTrueSteps,
        IReadOnlyList<IAutomationStep>? whenFalseSteps = null)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "条件步骤" : name;
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _whenTrueSteps = whenTrueSteps ?? throw new ArgumentNullException(nameof(whenTrueSteps));
        _whenFalseSteps = whenFalseSteps ?? Array.Empty<IAutomationStep>();
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var branchSteps = _predicate(context) ? _whenTrueSteps : _whenFalseSteps;
        if (branchSteps.Count == 0)
        {
            return;
        }

        new AutomationRunner().Run(branchSteps, context);
    }
}

public sealed class EnsureConditionStep : IAutomationStep
{
    private readonly Func<AutomationContext, bool> _predicate;
    private readonly IReadOnlyList<IAutomationStep> _retrySteps;
    private readonly int _maxRetryAttempts;
    private readonly int _retryDelayMilliseconds;
    private readonly string? _manualInterventionReason;

    public EnsureConditionStep(
        string name,
        Func<AutomationContext, bool> predicate,
        IReadOnlyList<IAutomationStep> retrySteps,
        int maxRetryAttempts,
        int retryDelayMilliseconds = 0,
        string? manualInterventionReason = null)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "确保条件满足" : name;
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _retrySteps = retrySteps ?? throw new ArgumentNullException(nameof(retrySteps));
        _maxRetryAttempts = Math.Max(0, maxRetryAttempts);
        _retryDelayMilliseconds = Math.Max(0, retryDelayMilliseconds);
        _manualInterventionReason = string.IsNullOrWhiteSpace(manualInterventionReason)
            ? null
            : manualInterventionReason;
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_predicate(context))
        {
            return;
        }

        var runner = new AutomationRunner();
        for (var attempt = 1; attempt <= _maxRetryAttempts; attempt++)
        {
            if (_retrySteps.Count > 0)
            {
                runner.Run(_retrySteps, context);
            }

            if (_retryDelayMilliseconds > 0)
            {
                Thread.Sleep(_retryDelayMilliseconds);
            }

            if (_predicate(context))
            {
                return;
            }
        }

        if (_manualInterventionReason is not null)
        {
            context.ExecutionController.WaitForContinue(_manualInterventionReason);
            if (_predicate(context))
            {
                return;
            }
        }

        throw new InvalidOperationException($"{Name} 失败，目标条件仍未满足。");
    }
}

public sealed class VerifiedActionContext
{
    public VerifiedActionContext(string actionValue, int attempt, AutomationContext automationContext)
    {
        ActionValue = actionValue ?? throw new ArgumentNullException(nameof(actionValue));
        Attempt = attempt;
        AutomationContext = automationContext ?? throw new ArgumentNullException(nameof(automationContext));
    }

    public string ActionValue { get; }

    public int Attempt { get; }

    public AutomationContext AutomationContext { get; }
}

public sealed record VerificationResult(
    bool Success,
    string Message,
    string? ObservedValue = null);

public interface IVerificationCheck
{
    string Name { get; }

    VerificationResult Verify(VerifiedActionContext context);
}

public enum OcrTextMatchMode
{
    Equals = 0,
    Contains = 1,
    NotEmpty = 2,
}

public sealed class OcrTextCheck : IVerificationCheck
{
    private readonly Rectangle _region;
    private readonly GeneralOcrOptions _options;
    private readonly OcrTextMatchMode _matchMode;
    private readonly Func<VerifiedActionContext, string?>? _expectedValueFactory;
    private readonly Func<VerifiedActionContext, string, string?, VerificationResult>? _customVerifier;

    public OcrTextCheck(
        string name,
        Rectangle region,
        GeneralOcrOptions options,
        OcrTextMatchMode matchMode,
        string? expectedValue = null)
        : this(name, region, options, matchMode, _ => expectedValue, null)
    {
    }

    public OcrTextCheck(
        string name,
        Rectangle region,
        GeneralOcrOptions options,
        OcrTextMatchMode matchMode,
        Func<VerifiedActionContext, string?>? expectedValueFactory)
        : this(name, region, options, matchMode, expectedValueFactory, null)
    {
    }

    public OcrTextCheck(
        string name,
        Rectangle region,
        GeneralOcrOptions options,
        OcrTextMatchMode matchMode,
        Func<VerifiedActionContext, string?>? expectedValueFactory,
        Func<VerifiedActionContext, string, string?, VerificationResult>? customVerifier)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "OCR 文本校验" : name;
        _region = region;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _matchMode = matchMode;
        _expectedValueFactory = expectedValueFactory;
        _customVerifier = customVerifier;
    }

    public string Name { get; }

    public VerificationResult Verify(VerifiedActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        using var source = context.AutomationContext.Capture.Capture(_region);
        var result = context.AutomationContext.GeneralOcr
            .RecognizeTextAsync(source, _options)
            .GetAwaiter()
            .GetResult();

        var observedValue = result.NormalizedText;
        if (!result.Success)
        {
            return new VerificationResult(false, $"{Name} 失败：{result.Message}", observedValue);
        }

        var expectedValue = _expectedValueFactory?.Invoke(context);
        var normalizedExpected = expectedValue is null
            ? null
            : GeneralTextNormalizer.NormalizeText(expectedValue);

        if (_customVerifier is not null)
        {
            return _customVerifier(context, observedValue, normalizedExpected);
        }

        var success = _matchMode switch
        {
            OcrTextMatchMode.Equals => !string.IsNullOrWhiteSpace(normalizedExpected) &&
                                       string.Equals(observedValue, normalizedExpected, StringComparison.OrdinalIgnoreCase),
            OcrTextMatchMode.Contains => !string.IsNullOrWhiteSpace(normalizedExpected) &&
                                         observedValue.Contains(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            OcrTextMatchMode.NotEmpty => !string.IsNullOrWhiteSpace(observedValue),
            _ => false,
        };

        var message = _matchMode switch
        {
            OcrTextMatchMode.Equals => $"{Name} 期望等于：{normalizedExpected}，实际为：{observedValue}",
            OcrTextMatchMode.Contains => $"{Name} 期望包含：{normalizedExpected}，实际为：{observedValue}",
            OcrTextMatchMode.NotEmpty => $"{Name} 期望非空，实际为：{observedValue}",
            _ => $"{Name} 未知校验类型",
        };

        return new VerificationResult(success, message, observedValue);
    }
}

public sealed class ImagePresenceCheck : IVerificationCheck
{
    private readonly Rectangle _captureRegion;
    private readonly string _templatePath;
    private readonly bool _shouldExist;
    private readonly double _threshold;
    private readonly bool _useGrayscale;
    private readonly int _step;
    private readonly ImageMatchMode _matchMode;
    private readonly ImageScanDirection _scanDirection;

    public ImagePresenceCheck(
        string name,
        Rectangle captureRegion,
        string templatePath,
        bool shouldExist = true,
        double threshold = 0.90,
        bool useGrayscale = true,
        int step = 1,
        ImageMatchMode matchMode = ImageMatchMode.First,
        ImageScanDirection scanDirection = ImageScanDirection.TopToBottom)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "图片存在校验" : name;
        _captureRegion = captureRegion;
        _templatePath = string.IsNullOrWhiteSpace(templatePath)
            ? throw new ArgumentException("Template path is required.", nameof(templatePath))
            : templatePath;
        _shouldExist = shouldExist;
        _threshold = threshold;
        _useGrayscale = useGrayscale;
        _step = step;
        _matchMode = matchMode;
        _scanDirection = scanDirection;
    }

    public string Name { get; }

    public VerificationResult Verify(VerifiedActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!System.IO.File.Exists(_templatePath))
        {
            return new VerificationResult(false, $"{Name} 失败：模板图片不存在：{_templatePath}");
        }

        using var sourceBitmap = context.AutomationContext.Capture.Capture(_captureRegion);
        using var templateBitmap = new Bitmap(_templatePath);

        var options = new ImageMatchOptions(
            new Rectangle(0, 0, _captureRegion.Width, _captureRegion.Height),
            _threshold,
            _useGrayscale,
            _step,
            _matchMode,
            _scanDirection);

        var result = context.AutomationContext.Matcher.Find(sourceBitmap, templateBitmap, options);
        var exists = result.Success;
        var success = _shouldExist ? exists : !exists;
        var expectation = _shouldExist ? "存在" : "不存在";
        var observed = exists ? "存在" : "不存在";
        return new VerificationResult(success, $"{Name} 期望{expectation}，实际为{observed}", observed);
    }
}

public sealed class CustomCheck : IVerificationCheck
{
    private readonly Func<VerifiedActionContext, VerificationResult> _check;

    public CustomCheck(string name, Func<AutomationContext, VerificationResult> check)
        : this(
            name,
            check is null
                ? throw new ArgumentNullException(nameof(check))
                : context => check(context.AutomationContext))
    {
    }

    public CustomCheck(string name, Func<VerifiedActionContext, VerificationResult> check)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "自定义校验" : name;
        _check = check ?? throw new ArgumentNullException(nameof(check));
    }

    public string Name { get; }

    public VerificationResult Verify(VerifiedActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return _check(context);
    }
}

public sealed class VerifiedActionStep : IAutomationStep
{
    private readonly string _actionValue;
    private readonly IReadOnlyList<IAutomationStep> _entrySteps;
    private readonly IReadOnlyList<IVerificationCheck> _checks;
    private readonly int _maxRetryAttempts;
    private readonly int _retryDelayMilliseconds;
    private readonly string _manualInterventionReason;
    private readonly int _postActionDelayMilliseconds;

    public VerifiedActionStep(
        string name,
        string actionValue,
        IReadOnlyList<IAutomationStep> entrySteps,
        IReadOnlyList<IVerificationCheck> checks,
        int maxRetryAttempts,
        int retryDelayMilliseconds,
        string manualInterventionReason,
        int postActionDelayMilliseconds = 0)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "执行动作并验证" : name;
        _actionValue = actionValue ?? throw new ArgumentNullException(nameof(actionValue));
        _entrySteps = entrySteps ?? throw new ArgumentNullException(nameof(entrySteps));
        _checks = checks ?? throw new ArgumentNullException(nameof(checks));
        _maxRetryAttempts = Math.Max(0, maxRetryAttempts);
        _retryDelayMilliseconds = Math.Max(0, retryDelayMilliseconds);
        _manualInterventionReason = string.IsNullOrWhiteSpace(manualInterventionReason)
            ? "动作后校验失败，请手工处理后按 F3 继续，或按 F4 跳过当前运行。"
            : manualInterventionReason;
        _postActionDelayMilliseconds = Math.Max(0, postActionDelayMilliseconds);
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var runner = new AutomationRunner();
        var totalAttempts = _maxRetryAttempts + 1;

        for (var attempt = 1; attempt <= totalAttempts; attempt++)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 开始执行 {Name}，尝试 {attempt}/{totalAttempts}");

            if (_entrySteps.Count > 0)
            {
                runner.Run(_entrySteps, context);
            }

            DelayIfNeeded(_postActionDelayMilliseconds);

            var actionContext = new VerifiedActionContext(_actionValue, attempt, context);
            var failedCheck = FindFirstFailedCheck(actionContext, out var verificationResult);
            if (failedCheck is null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {Name} 校验通过。");
                return;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {failedCheck.Name} 校验失败：{verificationResult!.Message}");
            if (_retryDelayMilliseconds > 0 && attempt < totalAttempts)
            {
                DelayIfNeeded(_retryDelayMilliseconds);
            }
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {Name} 超过自动重试次数，进入人工介入。");
        context.ExecutionController.WaitForContinue(_manualInterventionReason);

        var manualContext = new VerifiedActionContext(_actionValue, totalAttempts + 1, context);
        var failedAfterManual = FindFirstFailedCheck(manualContext, out var manualResult);
        if (failedAfterManual is null)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 人工介入后 {Name} 校验通过。");
            return;
        }

        throw new InvalidOperationException(
            $"{Name} 失败：人工介入后仍未通过校验。失败校验项：{failedAfterManual.Name}，原因：{manualResult!.Message}");
    }

    private IVerificationCheck? FindFirstFailedCheck(
        VerifiedActionContext actionContext,
        out VerificationResult? verificationResult)
    {
        foreach (var check in _checks)
        {
            verificationResult = check.Verify(actionContext);
            Console.WriteLine(
                $"[{DateTime.Now:HH:mm:ss}] 校验项：{check.Name} -> {(verificationResult.Success ? "通过" : "失败")}，{verificationResult.Message}");

            if (!verificationResult.Success)
            {
                return check;
            }
        }

        verificationResult = null;
        return null;
    }

    private static void DelayIfNeeded(int milliseconds)
    {
        if (milliseconds > 0)
        {
            Thread.Sleep(milliseconds);
        }
    }
}

public sealed class SettlementNavigationStep : IAutomationStep
{
    private readonly IReadOnlyList<IAutomationStep> _entrySteps;
    private readonly Func<AutomationContext, bool> _targetDetector;
    private readonly IReadOnlyList<OcrPopupStep> _popupSteps;
    private readonly int _maxIterations;
    private readonly string _manualInterventionReason;
    private readonly int _iterationDelayMilliseconds;

    public SettlementNavigationStep(
        string name,
        IReadOnlyList<IAutomationStep> entrySteps,
        Func<AutomationContext, bool> targetDetector,
        IReadOnlyList<OcrPopupStep> popupSteps,
        int maxIterations,
        string manualInterventionReason,
        int iterationDelayMilliseconds = 1500)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "结算导航" : name;
        _entrySteps = entrySteps ?? throw new ArgumentNullException(nameof(entrySteps));
        _targetDetector = targetDetector ?? throw new ArgumentNullException(nameof(targetDetector));
        _popupSteps = popupSteps ?? throw new ArgumentNullException(nameof(popupSteps));
        _maxIterations = Math.Max(1, maxIterations);
        _manualInterventionReason = string.IsNullOrWhiteSpace(manualInterventionReason)
            ? "未识别到已知弹窗，请手工处理后按 F3 继续，或按 F4 跳过当前号码。"
            : manualInterventionReason;
        _iterationDelayMilliseconds = Math.Max(0, iterationDelayMilliseconds);
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var runner = new AutomationRunner();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 进入结算导航：{Name}");

        if (_entrySteps.Count > 0)
        {
            runner.Run(_entrySteps, context);
        }

        DelayAfterAction(_iterationDelayMilliseconds);

        for (var iteration = 1; iteration <= _maxIterations; iteration++)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 结算导航轮次 {iteration}/{_maxIterations}");

            if (IsTargetReached(context))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已进入目标页面。");
                return;
            }

            var popupOutcome = TryHandleFirstPopup(context);
            if (popupOutcome == OcrPopupHandleResult.Handled)
            {
                DelayAfterAction(_iterationDelayMilliseconds);
                continue;
            }

            if (popupOutcome == OcrPopupHandleResult.MatchedButFailed)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已识别到弹窗但无法安全处理，进入人工介入。");
                context.ExecutionController.WaitForContinue(_manualInterventionReason);
                if (IsTargetReached(context))
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 人工介入后已进入目标页面。");
                    return;
                }

                throw new InvalidOperationException($"{Name} 失败：弹窗已命中但自动处理失败，人工介入后仍未进入目标页面。");
            }

            DelayAfterAction(_iterationDelayMilliseconds);
            if (IsTargetReached(context))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已进入目标页面。");
                return;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 未命中任何已知弹窗，继续等待界面变化。");
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 超过最大轮次，进入人工介入。");
        context.ExecutionController.WaitForContinue(_manualInterventionReason);

        if (IsTargetReached(context))
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 人工介入后已进入目标页面。");
            return;
        }

        throw new InvalidOperationException($"{Name} 失败：超过最大轮次 {_maxIterations}，且人工介入后仍未进入目标页面。");
    }

    private OcrPopupHandleResult TryHandleFirstPopup(AutomationContext context)
    {
        foreach (var popupStep in _popupSteps)
        {
            var result = popupStep.TryHandle(context);
            if (result != OcrPopupHandleResult.NotMatched)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 弹窗步骤结果：{popupStep.Name} -> {result}");
                return result;
            }
        }

        return OcrPopupHandleResult.NotMatched;
    }

    private bool IsTargetReached(AutomationContext context)
    {
        var reached = _targetDetector(context);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 目标页面判定：{(reached ? "已命中" : "未命中")}");
        return reached;
    }

    private static void DelayAfterAction(int delayMilliseconds)
    {
        if (delayMilliseconds > 0)
        {
            Thread.Sleep(delayMilliseconds);
        }
    }
}

public sealed class ManualAmountInputStep : IAutomationStep
{
    private readonly Action<AmountOcrResult> _onAmountCaptured;
    private readonly string _prompt;

    public ManualAmountInputStep(string name, string prompt, Action<AmountOcrResult> onAmountCaptured)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "手工输入金额" : name;
        _prompt = string.IsNullOrWhiteSpace(prompt) ? "请在控制台输入金额：" : prompt;
        _onAmountCaptured = onAmountCaptured ?? throw new ArgumentNullException(nameof(onAmountCaptured));
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        while (true)
        {
            Console.WriteLine(_prompt);
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("金额不能为空，请重新输入。");
                continue;
            }

            var normalized = input.Replace(",", string.Empty);
            if (!decimal.TryParse(normalized, out var amount))
            {
                Console.WriteLine($"无效金额：{input}，请重新输入。");
                continue;
            }

            _onAmountCaptured(new AmountOcrResult(
                true,
                input,
                normalized,
                amount,
                0L,
                "用户手工输入金额成功。"));
            return;
        }
    }
}

public sealed class OcrPopupStep : IAutomationStep
{
    private readonly Rectangle _contentRegion;
    private readonly GeneralOcrOptions _contentOcrOptions;
    private readonly string[] _contentKeywords;
    private readonly Rectangle _buttonRegion;
    private readonly GeneralOcrOptions _buttonOcrOptions;
    private readonly Func<IReadOnlyList<GeneralOcrRegion>, Point?> _selectClickPoint;
    private readonly int _delayBeforeCheckMilliseconds;
    private readonly int _delayAfterHandleMilliseconds;
    private readonly int _recheckCount;
    private readonly string? _manualInterventionReason;

    public OcrPopupStep(
        string name,
        Rectangle contentRegion,
        GeneralOcrOptions contentOcrOptions,
        IReadOnlyList<string> contentKeywords,
        Rectangle buttonRegion,
        GeneralOcrOptions buttonOcrOptions,
        Func<IReadOnlyList<GeneralOcrRegion>, Point?> selectClickPoint,
        int delayBeforeCheckMilliseconds = 0,
        int delayAfterHandleMilliseconds = 0,
        int recheckCount = 2,
        string? manualInterventionReason = null)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "OCR 弹窗处理" : name;
        _contentRegion = contentRegion;
        _contentOcrOptions = contentOcrOptions ?? throw new ArgumentNullException(nameof(contentOcrOptions));
        _contentKeywords = contentKeywords?.Where(static x => !string.IsNullOrWhiteSpace(x)).ToArray()
            ?? throw new ArgumentNullException(nameof(contentKeywords));
        _buttonRegion = buttonRegion;
        _buttonOcrOptions = buttonOcrOptions ?? throw new ArgumentNullException(nameof(buttonOcrOptions));
        _selectClickPoint = selectClickPoint ?? throw new ArgumentNullException(nameof(selectClickPoint));
        _delayBeforeCheckMilliseconds = Math.Max(0, delayBeforeCheckMilliseconds);
        _delayAfterHandleMilliseconds = Math.Max(0, delayAfterHandleMilliseconds);
        _recheckCount = Math.Max(1, recheckCount);
        _manualInterventionReason = string.IsNullOrWhiteSpace(manualInterventionReason)
            ? null
            : manualInterventionReason;
    }

    public string Name { get; }

    public void Execute(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = TryHandle(context);
        if (result == OcrPopupHandleResult.Handled || result == OcrPopupHandleResult.NotMatched)
        {
            return;
        }

        if (_manualInterventionReason is not null)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] OCR 弹窗命中但未能处理：{Name}，进入人工介入。");
            context.ExecutionController.WaitForContinue(_manualInterventionReason);
            return;
        }

        throw new InvalidOperationException($"{Name} 失败：已识别到目标弹窗，但未能选出安全点击坐标。");
    }

    public OcrPopupHandleResult TryHandle(AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_delayBeforeCheckMilliseconds > 0)
        {
            Thread.Sleep(_delayBeforeCheckMilliseconds);
        }

        for (var attempt = 1; attempt <= _recheckCount; attempt++)
        {
            var contentResult = ReadOcrResult(context, _contentRegion, _contentOcrOptions);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 弹窗正文 OCR：{contentResult.NormalizedText}，消息：{contentResult.Message}");

            if (!contentResult.Success || !ContainsAllKeywords(contentResult, _contentKeywords))
            {
                if (attempt < _recheckCount)
                {
                    Thread.Sleep(_delayBeforeCheckMilliseconds > 0 ? _delayBeforeCheckMilliseconds : 300);
                }

                continue;
            }

            var buttonResult = ReadOcrResult(context, _buttonRegion, _buttonOcrOptions);
            var absoluteRegions = buttonResult.SafeRegions
                .Select(region => OffsetRegion(region, _buttonRegion.X, _buttonRegion.Y))
                .ToArray();

            var clickPoint = _selectClickPoint(absoluteRegions);
            if (clickPoint is null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 弹窗正文命中，但 handler 未返回点击坐标。");
                return OcrPopupHandleResult.MatchedButFailed;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] OCR 弹窗处理点击坐标：({clickPoint.Value.X}, {clickPoint.Value.Y})");
            context.Mouse.MoveTo(clickPoint.Value.X, clickPoint.Value.Y);
            context.Mouse.LeftClick(clickPoint.Value.X, clickPoint.Value.Y);

            if (_delayAfterHandleMilliseconds > 0)
            {
                Thread.Sleep(_delayAfterHandleMilliseconds);
            }

            return OcrPopupHandleResult.Handled;
        }

        return OcrPopupHandleResult.NotMatched;
    }

    private static GeneralOcrResult ReadOcrResult(
        AutomationContext context,
        Rectangle region,
        GeneralOcrOptions options)
    {
        using var source = context.Capture.Capture(region);
        return context.GeneralOcr.RecognizeRegionsAsync(source, options).GetAwaiter().GetResult();
    }

    private static bool ContainsAllKeywords(GeneralOcrResult result, IReadOnlyList<string> keywords)
    {
        if (keywords.Count == 0)
        {
            return true;
        }

        var normalizedKeywords = keywords
            .Select(GeneralTextNormalizer.NormalizeText)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        return normalizedKeywords.Length > 0 &&
               normalizedKeywords.All(keyword =>
                   result.NormalizedText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static GeneralOcrRegion OffsetRegion(GeneralOcrRegion region, int offsetX, int offsetY)
    {
        return region with
        {
            Bounds = new Rectangle(region.Bounds.X + offsetX, region.Bounds.Y + offsetY, region.Bounds.Width, region.Bounds.Height),
            Center = new GeneralOcrPoint(region.Center.X + offsetX, region.Center.Y + offsetY)
        };
    }
}

public enum OcrPopupHandleResult
{
    NotMatched = 0,
    Handled = 1,
    MatchedButFailed = 2,
}
