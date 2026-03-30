using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Anjian;

public sealed class Win32MouseService : IMouseService
{
    private const uint InputMouse = 0;

    private const uint MouseeventfMove = 0x0001;
    private const uint MouseeventfLeftDown = 0x0002;
    private const uint MouseeventfLeftUp = 0x0004;
    private const uint MouseeventfRightDown = 0x0008;
    private const uint MouseeventfRightUp = 0x0010;
    private const uint MouseeventfAbsolute = 0x8000;
    private const uint MouseeventfVirtualDesk = 0x4000;

    public void MoveTo(int x, int y)
    {
        SendMouseInput(x, y, MouseeventfMove | MouseeventfAbsolute | MouseeventfVirtualDesk);
    }

    public void LeftClick(int x, int y)
    {
        MoveTo(x, y);
        SendMouseInput(x, y, MouseeventfLeftDown | MouseeventfAbsolute | MouseeventfVirtualDesk);
        SendMouseInput(x, y, MouseeventfLeftUp | MouseeventfAbsolute | MouseeventfVirtualDesk);
    }

    public void RightClick(int x, int y)
    {
        MoveTo(x, y);
        SendMouseInput(x, y, MouseeventfRightDown | MouseeventfAbsolute | MouseeventfVirtualDesk);
        SendMouseInput(x, y, MouseeventfRightUp | MouseeventfAbsolute | MouseeventfVirtualDesk);
    }

    public void LeftDoubleClick(int x, int y, int intervalMilliseconds = 80)
    {
        LeftClick(x, y);
        Thread.Sleep(Math.Max(1, intervalMilliseconds));
        LeftClick(x, y);
    }

    public void LeftDown(int x, int y)
    {
        MoveTo(x, y);
        SendMouseInput(x, y, MouseeventfLeftDown | MouseeventfAbsolute | MouseeventfVirtualDesk);
    }

    public void LeftUp(int x, int y)
    {
        MoveTo(x, y);
        SendMouseInput(x, y, MouseeventfLeftUp | MouseeventfAbsolute | MouseeventfVirtualDesk);
    }

    private static void SendMouseInput(int x, int y, uint flags)
    {
        var input = new Input
        {
            Type = InputMouse,
            Data = new InputUnion
            {
                Mouse = new MouseInput
                {
                    Dx = NormalizeAbsoluteX(x),
                    Dy = NormalizeAbsoluteY(y),
                    MouseData = 0,
                    DwFlags = flags,
                    Time = 0,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        var sent = SendInput(1, new[] { input }, Marshal.SizeOf<Input>());
        if (sent != 1)
        {
            throw new InvalidOperationException($"鼠标输入发送失败，Win32Error={Marshal.GetLastWin32Error()}");
        }
    }

    private static int NormalizeAbsoluteX(int x)
    {
        var bounds = SystemInformation.VirtualScreen;
        return (int)Math.Round((x - bounds.Left) * 65535d / Math.Max(1, bounds.Width - 1));
    }

    private static int NormalizeAbsoluteY(int y)
    {
        var bounds = SystemInformation.VirtualScreen;
        return (int)Math.Round((y - bounds.Top) * 65535d / Math.Max(1, bounds.Height - 1));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
}
