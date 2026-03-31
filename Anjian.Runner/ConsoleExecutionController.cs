using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 步骤 {index + 1}：{stepName}，模式：{GetModeText(Mode)}");
    }

    public void WaitForContinue(string reason)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已暂停：{reason}");
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 请选择：F3 继续，或 F4 跳过当前号码。");
        var mousePositionBeforeWait = Cursor.Position;

        using var cancellation = new CancellationTokenSource();
        var hotKeyTask = Task.Run(
            () => _continueSignal.WaitForContinueDecision(reason, cancellation.Token),
            cancellation.Token);

        var decision = ContinueChoiceDialog.Show(reason, hotKeyTask);
        cancellation.Cancel();

        try
        {
            hotKeyTask.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // User clicked dialog buttons first; background hotkey wait was canceled intentionally.
        }

        if (decision == ContinueDecision.SkipCurrentRun)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已收到 F4，跳过当前号码。");
            throw new SkipCurrentRunException("用户按下 F4，跳过当前号码。");
        }

        RestoreMousePosition(mousePositionBeforeWait);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已收到 F3，继续执行。");
    }

    private static void RestoreMousePosition(Point position)
    {
        if (Cursor.Position == position)
        {
            return;
        }

        Cursor.Position = position;
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

    private sealed class ContinueChoiceDialog : Form
    {
        private ContinueDecision _decision = ContinueDecision.Continue;
        private readonly Task<ContinueDecision> _hotKeyTask;
        private readonly System.Windows.Forms.Timer _pollTimer;

        private ContinueChoiceDialog(string reason, Task<ContinueDecision> hotKeyTask)
        {
            _hotKeyTask = hotKeyTask ?? throw new ArgumentNullException(nameof(hotKeyTask));

            Text = "执行已暂停";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            KeyPreview = true;
            ClientSize = new Size(520, 190);

            var messageLabel = new Label
            {
                AutoSize = false,
                Location = new Point(16, 16),
                Size = new Size(488, 92),
                Text = $"{reason}\n\n请选择：\n全局 F3 或 点击“继续执行”\n全局 F4 或 点击“跳过本次”",
            };

            var continueButton = new Button
            {
                Text = "继续执行 (F3)",
                Location = new Point(248, 130),
                Size = new Size(120, 32),
                DialogResult = DialogResult.OK
            };

            var skipButton = new Button
            {
                Text = "跳过本次 (F4)",
                Location = new Point(384, 130),
                Size = new Size(120, 32),
                DialogResult = DialogResult.Abort
            };

            continueButton.Click += (_, _) =>
            {
                _decision = ContinueDecision.Continue;
                Close();
            };

            skipButton.Click += (_, _) =>
            {
                _decision = ContinueDecision.SkipCurrentRun;
                Close();
            };

            Controls.Add(messageLabel);
            Controls.Add(continueButton);
            Controls.Add(skipButton);
            AcceptButton = continueButton;
            CancelButton = skipButton;

            _pollTimer = new System.Windows.Forms.Timer
            {
                Interval = 80
            };

            _pollTimer.Tick += (_, _) =>
            {
                if (!_hotKeyTask.IsCompleted)
                {
                    return;
                }

                if (_hotKeyTask.IsCompletedSuccessfully)
                {
                    _decision = _hotKeyTask.Result;
                    Close();
                    return;
                }

                if (_hotKeyTask.IsFaulted)
                {
                    if (_hotKeyTask.Exception is not null)
                    {
                        throw _hotKeyTask.Exception;
                    }

                    throw new InvalidOperationException("全局热键监听发生未知错误。");
                }
            };

            Shown += (_, _) => _pollTimer.Start();
            FormClosed += (_, _) => _pollTimer.Stop();
        }

        public static ContinueDecision Show(string reason, Task<ContinueDecision> hotKeyTask)
        {
            using var dialog = new ContinueChoiceDialog(reason, hotKeyTask);
            dialog.ShowDialog();
            return dialog._decision;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3)
            {
                _decision = ContinueDecision.Continue;
                Close();
                return true;
            }

            if (keyData == Keys.F4)
            {
                _decision = ContinueDecision.SkipCurrentRun;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
