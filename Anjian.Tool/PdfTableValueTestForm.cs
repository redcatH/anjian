using System;
using System.Linq;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class PdfTableValueTestForm : Form
{
    private readonly IPdfFirstPageTableValueExtractor _tableExtractor = new PdfFirstPageTableValueExtractor();
    private readonly IPdfFirstPageFieldExtractor _fieldExtractor = new PdfFirstPageFieldExtractor();

    public PdfTableValueTestForm()
    {
        InitializeComponent();
        txtRowLabel.Text = "照护费合计";
        txtColumnLabel.Text = "全额统筹";
    }

    private void btnBrowsePdf_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "PDF 文件 (*.pdf)|*.pdf|所有文件 (*.*)|*.*",
            Title = "选择 PDF 文件",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtPdfPath.Text = dialog.FileName;
            AppendLog($"已选择 PDF：{dialog.FileName}");
        }
    }

    private void btnExtract_Click(object? sender, EventArgs e)
    {
        var pdfPath = txtPdfPath.Text.Trim();
        var rowLabel = txtRowLabel.Text.Trim();
        var columnLabel = txtColumnLabel.Text.Trim();

        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            ShowError("请先选择 PDF 文件。");
            return;
        }

        if (string.IsNullOrWhiteSpace(rowLabel) || string.IsNullOrWhiteSpace(columnLabel))
        {
            ShowError("请填写目标行名和列名。");
            return;
        }

        var result = _tableExtractor.Extract(pdfPath, rowLabel, columnLabel);
        UpdateTableResult(result);
        AppendLog($"表格提取完成。状态={result.Success}，值={result.Value}，消息={result.Message}");
    }

    private void btnExtractFields_Click(object? sender, EventArgs e)
    {
        var pdfPath = txtPdfPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            ShowError("请先选择 PDF 文件。");
            return;
        }

        var fields = new[]
        {
            new PdfFieldDefinition("姓名", ["姓名"], ["性别"]),
            new PdfFieldDefinition("证件号码", ["证件号码"], ["联系人姓名"]),
            new PdfFieldDefinition("个人编号", ["个人编号"], ["结算时间"]),
            new PdfFieldDefinition("结算时间", ["结算时间"], ["一、基本信息"]),
        };

        var result = _fieldExtractor.Extract(pdfPath, fields);
        UpdateFieldResult(result);
        if (string.IsNullOrWhiteSpace(txtFirstPageText.Text))
        {
            txtFirstPageText.Text = result.FirstPageText;
        }

        AppendLog($"基础字段提取完成。状态={result.Success}，消息={result.Message}");
    }

    private void UpdateTableResult(PdfFirstPageTableValueResult result)
    {
        lblStatusValue.Text = result.Success ? "成功" : "失败";
        lblValueValue.Text = string.IsNullOrWhiteSpace(result.Value) ? "-" : result.Value;
        lblMessageValue.Text = result.Message;
        txtHeaderLine.Text = result.HeaderLine;
        txtRowLine.Text = result.RowLine;
        txtJson.Text = result.ToJson();
        txtFirstPageText.Text = result.FirstPageText;
    }

    private void UpdateFieldResult(PdfFirstPageExtractResult result)
    {
        txtFieldJson.Text = result.ToJson();
        txtFieldSummary.Text = BuildFieldSummary(result);
    }

    private static string BuildFieldSummary(PdfFirstPageExtractResult result)
    {
        if (result.Fields.Count == 0)
        {
            return result.Message;
        }

        return string.Join(
            Environment.NewLine,
            result.Fields.Select(static kvp => $"{kvp.Key}: {kvp.Value}"));
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

    private void btnCopyValue_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(lblValueValue.Text);
        AppendLog("提取值已复制到剪贴板。");
    }

    private void btnCopyJson_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtJson.Text);
        AppendLog("表格结果 JSON 已复制到剪贴板。");
    }

    private void btnCopyFieldJson_Click(object? sender, EventArgs e)
    {
        Clipboard.SetText(txtFieldJson.Text);
        AppendLog("基础字段 JSON 已复制到剪贴板。");
    }
}
