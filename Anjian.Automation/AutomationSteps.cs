using System;
using System.Collections.Generic;
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

    public string Name => $"鼠标移动到 ({X}, {Y})";

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

        var runner = new AutomationRunner();
        runner.Run(branchSteps, context);
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

        throw new InvalidOperationException($"{Name}失败，目标条件仍未满足。");
    }
}

public sealed class SettlementDialogRule
{
    public SettlementDialogRule(
        string name,
        Func<AutomationContext, bool> isMatch,
        IReadOnlyList<IAutomationStep> steps,
        int postActionDelayMilliseconds = 0)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "未命名弹窗规则" : name;
        IsMatch = isMatch ?? throw new ArgumentNullException(nameof(isMatch));
        Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        PostActionDelayMilliseconds = Math.Max(0, postActionDelayMilliseconds);
    }

    public string Name { get; }

    public Func<AutomationContext, bool> IsMatch { get; }

    public IReadOnlyList<IAutomationStep> Steps { get; }

    public int PostActionDelayMilliseconds { get; }
}

public sealed class SettlementNavigationStep : IAutomationStep
{
    private readonly IReadOnlyList<IAutomationStep> _entrySteps;
    private readonly Func<AutomationContext, bool> _targetDetector;
    private readonly IReadOnlyList<SettlementDialogRule> _dialogRules;
    private readonly int _maxIterations;
    private readonly string _manualInterventionReason;
    private readonly int _iterationDelayMilliseconds;

    public SettlementNavigationStep(
        string name,
        IReadOnlyList<IAutomationStep> entrySteps,
        Func<AutomationContext, bool> targetDetector,
        IReadOnlyList<SettlementDialogRule> dialogRules,
        int maxIterations,
        string manualInterventionReason,
        int iterationDelayMilliseconds = 0)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "结算导航" : name;
        _entrySteps = entrySteps ?? throw new ArgumentNullException(nameof(entrySteps));
        _targetDetector = targetDetector ?? throw new ArgumentNullException(nameof(targetDetector));
        _dialogRules = dialogRules ?? throw new ArgumentNullException(nameof(dialogRules));
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

            var matchedRule = FindFirstMatchingRule(context);
            if (matchedRule is not null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 命中弹窗规则：{matchedRule.Name}");
                if (matchedRule.Steps.Count > 0)
                {
                    runner.Run(matchedRule.Steps, context);
                }

                DelayAfterAction(matchedRule.PostActionDelayMilliseconds);
                continue;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 未命中任何已知弹窗规则，进入人工介入。");
            context.ExecutionController.WaitForContinue($"{_manualInterventionReason} 当前轮次：{iteration}/{_maxIterations}");

            if (IsTargetReached(context))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 人工介入后已进入目标页面。");
                return;
            }

            DelayAfterAction(_iterationDelayMilliseconds);
        }

        throw new InvalidOperationException($"{Name}失败：超过最大轮次 {_maxIterations}，仍未进入目标页面。");
    }

    private SettlementDialogRule? FindFirstMatchingRule(AutomationContext context)
    {
        return _dialogRules.FirstOrDefault(rule => rule.IsMatch(context));
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
