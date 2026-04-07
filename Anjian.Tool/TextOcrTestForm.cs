using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class TextOcrTestForm : Form
{
    private readonly ScreenCaptureService _screenCaptureService = new();
    private readonly ImagePreprocessService _imagePreprocessService = new();
    private readonly IGeneralOcrService _generalOcrService;
    private readonly ITableOcrService _tableOcrService;
    private readonly GeneralOcrRuntimeOptions _generalOcrRuntimeOptions;

    private Bitmap? _capturedBitmap;
    private Bitmap? _sourcePreviewBitmap;
    private Bitmap? _processedBitmap;
    private GeneralOcrOptions? _lastOptions;

    public TextOcrTestForm()
    {
        _generalOcrRuntimeOptions = new GeneralOcrRuntimeOptions(
            Provider: OcrEngineType.PaddleSharp,
            Device: GeneralOcrDeviceType.CpuMkl);
        _generalOcrService = GeneralOcrServiceFactory.Create(_imagePreprocessService, _generalOcrRuntimeOptions);
        _tableOcrService = new PaddleSharpTableOcrService(_generalOcrRuntimeOptions);

        InitializeComponent();
        SetDefaultRegion();
        AppendLog($"当前文字 OCR：{_generalOcrRuntimeOptions.Provider} / {_generalOcrRuntimeOptions.Device}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capturedBitmap?.Dispose();
            _sourcePreviewBitmap?.Dispose();
            _processedBitmap?.Dispose();

            if (_generalOcrService is IDisposable generalDisposable)
            {
                generalDisposable.Dispose();
            }

            if (_tableOcrService is IDisposable tableDisposable)
            {
                tableDisposable.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private void SetDefaultRegion()
    {
        var virtualScreen = ScreenCaptureService.GetVirtualScreenBounds();
        txtRegionX.Text = virtualScreen.Left.ToString();
        txtRegionY.Text = virtualScreen.Top.ToString();
        txtRegionWidth.Text = Math.Min(560, virtualScreen.Width).ToString();
        txtRegionHeight.Text = Math.Min(180, virtualScreen.Height).ToString();
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
            ReplaceSourcePreview(new Bitmap(_capturedBitmap!));
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
            EnsureCapturedBitmap(region);

            _lastOptions = options;
            ReplaceProcessedBitmap(_imagePreprocessService.Preprocess(new Bitmap(_capturedBitmap!), options));
            AppendLog($"开始文字识别。灰度={options.UseGrayscale}，二值化={options.UseBinarization}，阈值={options.BinarizationThreshold}，放大 2x={options.Scale2x}");

            using var bitmap = new Bitmap(_capturedBitmap!);
            var result = await _generalOcrService.RecognizeRegionsAsync(bitmap, options);
            UpdateResult(result);
            ReplaceSourcePreview(RenderOverlay(_capturedBitmap!, result.SafeRegions));
            AppendRegionLog(result.SafeRegions);
            AppendLog($"文字识别完成。状态={result.Success}，耗时={result.ElapsedMilliseconds} ms，消息={result.Message}");
            GenerateSnippet(false);
        }
        catch (Exception ex)
        {
            ShowError($"文字识别失败：{ex.Message}");
        }
    }

    private async Task ExecuteTableOcrAsync()
    {
        if (!TryReadRegion(out var region))
        {
            return;
        }

        try
        {
            EnsureCapturedBitmap(region);

            AppendLog($"开始表格识别。区域：X={region.X}, Y={region.Y}, W={region.Width}, H={region.Height}");
            using var bitmap = new Bitmap(_capturedBitmap!);
            var result = await _tableOcrService.RecognizeTableAsJsonAsync(bitmap);
            UpdateTableResult(result);
            txtSnippet.Text = BuildSnippetText(CodeSnippetBuilder.BuildTableOcrAsJson(region));
            AppendLog($"表格识别完成。状态={result.Success}，耗时={result.ElapsedMilliseconds} ms，消息={result.Message}");
        }
        catch (Exception ex)
        {
            ShowError($"表格识别失败：{ex.Message}");
        }
    }

    private void EnsureCapturedBitmap(Rectangle region)
    {
        if (_capturedBitmap is not null)
        {
            return;
        }

        ReplaceCapturedBitmap(_screenCaptureService.Capture(region));
        ReplaceSourcePreview(new Bitmap(_capturedBitmap!));
        RefreshProcessedPreview();
        AppendLog("未检测到截图内容，已在识别前自动截图。");
    }

    private void GenerateSnippet(bool appendLog)
    {
        if (!TryReadRegion(out var region))
        {
            if (!appendLog)
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
                if (!appendLog)
                {
                    txtSnippet.Clear();
                }

                return;
            }

            options = currentOptions;
        }

        txtSnippet.Text = BuildSnippetText(CodeSnippetBuilder.BuildTextOcr(region, options));

        if (appendLog)
        {
            AppendLog("已生成文字识别 C# 代码。");
        }
    }

    private static string BuildSnippetText(CodeSnippetResult snippet)
    {
        return snippet.Description is null
            ? snippet.Code
            : $"// {snippet.Description}{Environment.NewLine}{snippet.Code}";
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

    private bool TryReadOptions(out GeneralOcrOptions options)
    {
        options = default!;
        if (!int.TryParse(txtThreshold.Text, out var threshold) || threshold < 0 || threshold > 255)
        {
            ShowError("二值化阈值必须在 0 到 255 之间。");
            return false;
        }

        options = new GeneralOcrOptions(
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
    }

    private void ReplaceSourcePreview(Bitmap bitmap)
    {
        _sourcePreviewBitmap?.Dispose();
        _sourcePreviewBitmap = bitmap;
        picSource.Image = _sourcePreviewBitmap;
    }

    private void ReplaceProcessedBitmap(Bitmap bitmap)
    {
        _processedBitmap?.Dispose();
        _processedBitmap = bitmap;
        picProcessed.Image = _processedBitmap;
    }

    private static Bitmap RenderOverlay(Bitmap source, System.Collections.Generic.IReadOnlyList<GeneralOcrRegion> regions)
    {
        var overlay = new Bitmap(source);
        using var graphics = Graphics.FromImage(overlay);
        using var boxPen = new Pen(Color.LimeGreen, 2);
        using var centerBrush = new SolidBrush(Color.Red);
        using var labelBackground = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
        using var labelBrush = new SolidBrush(Color.White);
        using var font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        foreach (var region in regions)
        {
            graphics.DrawRectangle(boxPen, region.Bounds);

            var centerX = region.Center.X;
            var centerY = region.Center.Y;
            graphics.FillEllipse(centerBrush, centerX - 3, centerY - 3, 6, 6);

            var label = $"{region.Text} ({region.Score.ToString("0.00", CultureInfo.InvariantCulture)})";
            var labelSize = graphics.MeasureString(label, font);
            var labelX = Math.Max(0, region.Bounds.Left);
            var labelY = Math.Max(0, region.Bounds.Top - labelSize.Height - 4);
            graphics.FillRectangle(labelBackground, labelX, labelY, labelSize.Width + 8, labelSize.Height + 4);
            graphics.DrawString(label, font, labelBrush, labelX + 4, labelY + 2);
        }

        return overlay;
    }

    private void AppendRegionLog(System.Collections.Generic.IReadOnlyList<GeneralOcrRegion> regions)
    {
        if (regions.Count == 0)
        {
            AppendLog("未返回任何文字区域。");
            return;
        }

        for (var i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            AppendLog($"Region {i + 1}: 文本={region.Text}, 分数={region.Score.ToString("0.00", CultureInfo.InvariantCulture)}, 中心=({region.Center.X:0.##}, {region.Center.Y:0.##}), 边界=({region.Bounds.X}, {region.Bounds.Y}, {region.Bounds.Width}, {region.Bounds.Height})");
        }
    }

    private void UpdateResult(GeneralOcrResult result)
    {
        lblStatusValue.Text = result.Success ? "成功" : "失败";
        lblElapsedValue.Text = result.ElapsedMilliseconds.ToString();
        lblMessageValue.Text = result.Message;
        txtRaw.Text = result.RawText;
        txtNormalized.Text = result.NormalizedText;
    }

    private void UpdateTableResult(TableOcrResult result)
    {
        lblStatusValue.Text = result.Success ? "成功" : "失败";
        lblElapsedValue.Text = result.ElapsedMilliseconds.ToString();
        lblMessageValue.Text = result.Message;
        txtTableJson.Text = result.Json;
    }

    private void ResetResult()
    {
        lblStatusValue.Text = "-";
        lblElapsedValue.Text = "-";
        lblMessageValue.Text = "-";
        txtRaw.Clear();
        txtNormalized.Clear();
        txtTableJson.Clear();
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

    private async void btnExecuteTableJson_Click(object? sender, EventArgs e) => await ExecuteTableOcrAsync();

    private void btnGenerateSnippet_Click(object? sender, EventArgs e) => GenerateSnippet(true);

    private void btnCopySnippet_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtSnippet.Text);
        AppendLog("代码片段已复制到剪贴板。");
    }

    private void btnCopyText_Click(object? sender, EventArgs e)
    {
        var text = string.IsNullOrWhiteSpace(txtNormalized.Text) ? txtRaw.Text : txtNormalized.Text;
        Clipboard.SetText(text);
        AppendLog("识别文本已复制到剪贴板。");
    }

    private void btnCopyTableJson_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtTableJson.Text);
        AppendLog("表格 JSON 已复制到剪贴板。");
    }

    private void chkOptions_CheckedChanged(object? sender, EventArgs e) => RefreshProcessedPreview();

    private void txtThreshold_TextChanged(object? sender, EventArgs e) => RefreshProcessedPreview();
}
