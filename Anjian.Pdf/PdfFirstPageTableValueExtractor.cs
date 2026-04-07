using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Anjian;

public sealed class PdfFirstPageTableValueExtractor : IPdfFirstPageTableValueExtractor
{
    private const double LineTolerance = 3.5d;

    public PdfFirstPageTableValueResult Extract(string pdfPath, string rowLabel, string columnLabel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(rowLabel);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnLabel);

        if (!File.Exists(pdfPath))
        {
            return new PdfFirstPageTableValueResult(
                pdfPath,
                false,
                rowLabel,
                columnLabel,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                "PDF 文件不存在。");
        }

        try
        {
            using var document = PdfDocument.Open(pdfPath);
            if (document.NumberOfPages <= 0)
            {
                return new PdfFirstPageTableValueResult(
                    pdfPath,
                    false,
                    rowLabel,
                    columnLabel,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "PDF 没有任何页面。");
            }

            var page = document.GetPage(1);
            var firstPageText = page.Text ?? string.Empty;
            var lines = BuildLines(page.GetWords());
            var headerLine = FindHeaderLine(lines, columnLabel);
            if (headerLine is null)
            {
                return new PdfFirstPageTableValueResult(
                    pdfPath,
                    false,
                    rowLabel,
                    columnLabel,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    firstPageText,
                    $"未找到列标题：{columnLabel}");
            }

            var columnCenter = FindSpanCenterX(headerLine.Words, columnLabel);
            if (columnCenter is null)
            {
                return new PdfFirstPageTableValueResult(
                    pdfPath,
                    false,
                    rowLabel,
                    columnLabel,
                    string.Empty,
                    headerLine.Text,
                    string.Empty,
                    firstPageText,
                    $"已找到表头行，但未能定位列：{columnLabel}");
            }

            var rowLine = FindRowLine(lines, headerLine.CenterY, rowLabel);
            if (rowLine is null)
            {
                return new PdfFirstPageTableValueResult(
                    pdfPath,
                    false,
                    rowLabel,
                    columnLabel,
                    string.Empty,
                    headerLine.Text,
                    string.Empty,
                    firstPageText,
                    $"未找到目标行：{rowLabel}");
            }

            var orderedValue = TryExtractByColumnOrder(headerLine.Text, rowLine.Text, rowLabel, columnLabel);
            if (!string.IsNullOrWhiteSpace(orderedValue))
            {
                return new PdfFirstPageTableValueResult(
                    pdfPath,
                    true,
                    rowLabel,
                    columnLabel,
                    orderedValue,
                    headerLine.Text,
                    rowLine.Text,
                    firstPageText,
                    "首页表格取值成功。");
            }

            var columnWindow = BuildColumnWindow(headerLine.Words, columnLabel, columnCenter.Value);
            var rowLabelRight = FindSpanRight(rowLine.Words, rowLabel);
            var value = FindCellValue(rowLine.Words, columnWindow, rowLabelRight);
            if (string.IsNullOrWhiteSpace(value))
            {
                return new PdfFirstPageTableValueResult(
                    pdfPath,
                    false,
                    rowLabel,
                    columnLabel,
                    string.Empty,
                    headerLine.Text,
                    rowLine.Text,
                    firstPageText,
                    "已找到目标行和列，但未能提取交叉单元格值。");
            }

            return new PdfFirstPageTableValueResult(
                pdfPath,
                true,
                rowLabel,
                columnLabel,
                value,
                headerLine.Text,
                rowLine.Text,
                firstPageText,
                "首页表格取值成功。");
        }
        catch (Exception ex)
        {
            return new PdfFirstPageTableValueResult(
                pdfPath,
                false,
                rowLabel,
                columnLabel,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                $"首页表格取值失败：{ex.GetBaseException().Message}");
        }
    }

    private static string? TryExtractByColumnOrder(string headerLineText, string rowLineText, string rowLabel, string columnLabel)
    {
        var headerTokens = SplitTokens(headerLineText);
        var rowTokens = SplitTokens(rowLineText);
        if (headerTokens.Count < 2 || rowTokens.Count < 2)
        {
            return null;
        }

        var headerIndex = headerTokens.FindIndex(token => string.Equals(token, columnLabel, StringComparison.Ordinal));
        if (headerIndex <= 0)
        {
            return null;
        }

        var rowLabelIndex = rowTokens.FindIndex(token => string.Equals(token, rowLabel, StringComparison.Ordinal));
        if (rowLabelIndex < 0)
        {
            return null;
        }

        var valueIndex = rowLabelIndex + headerIndex;
        if (valueIndex < rowTokens.Count)
        {
            return rowTokens[valueIndex];
        }

        return null;
    }

    private static IReadOnlyList<PdfWordLine> BuildLines(IEnumerable<Word> words)
    {
        var lines = new List<PdfWordLine>();
        foreach (var word in words
                     .Where(static x => !string.IsNullOrWhiteSpace(x.Text))
                     .OrderByDescending(GetCenterY)
                     .ThenBy(static x => x.BoundingBox.Left))
        {
            var centerY = GetCenterY(word);
            var existingLine = lines.FirstOrDefault(line => Math.Abs(line.CenterY - centerY) <= LineTolerance);
            if (existingLine is null)
            {
                existingLine = new PdfWordLine(centerY);
                lines.Add(existingLine);
            }

            existingLine.Words.Add(new PdfWordBox(
                word.Text.Trim(),
                (double)word.BoundingBox.Left,
                (double)word.BoundingBox.Right,
                centerY));
        }

        foreach (var line in lines)
        {
            line.Words.Sort(static (a, b) => a.Left.CompareTo(b.Left));
        }

        return lines
            .OrderByDescending(static x => x.CenterY)
            .ToArray();
    }

    private static PdfWordLine? FindHeaderLine(IReadOnlyList<PdfWordLine> lines, string columnLabel)
    {
        var normalizedColumnLabel = NormalizeText(columnLabel);
        return lines.FirstOrDefault(line =>
            NormalizeText(line.Text).Contains(normalizedColumnLabel, StringComparison.Ordinal) &&
            NormalizeText(line.Text).Contains(NormalizeText("项目名称"), StringComparison.Ordinal));
    }

    private static PdfWordLine? FindRowLine(IReadOnlyList<PdfWordLine> lines, double headerCenterY, string rowLabel)
    {
        var normalizedRowLabel = NormalizeText(rowLabel);
        return lines.FirstOrDefault(line =>
            line.CenterY < headerCenterY &&
            NormalizeText(line.Text).Contains(normalizedRowLabel, StringComparison.Ordinal));
    }

    private static double? FindSpanCenterX(IReadOnlyList<PdfWordBox> words, string targetText)
    {
        var span = FindSpan(words, targetText);
        return span is null ? null : (span.Left + span.Right) / 2d;
    }

    private static double FindSpanRight(IReadOnlyList<PdfWordBox> words, string targetText)
    {
        var span = FindSpan(words, targetText);
        return span?.Right ?? double.MinValue;
    }

    private static PdfSpan? FindSpan(IReadOnlyList<PdfWordBox> words, string targetText)
    {
        var normalizedTarget = NormalizeText(targetText);
        for (var start = 0; start < words.Count; start++)
        {
            var merged = string.Empty;
            for (var end = start; end < words.Count; end++)
            {
                merged += NormalizeText(words[end].Text);
                if (!normalizedTarget.StartsWith(merged, StringComparison.Ordinal) &&
                    !merged.Contains(normalizedTarget, StringComparison.Ordinal))
                {
                    if (merged.Length > normalizedTarget.Length)
                    {
                        break;
                    }
                }

                if (merged.Contains(normalizedTarget, StringComparison.Ordinal))
                {
                    return new PdfSpan(words[start].Left, words[end].Right);
                }
            }
        }

        return null;
    }

    private static ColumnWindow BuildColumnWindow(IReadOnlyList<PdfWordBox> headerWords, string columnLabel, double targetCenterX)
    {
        var span = FindSpan(headerWords, columnLabel);
        if (span is null)
        {
            return new ColumnWindow(targetCenterX - 20d, targetCenterX + 20d, targetCenterX);
        }

        var leftNeighbor = headerWords
            .Where(word => word.Right <= span.Left)
            .OrderByDescending(static word => word.CenterX)
            .FirstOrDefault();

        var rightNeighbor = headerWords
            .Where(word => word.Left >= span.Right)
            .OrderBy(static word => word.CenterX)
            .FirstOrDefault();

        var leftBoundary = leftNeighbor is null
            ? span.Left - 20d
            : (leftNeighbor.CenterX + targetCenterX) / 2d;
        var rightBoundary = rightNeighbor is null
            ? span.Right + 20d
            : (rightNeighbor.CenterX + targetCenterX) / 2d;

        return new ColumnWindow(leftBoundary, rightBoundary, targetCenterX);
    }

    private static string FindCellValue(IReadOnlyList<PdfWordBox> words, ColumnWindow columnWindow, double minLeft)
    {
        var candidates = words
            .Where(word =>
                word.Left >= minLeft - 2d &&
                word.CenterX >= columnWindow.Left &&
                word.CenterX <= columnWindow.Right)
            .Select(word => new
            {
                Word = word,
                Distance = Math.Abs(word.CenterX - columnWindow.CenterX),
                IsNumeric = IsValueLike(word.Text),
            })
            .OrderBy(static x => x.Distance)
            .ThenByDescending(static x => x.IsNumeric)
            .ThenBy(static x => x.Word.Left)
            .ToArray();

        var bestNumeric = candidates.FirstOrDefault(static x => x.IsNumeric);
        if (bestNumeric is not null && bestNumeric.Distance <= 40d)
        {
            return bestNumeric.Word.Text;
        }

        var best = candidates.FirstOrDefault();
        return best?.Distance <= 40d ? best.Word.Text : string.Empty;
    }

    private static bool IsValueLike(string text)
    {
        return text.All(static ch => char.IsDigit(ch) || ch is '.' or '-' or ',' or '%');
    }

    private static List<string> SplitTokens(string text)
    {
        return text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .ToList();
    }

    private static string NormalizeText(string text)
    {
        return string.Concat(text.Where(static ch => !char.IsWhiteSpace(ch)));
    }

    private static double GetCenterY(Word word)
    {
        return ((double)word.BoundingBox.Top + (double)word.BoundingBox.Bottom) / 2d;
    }

    private sealed class PdfWordLine
    {
        public PdfWordLine(double centerY)
        {
            CenterY = centerY;
        }

        public double CenterY { get; }

        public List<PdfWordBox> Words { get; } = new();

        public string Text => string.Join(" ", Words.Select(static x => x.Text));
    }

    private sealed record PdfWordBox(string Text, double Left, double Right, double CenterY)
    {
        public double CenterX => (Left + Right) / 2d;
    }

    private sealed record PdfSpan(double Left, double Right);

    private sealed record ColumnWindow(double Left, double Right, double CenterX);
}
