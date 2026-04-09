using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Anjian;

public sealed class PdfFirstPageFieldExtractor : IPdfFirstPageFieldExtractor
{
    private const double LineTolerance = 3.5d;

    public PdfFirstPageExtractResult Extract(string pdfPath, IReadOnlyList<PdfFieldDefinition> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);
        ArgumentNullException.ThrowIfNull(fields);

        if (!File.Exists(pdfPath))
        {
            return new PdfFirstPageExtractResult(
                pdfPath,
                false,
                new Dictionary<string, string>(StringComparer.Ordinal),
                fields.Select(static x => x.FieldName).ToArray(),
                string.Empty,
                "PDF 文件不存在。");
        }

        if (fields.Count == 0)
        {
            return new PdfFirstPageExtractResult(
                pdfPath,
                false,
                new Dictionary<string, string>(StringComparer.Ordinal),
                Array.Empty<string>(),
                string.Empty,
                "未提供字段定义。");
        }

        try
        {
            using var document = PdfDocument.Open(pdfPath);
            if (document.NumberOfPages <= 0)
            {
                return new PdfFirstPageExtractResult(
                    pdfPath,
                    false,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    fields.Select(static x => x.FieldName).ToArray(),
                    string.Empty,
                    "PDF 没有任何页面。");
            }

            var page = document.GetPage(1);
            var firstPageText = page.Text ?? string.Empty;
            var lines = BuildLines(page.GetWords());
            var extractedFields = ExtractFieldsFromLines(lines, fields);
            var missingFields = fields
                .Select(static x => x.FieldName)
                .Where(fieldName => !extractedFields.ContainsKey(fieldName))
                .ToArray();

            var success = extractedFields.Count > 0;
            var message = missingFields.Length == 0
                ? "首页字段提取成功。"
                : $"首页字段提取完成，缺失字段：{string.Join(", ", missingFields)}";

            return new PdfFirstPageExtractResult(
                pdfPath,
                success,
                extractedFields,
                missingFields,
                firstPageText,
                message);
        }
        catch (Exception ex)
        {
            return new PdfFirstPageExtractResult(
                pdfPath,
                false,
                new Dictionary<string, string>(StringComparer.Ordinal),
                fields.Select(static x => x.FieldName).ToArray(),
                string.Empty,
                $"首页字段提取失败：{ex.GetBaseException().Message}");
        }
    }

    public IReadOnlyList<PdfFirstPageExtractResult> ExtractDirectory(string directoryPath, IReadOnlyList<PdfFieldDefinition> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentNullException.ThrowIfNull(fields);

        if (!Directory.Exists(directoryPath))
        {
            return new[]
            {
                new PdfFirstPageExtractResult(
                    directoryPath,
                    false,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    fields.Select(static x => x.FieldName).ToArray(),
                    string.Empty,
                    "目录不存在。")
            };
        }

        return Directory
            .EnumerateFiles(directoryPath, "*.pdf", SearchOption.TopDirectoryOnly)
            .OrderBy(static x => x, StringComparer.OrdinalIgnoreCase)
            .Select(pdfPath => Extract(pdfPath, fields))
            .ToArray();
    }

    private static Dictionary<string, string> ExtractFieldsFromLines(
        IReadOnlyList<PdfWordLine> lines,
        IReadOnlyList<PdfFieldDefinition> fields)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var allLabels = fields
            .SelectMany(static x => x.Labels.Concat(x.StopLabels))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        foreach (var field in fields)
        {
            var value = TryExtractFieldValue(lines, field, allLabels);
            if (!string.IsNullOrWhiteSpace(value))
            {
                result[field.FieldName] = value;
            }
        }

        return result;
    }

    private static string? TryExtractFieldValue(
        IReadOnlyList<PdfWordLine> lines,
        PdfFieldDefinition field,
        IReadOnlyList<string> allLabels)
    {
        foreach (var line in lines)
        {
            foreach (var label in field.Labels)
            {
                var labelSpan = FindSpan(line.Words, label);
                if (labelSpan is null)
                {
                    continue;
                }

                var inlineValue = ExtractInlineValue(line.Words, labelSpan.Value, label, field, allLabels);
                if (!string.IsNullOrWhiteSpace(inlineValue))
                {
                    return inlineValue;
                }
            }
        }

        return null;
    }

    private static string? ExtractInlineValue(
        IReadOnlyList<PdfWordBox> words,
        PdfSpan labelSpan,
        string currentLabel,
        PdfFieldDefinition field,
        IReadOnlyList<string> allLabels)
    {
        var rightSideWords = words
            .Where(word => word.Left > labelSpan.Right)
            .OrderBy(static word => word.Left)
            .ToArray();

        if (rightSideWords.Length == 0)
        {
            return null;
        }

        double? nextLabelLeft = null;
        var stopLabels = field.StopLabels.Count > 0 ? field.StopLabels : allLabels;
        foreach (var label in stopLabels)
        {
            if (string.Equals(label, currentLabel, StringComparison.Ordinal))
            {
                continue;
            }

            var span = FindSpan(rightSideWords, label);
            if (span is null)
            {
                continue;
            }

            if (nextLabelLeft is null || span.Value.Left < nextLabelLeft.Value)
            {
                nextLabelLeft = span.Value.Left;
            }
        }

        var valueWords = rightSideWords
            .Where(word => nextLabelLeft is null || word.Right <= nextLabelLeft.Value)
            .Select(static word => word.Text)
            .Where(static text => !string.IsNullOrWhiteSpace(text))
            .ToArray();

        if (valueWords.Length == 0)
        {
            return null;
        }

        var value = string.Concat(valueWords);
        value = value.Trim().Trim(':', '：');
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static PdfSpan? FindSpan(IReadOnlyList<PdfWordBox> words, string targetText)
    {
        var normalizedTarget = NormalizeForMatch(targetText);
        for (var start = 0; start < words.Count; start++)
        {
            var merged = string.Empty;
            for (var end = start; end < words.Count; end++)
            {
                merged += NormalizeForMatch(words[end].Text);
                if (string.IsNullOrEmpty(merged))
                {
                    continue;
                }

                if (string.Equals(merged, normalizedTarget, StringComparison.Ordinal))
                {
                    return new PdfSpan(words[start].Left, words[end].Right);
                }

                if (!normalizedTarget.StartsWith(merged, StringComparison.Ordinal) ||
                    merged.Length >= normalizedTarget.Length)
                {
                    break;
                }
            }
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

        return lines.OrderByDescending(static x => x.CenterY).ToArray();
    }

    private static string NormalizeText(string text)
    {
        return string.Concat(text.Where(static ch => !char.IsWhiteSpace(ch)));
    }

    private static string NormalizeForMatch(string text)
    {
        return NormalizeText(text).Trim(':', '：');
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
    }

    private sealed record PdfWordBox(string Text, double Left, double Right, double CenterY);

    private readonly record struct PdfSpan(double Left, double Right);
}
