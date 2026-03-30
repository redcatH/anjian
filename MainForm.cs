using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class MainForm : Form
{
    private const int HotKeyCapture = 1;
    private const int HotKeyCopyPoint = 2;
    private const int WmHotKey = 0x0312;

    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;

    private readonly Timer _cursorTimer;
    private readonly IMouseService _mouseService = new Win32MouseService();
    private readonly List<Point> _capturedPoints = new();

    public MainForm()
    {
        InitializeComponent();

        _cursorTimer = new Timer { Interval = 150 };
        _cursorTimer.Tick += (_, _) => UpdateLiveCursorPosition();
        _cursorTimer.Start();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        RegisterHotKeys();
        UpdateStatus("全局热键已注册。");
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        UnregisterHotKeys();
        base.OnHandleDestroyed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotKey)
        {
            switch (m.WParam.ToInt32())
            {
                case HotKeyCapture:
                    CaptureCurrentPosition();
                    break;
                case HotKeyCopyPoint:
                    CaptureCurrentPosition();
                    CopyCurrentPoint();
                    break;
            }
        }

        base.WndProc(ref m);
    }

    private void CaptureCurrentPosition()
    {
        var point = Cursor.Position;
        _capturedPoints.Insert(0, point);
        txtX.Text = point.X.ToString();
        txtY.Text = point.Y.ToString();
        lstHistory.Items.Insert(0, $"X={point.X}, Y={point.Y}");
        lstHistory.SelectedIndex = 0;
        UpdateStatus($"已采集坐标：X={point.X}, Y={point.Y}");
    }

    private void CopyCurrentPoint()
    {
        if (!TryGetCurrentPoint(out var point))
        {
            return;
        }

        Clipboard.SetText($"{point.X}, {point.Y}");
        UpdateStatus("当前坐标已复制到剪贴板。");
    }

    private void LoadSelectedPoint()
    {
        if (lstHistory.SelectedIndex < 0 || lstHistory.SelectedIndex >= _capturedPoints.Count)
        {
            return;
        }

        var point = _capturedPoints[lstHistory.SelectedIndex];
        txtX.Text = point.X.ToString();
        txtY.Text = point.Y.ToString();
        UpdateStatus($"已载入坐标：X={point.X}, Y={point.Y}");
    }

    private void MoveMouseAndGenerate()
    {
        if (!TryGetCurrentPoint(out var point))
        {
            return;
        }

        _mouseService.MoveTo(point.X, point.Y);
        SetSnippet(CodeSnippetBuilder.BuildMouseMove(point.X, point.Y));
        UpdateStatus("已执行鼠标移动，并生成 C# 代码。");
    }

    private void ClickMouseAndGenerate()
    {
        if (!TryGetCurrentPoint(out var point))
        {
            return;
        }

        _mouseService.LeftClick(point.X, point.Y);
        SetSnippet(CodeSnippetBuilder.BuildMouseClick(point.X, point.Y));
        UpdateStatus("已执行鼠标单击，并生成 C# 代码。");
    }

    private void DoubleClickMouseAndGenerate()
    {
        if (!TryGetCurrentPoint(out var point))
        {
            return;
        }

        if (!int.TryParse(txtDoubleClickInterval.Text, out var interval) || interval <= 0)
        {
            MessageBox.Show(this, "双击间隔必须是大于 0 的整数。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _mouseService.LeftDoubleClick(point.X, point.Y, interval);
        SetSnippet(CodeSnippetBuilder.BuildMouseDoubleClick(point.X, point.Y, interval));
        UpdateStatus("已执行鼠标双击，并生成 C# 代码。");
    }

    private bool TryGetCurrentPoint(out Point point)
    {
        if (int.TryParse(txtX.Text, out var x) && int.TryParse(txtY.Text, out var y))
        {
            point = new Point(x, y);
            return true;
        }

        point = default;
        MessageBox.Show(this, "请先采集一个鼠标坐标。", "缺少坐标", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
    }

    private void SetSnippet(CodeSnippetResult snippet)
    {
        lblSnippetTitle.Text = snippet.Title;
        txtSnippet.Text = snippet.Description is null
            ? snippet.Code
            : $"// {snippet.Description}{Environment.NewLine}{snippet.Code}";
    }

    private void UpdateLiveCursorPosition()
    {
        if (lblLivePosition is null)
        {
            return;
        }

        var point = Cursor.Position;
        lblLivePosition.Text = $"当前鼠标：X={point.X}，Y={point.Y}";
    }

    private void UpdateStatus(string text)
    {
        if (lblStatus is null)
        {
            return;
        }

        lblStatus.Text = text;
    }

    private void RegisterHotKeys()
    {
        RegisterHotKeyOrThrow(HotKeyCapture, ModControl | ModAlt, Keys.D1, "Ctrl + Alt + 1");
        RegisterHotKeyOrThrow(HotKeyCopyPoint, ModControl | ModAlt | ModShift, Keys.D1, "Ctrl + Alt + Shift + 1");
    }

    private void UnregisterHotKeys()
    {
        UnregisterHotKey(Handle, HotKeyCapture);
        UnregisterHotKey(Handle, HotKeyCopyPoint);
    }

    private void RegisterHotKeyOrThrow(int id, uint modifiers, Keys key, string displayName)
    {
        if (!RegisterHotKey(Handle, id, modifiers, (uint)key))
        {
            throw new InvalidOperationException($"热键注册失败：{displayName}");
        }
    }

    private void btnCapturePoint_Click(object? sender, EventArgs e) => CaptureCurrentPosition();

    private void btnCopyPoint_Click(object? sender, EventArgs e) => CopyCurrentPoint();

    private void btnMoveMouse_Click(object? sender, EventArgs e) => MoveMouseAndGenerate();

    private void btnLeftClick_Click(object? sender, EventArgs e) => ClickMouseAndGenerate();

    private void btnDoubleClick_Click(object? sender, EventArgs e) => DoubleClickMouseAndGenerate();

    private void btnOpenImageMatch_Click(object? sender, EventArgs e)
    {
        using var form = new ImageMatchTestForm();
        form.ShowDialog(this);
    }

    private void btnOpenAmountOcr_Click(object? sender, EventArgs e)
    {
        using var form = new AmountOcrTestForm();
        form.ShowDialog(this);
    }

    private void btnCopySnippet_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtSnippet.Text);
        UpdateStatus("代码片段已复制到剪贴板。");
    }

    private void btnClearSnippet_Click(object? sender, EventArgs e)
    {
        lblSnippetTitle.Text = "当前代码片段";
        txtSnippet.Clear();
        UpdateStatus("代码片段已清空。");
    }

    private void lstHistory_DoubleClick(object? sender, EventArgs e) => LoadSelectedPoint();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
