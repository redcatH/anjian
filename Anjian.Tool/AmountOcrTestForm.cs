using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class AmountOcrTestForm : Form
{
    private readonly ScreenCaptureService _screenCaptureService = new();
    private readonly ImagePreprocessService _imagePreprocessService = new();
    private readonly IOcrService _ocrService = new TesseractAmountOcrService();

    private Bitmap? _capturedBitmap;
    private Bitmap? _processedBitmap;
    private AmountOcrOptions? _lastOptions;

    public AmountOcrTestForm()
    {
        InitializeComponent();
        SetDefaultRegion();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capturedBitmap?.Dispose();
            _processedBitmap?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void SetDefaultRegion()
    {
        var virtualScreen = ScreenCaptureService.GetVirtualScreenBounds();
        txtRegionX.Text = virtualScreen.Left.ToString();
        txtRegionY.Text = virtualScreen.Top.ToString();
        txtRegionWidth.Text = Math.Min(400, virtualScreen.Width).ToString();
        txtRegionHeight.Text = Math.Min(120, virtualScreen.Height).ToString();
        AppendLog($"已加载默认区域：{txtRegionX.Text}, {txtRegionY.Text}, {txtRegionWidth.Text}, {txtRegionHeight.Text}");
        GenerateSnippet(false);
    }

    private void CaptureRegionPreview()
    {
        if (!TryReadRegion(out var region))
        {
            return;
        }

        try
        {
            ReplaceCapturedBitmap(_screenCaptureService.Capture(region));
            RefreshProcessedPreview();
            ResetResult();
            AppendLog($"已截图区域：X={region.X}, Y={region.Y}, W={region.Width}, H={region.Height}");
            GenerateSnippet(false);
        }
        catch (Exception ex)
        {
            ShowError($"截图失败：{ex.Message}");
        }
    }

    private async Task ExecuteOcrAsync()
    {
        if (!TryReadRegion(out var region))
        {
            return;
        }

        if (!TryReadOptions(out var options))
        {
            return;
        }

        try
        {
            if (_capturedBitmap is null)
            {
                ReplaceCapturedBitmap(_screenCaptureService.Capture(region));
                AppendLog("未检测到截图内容，已在识别前自动截图。");
            }

            _lastOptions = options;
            ReplaceProcessedBitmap(_imagePreprocessService.Preprocess(new Bitmap(_capturedBitmap!), options));
            AppendLog($"开始金额识别。灰度={options.UseGrayscale}，二值化={options.UseBinarization}，放大2x={options.Scale2x}。");

            var result = await _ocrService.RecognizeAmountAsync(_processedBitmap!);
            UpdateResult(result);
            AppendLog($"识别完成。状态={result.Success}，金额={result.Amount?.ToString() ?? "-"}，耗时={result.ElapsedMilliseconds} ms，消息={result.Message}");
            GenerateSnippet(false);
        }
        catch (Exception ex)
        {
            ShowError($"金额识别失败：{ex.Message}");
        }
    }

    private void GenerateSnippet(bool showError)
    {
        if (!TryReadRegion(out var region))
        {
            if (!showError)
            {
                txtSnippet.Clear();
            }

            return;
        }

        var options = _lastOptions;
        if (options is null)
        {
            if (!TryReadOptions(out var currentOptions))
            {
                if (!showError)
                {
                    txtSnippet.Clear();
                }

                return;
            }

            options = currentOptions;
        }

        var snippet = CodeSnippetBuilder.BuildAmountOcr(region, options);
        txtSnippet.Text = snippet.Description is null
            ? snippet.Code
            : $"// {snippet.Description}{Environment.NewLine}{snippet.Code}";

        if (showError)
        {
            AppendLog("已生成金额识别 C# 代码。");
        }
    }

    private bool TryReadRegion(out Rectangle region)
    {
        region = Rectangle.Empty;
        if (!int.TryParse(txtRegionX.Text, out var x) ||
            !int.TryParse(txtRegionY.Text, out var y) ||
            !int.TryParse(txtRegionWidth.Text, out var width) ||
            !int.TryParse(txtRegionHeight.Text, out var height))
        {
            if (Visible)
            {
                ShowError("区域参数必须是整数。");
            }

            return false;
        }

        if (width <= 0 || height <= 0)
        {
            if (Visible)
            {
                ShowError("Width 和 Height 必须大于 0。");
            }

            return false;
        }

        region = new Rectangle(x, y, width, height);
        var virtualScreen = ScreenCaptureService.GetVirtualScreenBounds();
        if (!virtualScreen.Contains(region))
        {
            if (Visible)
            {
                ShowError("识别区域超出了当前虚拟屏幕范围。");
            }

            return false;
        }

        return true;
    }

    private bool TryReadOptions(out AmountOcrOptions options)
    {
        options = default!;
        if (!int.TryParse(txtThreshold.Text, out var threshold) || threshold < 0 || threshold > 255)
        {
            ShowError("二值阈值必须在 0 到 255 之间。");
            return false;
        }

        options = new AmountOcrOptions(
            chkGrayscale.Checked,
            chkBinarization.Checked,
            threshold,
            chkScale2x.Checked);
        return true;
    }

    private void RefreshProcessedPreview()
    {
        if (_capturedBitmap is null || !TryReadOptions(out var options))
        {
            return;
        }

        ReplaceProcessedBitmap(_imagePreprocessService.Preprocess(new Bitmap(_capturedBitmap), options));
        _lastOptions = options;
        GenerateSnippet(false);
    }

    private void PickRegion()
    {
        Rectangle? initialRegion = null;
        if (TryReadRegion(out var currentRegion))
        {
            initialRegion = currentRegion;
        }

        var selectedRegion = RegionSelectionService.PickRegion(this, initialRegion);
        if (selectedRegion is null)
        {
            AppendLog("已取消区域选择。");
            return;
        }

        txtRegionX.Text = selectedRegion.Value.X.ToString();
        txtRegionY.Text = selectedRegion.Value.Y.ToString();
        txtRegionWidth.Text = selectedRegion.Value.Width.ToString();
        txtRegionHeight.Text = selectedRegion.Value.Height.ToString();
        AppendLog($"已选定区域：X={selectedRegion.Value.X}, Y={selectedRegion.Value.Y}, W={selectedRegion.Value.Width}, H={selectedRegion.Value.Height}");
        AppendLog(CodeSnippetBuilder.FormatRectangleDeclaration("rect", selectedRegion.Value));
        GenerateSnippet(false);
    }

    private void ReplaceCapturedBitmap(Bitmap bitmap)
    {
        _capturedBitmap?.Dispose();
        _capturedBitmap = bitmap;
        picSource.Image = _capturedBitmap;
    }

    private void ReplaceProcessedBitmap(Bitmap bitmap)
    {
        _processedBitmap?.Dispose();
        _processedBitmap = bitmap;
        picProcessed.Image = _processedBitmap;
    }

    private void UpdateResult(AmountOcrResult result)
    {
        lblStatusValue.Text = result.Success ? "成功" : "失败";
        lblAmountValue.Text = result.Amount?.ToString() ?? "-";
        lblElapsedValue.Text = result.ElapsedMilliseconds.ToString();
        txtRaw.Text = result.RawText;
        txtNormalized.Text = result.NormalizedText;
    }

    private void ResetResult()
    {
        lblStatusValue.Text = "-";
        lblAmountValue.Text = "-";
        lblElapsedValue.Text = "-";
        txtRaw.Clear();
        txtNormalized.Clear();
    }

    private void AppendLog(string message)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void ShowError(string message)
    {
        AppendLog(message);
        MessageBox.Show(this, message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void btnCaptureRegion_Click(object? sender, EventArgs e) => CaptureRegionPreview();

    private void btnPickRegion_Click(object? sender, EventArgs e) => PickRegion();

    private async void btnExecuteOcr_Click(object? sender, EventArgs e) => await ExecuteOcrAsync();

    private void btnGenerateSnippet_Click(object? sender, EventArgs e) => GenerateSnippet(true);

    private void btnCopySnippet_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtSnippet.Text);
        AppendLog("代码片段已复制到剪贴板。");
    }
}
