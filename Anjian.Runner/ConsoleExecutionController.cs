using System;

namespace Anjian;

internal sealed class ConsoleExecutionController : IExecutionController
{
    private readonly IGlobalContinueSignal _continueSignal;

    public ConsoleExecutionController(ExecutionMode mode, IGlobalContinueSignal continueSignal)
    {
        Mode = mode;
        _continueSignal = continueSignal ?? throw new ArgumentNullException(nameof(continueSignal));
    }

    public ExecutionMode Mode { get; }

    public void BeforeStep(string stepName, int index)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 步骤 {index + 1}：{stepName}，模式={GetModeText(Mode)}");
    }

    public void WaitForContinue(string reason)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已暂停：{reason}");
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 请按全局 F3 继续。");
        _continueSignal.WaitForF3(reason);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已收到 F3，继续执行。");
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
