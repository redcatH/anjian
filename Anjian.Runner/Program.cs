using System;
using System.Drawing;
using System.Text;

namespace Anjian;

internal static class Program
{
    private static void Main()
    {
        const ExecutionMode mode = ExecutionMode.RunUntilWait;

        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Anjian.Runner 已启动。");
        Console.WriteLine($"当前执行模式：{GetModeText(mode)}");
        Console.WriteLine("调试控制：全局按 F3 继续，按 F4 跳过当前号码。");

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

        var numbers = new[]
        {
            "M0003",
            "M0004",
            "M0005",
        };

        var workflow = new SettlementWorkflow();

        foreach (var number in numbers)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 开始处理病例编号：{number}");
                workflow.Run(number, context);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 病例编号 {number} 执行完成。");
            }
            catch (SkipCurrentRunException ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 病例编号 {number} 已跳过：{ex.Message}");
            }
            catch (AutomationStepExecutionException ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 病例编号 {number} 执行失败：步骤 {ex.StepIndex + 1}，名称 {ex.StepName}");
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                context.ExecutionController.WaitForContinue($"病例编号 {number} 执行失败，请检查现场后按 F3 继续下一个号码。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 病例编号 {number} 执行失败：{ex.Message}");
                context.ExecutionController.WaitForContinue($"病例编号 {number} 执行失败，请检查现场后按 F3 继续下一个号码。");
            }
        }

        Console.WriteLine("可用服务：Win32MouseService、TemplateMatcher、TesseractAmountOcrService、Win32KeyboardService");
        Console.WriteLine("示例区域：{0}", new Rectangle(0, 0, 300, 120));
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
