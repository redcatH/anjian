using System;
using System.Drawing;
using System.Threading;

namespace Anjian;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Anjian.Runner 已启动。");
        Console.WriteLine("这里是控制台入口，你可以直接编写自动化流程。");

        var mouse = new Win32MouseService();
        var matcher = new TemplateMatcher();
        var ocr = new TesseractAmountOcrService();
        var keyboard = new Win32KeyboardService();
        mouse.MoveTo(2203, 386);
        Thread.Sleep(1000);
        mouse.LeftDoubleClick(2203, 386, 80);
        Thread.Sleep(1000);
        mouse.LeftClick(2292, 443);
        keyboard.TextInput("1233333");
        _ = mouse;
        _ = matcher;
        _ = ocr;

        Console.WriteLine("可用服务：Win32MouseService、TemplateMatcher、TesseractAmountOcrService。");
        Console.WriteLine("示例区域：{0}", new Rectangle(0, 0, 300, 120));
    }
}
