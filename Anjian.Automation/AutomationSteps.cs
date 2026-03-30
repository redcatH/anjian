using System;
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
