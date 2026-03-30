using System;
using System.Drawing;

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

        _ = mouse;
        _ = matcher;
        _ = ocr;

        Console.WriteLine("可用服务：Win32MouseService、TemplateMatcher、TesseractAmountOcrService。");
        Console.WriteLine("示例区域：{0}", new Rectangle(0, 0, 300, 120));
    }
}
