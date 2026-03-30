using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Anjian;

public sealed class Win32KeyboardService : IKeyboardService
{
    private const uint InputKeyboard = 1;
    private const uint KeyeventfKeyUp = 0x0002;
    private const uint KeyeventfUnicode = 0x0004;

    public void TextInput(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        foreach (var ch in text)
        {
            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                KeyPress(Keys.Enter);
                continue;
            }

            SendUnicodeKey(ch, false);
            SendUnicodeKey(ch, true);
        }
    }

    public void KeyPress(Keys key)
    {
        KeyDown(key);
        KeyUp(key);
    }

    public void KeyDown(Keys key)
    {
        SendVirtualKey(key, false);
    }

    public void KeyUp(Keys key)
    {
        SendVirtualKey(key, true);
    }

    public void HotKey(params Keys[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        if (keys.Length == 0)
        {
            return;
        }

        foreach (var key in keys)
        {
            KeyDown(key);
        }

        for (var i = keys.Length - 1; i >= 0; i--)
        {
            KeyUp(keys[i]);
        }
    }

    private static void SendVirtualKey(Keys key, bool keyUp)
    {
        var input = new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    WVk = (ushort)(key & Keys.KeyCode),
                    WScan = 0,
                    DwFlags = keyUp ? KeyeventfKeyUp : 0,
                    Time = 0,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendKeyboardInput(input);
    }

    private static void SendUnicodeKey(char ch, bool keyUp)
    {
        var input = new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    WVk = 0,
                    WScan = (ushort)ch,
                    DwFlags = KeyeventfUnicode | (keyUp ? KeyeventfKeyUp : 0),
                    Time = 0,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendKeyboardInput(input);
    }

    private static void SendKeyboardInput(Input input)
    {
        var sent = SendInput(1, new[] { input }, Marshal.SizeOf<Input>());
        if (sent != 1)
        {
            throw new InvalidOperationException($"键盘输入发送失败，Win32Error={Marshal.GetLastWin32Error()}");
        }
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
        public KeyboardInput Keyboard;

        [FieldOffset(0)]
        public MouseInput Mouse;

        [FieldOffset(0)]
        public HardwareInput Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
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

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint UMsg;
        public ushort WParamL;
        public ushort WParamH;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
}
