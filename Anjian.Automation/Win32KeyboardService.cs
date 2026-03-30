using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Anjian;

public sealed class Win32KeyboardService : IKeyboardService
{
    private const uint InputKeyboard = 1;
    private const uint KeyeventfKeyUp = 0x0002;
    private const uint KeyeventfUnicode = 0x0004;

    public void TextInput(string text)
    {
        TextInput(text, new KeyboardTextInputOptions());
    }

    public void TextInput(string text, KeyboardTextInputOptions options)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(options);

        if (options.PerCharacterDelayMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "逐字延时不能小于 0。");
        }

        switch (options.Mode)
        {
            case KeyboardTextInputMode.ClipboardPaste:
                PasteText(text, new KeyboardPasteOptions(
                    options.RestoreClipboard,
                    options.PasteMode,
                    options.ClipboardSettleDelayMs,
                    options.PasteSettleDelayMs));
                break;
            default:
                SendInputText(text, options.PerCharacterDelayMs);
                break;
        }
    }

    public void PasteText(string text, KeyboardPasteOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        var actualOptions = options ?? new KeyboardPasteOptions();
        IDataObject? backup = null;

        try
        {
            if (actualOptions.RestoreClipboard)
            {
                backup = RunSta(Clipboard.GetDataObject);
            }

            RunSta(() => Clipboard.SetText(text));
            Thread.Sleep(Math.Max(0, actualOptions.ClipboardSettleDelayMs));

            switch (actualOptions.PasteMode)
            {
                case KeyboardPasteMode.ShiftInsert:
                    HotKey(Keys.ShiftKey, Keys.Insert);
                    break;
                default:
                    HotKey(Keys.ControlKey, Keys.V);
                    break;
            }

            Thread.Sleep(Math.Max(0, actualOptions.PasteSettleDelayMs));
        }
        finally
        {
            if (actualOptions.RestoreClipboard && backup is not null)
            {
                try
                {
                    RunSta(() => Clipboard.SetDataObject(backup, true));
                }
                catch
                {
                    // 剪贴板恢复失败不影响主流程，保留原始输入结果。
                }
            }
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

    private static void SendInputText(string text, int perCharacterDelayMs)
    {
        foreach (var ch in text)
        {
            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                KeyPressStatic(Keys.Enter);
                ApplyCharacterDelay(perCharacterDelayMs);
                continue;
            }

            SendUnicodeKey(ch, false);
            SendUnicodeKey(ch, true);
            ApplyCharacterDelay(perCharacterDelayMs);
        }
    }

    private static void ApplyCharacterDelay(int perCharacterDelayMs)
    {
        if (perCharacterDelayMs > 0)
        {
            Thread.Sleep(perCharacterDelayMs);
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

    private static void KeyPressStatic(Keys key)
    {
        SendVirtualKey(key, false);
        SendVirtualKey(key, true);
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

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        using var completed = new ManualResetEventSlim(false);
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        completed.Wait();

        if (exception is not null)
        {
            throw new InvalidOperationException($"剪贴板操作失败：{exception.Message}", exception);
        }
    }

    private static T RunSta<T>(Func<T> func)
    {
        Exception? exception = null;
        T? result = default;
        using var completed = new ManualResetEventSlim(false);
        var thread = new Thread(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        completed.Wait();

        if (exception is not null)
        {
            throw new InvalidOperationException($"剪贴板操作失败：{exception.Message}", exception);
        }

        return result!;
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
