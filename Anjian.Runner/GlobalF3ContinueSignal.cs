using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Anjian;

internal sealed class GlobalF3ContinueSignal : IGlobalContinueSignal, IDisposable
{
    private const int ContinueHotKeyId = 9001;
    private const int SkipHotKeyId = 9002;
    private const uint VkF3 = 0x72;
    private const uint VkF4 = 0x73;
    private const uint ModNoRepeat = 0x4000;
    private const uint WmHotKey = 0x0312;
    private const uint WmQuit = 0x0012;

    private readonly AutoResetEvent _decisionEvent = new(false);
    private readonly ManualResetEventSlim _readyEvent = new(false);
    private readonly Thread _messageThread;
    private volatile bool _isWaiting;
    private volatile ContinueDecision _decision = ContinueDecision.Continue;
    private uint _threadId;
    private bool _disposed;
    private Exception? _startupException;

    public GlobalF3ContinueSignal()
    {
        _messageThread = new Thread(MessageLoop)
        {
            IsBackground = true,
            Name = "GlobalF3HotKeyThread"
        };
        _messageThread.Start();
        _readyEvent.Wait();

        if (_startupException is not null)
        {
            throw new InvalidOperationException($"全局 F3 热键初始化失败：{_startupException.Message}", _startupException);
        }
    }

    public ContinueDecision WaitForContinueDecision(string reason, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _ = reason;

        _isWaiting = true;
        try
        {
            if (!cancellationToken.CanBeCanceled)
            {
                _decisionEvent.WaitOne();
                return _decision;
            }

            var signaledIndex = WaitHandle.WaitAny(new[] { _decisionEvent, cancellationToken.WaitHandle });
            if (signaledIndex == 1)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return _decision;
        }
        finally
        {
            _isWaiting = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_threadId != 0)
        {
            PostThreadMessage(_threadId, WmQuit, UIntPtr.Zero, IntPtr.Zero);
        }

        _messageThread.Join();
        _decisionEvent.Dispose();
        _readyEvent.Dispose();
    }

    private void MessageLoop()
    {
        try
        {
            _threadId = GetCurrentThreadId();
            PeekMessage(out _, IntPtr.Zero, 0, 0, 0);

            if (!RegisterHotKey(IntPtr.Zero, ContinueHotKeyId, ModNoRepeat, VkF3))
            {
                throw new InvalidOperationException($"注册全局 F3 热键失败，Win32Error={Marshal.GetLastWin32Error()}");
            }

            if (!RegisterHotKey(IntPtr.Zero, SkipHotKeyId, ModNoRepeat, VkF4))
            {
                throw new InvalidOperationException($"注册全局 F4 热键失败，Win32Error={Marshal.GetLastWin32Error()}");
            }

            _readyEvent.Set();

            while (GetMessage(out var message, IntPtr.Zero, 0, 0) > 0)
            {
                if (message.message != WmHotKey || !_isWaiting)
                {
                    continue;
                }

                if (message.wParam == (UIntPtr)ContinueHotKeyId)
                {
                    _decision = ContinueDecision.Continue;
                    _decisionEvent.Set();
                    continue;
                }

                if (message.wParam == (UIntPtr)SkipHotKeyId)
                {
                    _decision = ContinueDecision.SkipCurrentRun;
                    _decisionEvent.Set();
                }
            }
        }
        catch (Exception ex)
        {
            _startupException = ex;
            _readyEvent.Set();
        }
        finally
        {
            if (_threadId != 0)
            {
                UnregisterHotKey(IntPtr.Zero, ContinueHotKeyId);
                UnregisterHotKey(IntPtr.Zero, SkipHotKeyId);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int ptX;
        public int ptY;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern sbyte GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostThreadMessage(uint idThread, uint msg, UIntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
}
