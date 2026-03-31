using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class ImageMatchTestForm : Form
{
    private readonly ScreenCaptureService _screenCaptureService = new();
    private readonly IImageMatcher _imageMatcher = new TemplateMatcher();

    private Bitmap? _capturedBitmap;
    private Bitmap? _previewBitmap;
    private Bitmap? _templateBitmap;
    private ImageMatchOptions? _lastOptions;

    public ImageMatchTestForm()
    {
        InitializeComponent();
        InitializeOptionCombos();
        SetDefaultRegion();
    }

    private void InitializeOptionCombos()
    {
        cboMatchMode.Items.Clear();
        cboMatchMode.Items.Add(new MatchModeItem("只找第一个", ImageMatchMode.First));
        cboMatchMode.Items.Add(new MatchModeItem("找全部", ImageMatchMode.All));
        cboMatchMode.SelectedIndex = 0;

        cboScanDirection.Items.Clear();
        cboScanDirection.Items.Add(new ScanDirectionItem("从上到下", ImageScanDirection.TopToBottom));
        cboScanDirection.Items.Add(new ScanDirectionItem("从下到上", ImageScanDirection.BottomToTop));
        cboScanDirection.Items.Add(new ScanDirectionItem("从中间向上下扩散", ImageScanDirection.CenterOutVertical));
        cboScanDirection.Items.Add(new ScanDirectionItem("从左到右，按列扫描", ImageScanDirection.LeftToRight));
        cboScanDirection.Items.Add(new ScanDirectionItem("从右到左，按列扫描", ImageScanDirection.RightToLeft));
        cboScanDirection.Items.Add(new ScanDirectionItem("从中间向两侧扩散，按列扫描", ImageScanDirection.CenterOutHorizontal));
        cboScanDirection.SelectedIndex = 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capturedBitmap?.Dispose();
            _previewBitmap?.Dispose();
            _templateBitmap?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void SetDefaultRegion()
    {
        var virtualScreen = ScreenCaptureService.GetVirtualScreenBounds();
        txtRegionX.Text = virtualScreen.Left.ToString();
        txtRegionY.Text = virtualScreen.Top.ToString();
        txtRegionWidth.Text = Math.Min(800, virtualScreen.Width).ToString();
        txtRegionHeight.Text = Math.Min(600, virtualScreen.Height).ToString();
        AppendLog($"已加载默认区域：{txtRegionX.Text}, {txtRegionY.Text}, {txtRegionWidth.Text}, {txtRegionHeight.Text}");
        GenerateSnippet(false);
    }

    private void SelectTemplate()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "图片文件|*.png;*.bmp;*.jpg;*.jpeg",
            Title = "选择模板图片"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using var tempBitmap = new Bitmap(dialog.FileName);
            ReplaceTemplateBitmap(new Bitmap(tempBitmap));
            txtTemplatePath.Text = dialog.FileName;
            AppendLog($"模板加载成功：{dialog.FileName}");
            GenerateSnippet(false);
        }
        catch (Exception ex)
        {
            ShowError($"模板加载失败：{ex.Message}");
        }
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
            ResetResult();
            AppendLog($"已截图区域：X={region.X}, Y={region.Y}, W={region.Width}, H={region.Height}");
            GenerateSnippet(false);
        }
        catch (Exception ex)
        {
            ShowError($"截图失败：{ex.Message}");
        }
    }

    private void ExecuteMatch()
    {
        if (!TryReadRegion(out var region))
        {
            return;
        }

        if (!TryLoadTemplateBitmap(out var template))
        {
            return;
        }

        if (!TryReadOptions(region, template, out var options))
        {
            return;
        }

        try
        {
            if (_capturedBitmap is null)
            {
                ReplaceCapturedBitmap(_screenCaptureService.Capture(region));
                AppendLog("未检测到截图内容，已在匹配前自动截图。");
            }

            _lastOptions = options;
            AppendLog($"开始匹配。模式={GetMatchModeText(options.MatchMode)}，方向={GetScanDirectionText(options.ScanDirection)}，Step={options.Step}。");

            var result = _imageMatcher.Find(_capturedBitmap!, template, options);
            UpdateResult(result);
            DrawHits(template.Size, result.Hits);
            AppendMatchLog(result);
            GenerateSnippet(false);
        }
        catch (Exception ex)
        {
            ShowError($"匹配失败：{ex.Message}");
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

        if (string.IsNullOrWhiteSpace(txtTemplatePath.Text))
        {
            if (showError)
            {
                ShowError("请先选择模板图片。");
            }
            else
            {
                txtSnippet.Clear();
            }

            return;
        }

        var options = _lastOptions;
        if (options is null)
        {
            if (!TryLoadTemplateBitmap(out var template) || !TryReadOptions(region, template, out var currentOptions))
            {
                if (!showError)
                {
                    txtSnippet.Clear();
                }

                return;
            }

            options = currentOptions;
        }

        var snippet = CodeSnippetBuilder.BuildImageMatch(region, txtTemplatePath.Text, options);
        txtSnippet.Text = snippet.Description is null
            ? snippet.Code
            : $"// {snippet.Description}{Environment.NewLine}{snippet.Code}";

        if (showError)
        {
            AppendLog("已生成找图 C# 代码。");
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
                ShowError("搜索区域超出了当前虚拟屏幕范围。");
            }

            return false;
        }

        return true;
    }

    private bool TryLoadTemplateBitmap(out Bitmap template)
    {
        if (_templateBitmap is not null)
        {
            template = _templateBitmap;
            return true;
        }

        if (string.IsNullOrWhiteSpace(txtTemplatePath.Text))
        {
            ShowError("请先选择模板图片。");
            template = null!;
            return false;
        }

        if (!File.Exists(txtTemplatePath.Text))
        {
            ShowError("模板图片路径不存在。");
            template = null!;
            return false;
        }

        try
        {
            using var tempBitmap = new Bitmap(txtTemplatePath.Text);
            ReplaceTemplateBitmap(new Bitmap(tempBitmap));
            template = _templateBitmap!;
            return true;
        }
        catch (Exception ex)
        {
            ShowError($"重新加载模板失败：{ex.Message}");
            template = null!;
            return false;
        }
    }

    private bool TryReadOptions(Rectangle region, Bitmap template, out ImageMatchOptions options)
    {
        options = null!;

        if (!double.TryParse(txtThreshold.Text, out var threshold) || threshold <= 0d || threshold > 1d)
        {
            ShowError("Threshold 必须大于 0 且小于等于 1。");
            return false;
        }

        if (!int.TryParse(txtStep.Text, out var step) || step <= 0)
        {
            ShowError("Step 必须是正整数。");
            return false;
        }

        if (template.Width > region.Width || template.Height > region.Height)
        {
            ShowError("模板图片必须小于搜索区域。");
            return false;
        }

        var selectedMode = (MatchModeItem)cboMatchMode.SelectedItem!;
        var selectedDirection = (ScanDirectionItem)cboScanDirection.SelectedItem!;
        options = new ImageMatchOptions(
            new Rectangle(0, 0, region.Width, region.Height),
            threshold,
            chkGrayscale.Checked,
            step,
            selectedMode.Mode,
            selectedDirection.Direction);
        return true;
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
        ShowPreview(bitmap);
    }

    private void ReplaceTemplateBitmap(Bitmap bitmap)
    {
        _templateBitmap?.Dispose();
        _templateBitmap = bitmap;
        picTemplate.Image = _templateBitmap;
    }

    private void ShowPreview(Bitmap bitmap)
    {
        _previewBitmap?.Dispose();
        _previewBitmap = new Bitmap(bitmap);
        picSource.Image = _previewBitmap;
    }

    private void DrawHits(Size templateSize, System.Collections.Generic.IReadOnlyList<ImageMatchHit> hits)
    {
        if (_capturedBitmap is null)
        {
            return;
        }

        ShowPreview(_capturedBitmap);
        if (_previewBitmap is null || hits.Count == 0)
        {
            return;
        }

        using var graphics = Graphics.FromImage(_previewBitmap);
        using var pen = new Pen(Color.Red, 2f);
        using var brush = new SolidBrush(Color.FromArgb(200, Color.Yellow));
        using var font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);

        for (var index = 0; index < hits.Count; index++)
        {
            var hit = hits[index];
            var rect = new Rectangle(hit.Location, templateSize);
            graphics.DrawRectangle(pen, rect);
            graphics.FillRectangle(brush, rect.X, rect.Y, 44, 18);
            graphics.DrawString((index + 1).ToString(), font, Brushes.Red, rect.X + 2, rect.Y + 1);
        }

        picSource.Refresh();
    }

    private void UpdateResult(ImageMatchResult result)
    {
        lblStatusValue.Text = result.Success ? "成功" : "失败";
        lblLocationValue.Text = result.Location is null ? "-" : $"X={result.Location.Value.X}, Y={result.Location.Value.Y}";
        lblScoreValue.Text = result.Score.ToString("F4");
        lblElapsedValue.Text = result.ElapsedMilliseconds.ToString();
        lblCountValue.Text = result.Hits.Count.ToString();
    }

    private void ResetResult()
    {
        lblStatusValue.Text = "-";
        lblLocationValue.Text = "-";
        lblScoreValue.Text = "-";
        lblElapsedValue.Text = "-";
        lblCountValue.Text = "-";

        if (_capturedBitmap is not null)
        {
            ShowPreview(_capturedBitmap);
        }
    }

    private void AppendMatchLog(ImageMatchResult result)
    {
        AppendLog($"匹配完成。状态={result.Success}，首个分数={result.Score:F4}，耗时={result.ElapsedMilliseconds} ms，消息={result.Message}");

        if (result.Hits.Count == 0)
        {
            AppendLog("本次没有命中坐标。");
            return;
        }

        foreach (var item in result.Hits.Select((hit, index) => new { hit, index }))
        {
            AppendLog($"命中 {item.index + 1}：X={item.hit.Location.X}, Y={item.hit.Location.Y}, 分数={item.hit.Score:F4}");
        }
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

    private static string GetMatchModeText(ImageMatchMode mode)
    {
        return mode == ImageMatchMode.All ? "找全部" : "只找第一个";
    }

    private static string GetScanDirectionText(ImageScanDirection direction)
    {
        return direction switch
        {
            ImageScanDirection.BottomToTop => "从下到上",
            ImageScanDirection.CenterOutVertical => "从中间向上下扩散",
            ImageScanDirection.LeftToRight => "从左到右，按列扫描",
            ImageScanDirection.RightToLeft => "从右到左，按列扫描",
            ImageScanDirection.CenterOutHorizontal => "从中间向两侧扩散，按列扫描",
            _ => "从上到下"
        };
    }

    private void btnSelectTemplate_Click(object? sender, EventArgs e) => SelectTemplate();

    private void btnPickRegion_Click(object? sender, EventArgs e) => PickRegion();

    private void btnCaptureRegion_Click(object? sender, EventArgs e) => CaptureRegionPreview();

    private void btnExecuteMatch_Click(object? sender, EventArgs e) => ExecuteMatch();

    private void btnGenerateSnippet_Click(object? sender, EventArgs e) => GenerateSnippet(true);

    private void btnCopySnippet_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtSnippet.Text);
        AppendLog("代码片段已复制到剪贴板。");
    }

    private sealed record MatchModeItem(string Text, ImageMatchMode Mode)
    {
        public override string ToString() => Text;
    }

    private sealed record ScanDirectionItem(string Text, ImageScanDirection Direction)
    {
        public override string ToString() => Text;
    }
}
