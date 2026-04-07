using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Anjian;

public sealed class LongTermCareSettlementPdfExtractor
{
    private static readonly PdfFieldDefinition[] FieldDefinitions =
    [
        new("姓名", ["姓名"], ["性别"]),
        new("证件号码", ["证件号码"], ["联系人姓名"]),
        new("个人编号", ["个人编号"], ["结算时间"]),
        new("结算时间", ["结算时间"], ["一、基本信息"])
    ];

    private readonly IPdfFirstPageFieldExtractor _fieldExtractor;
    private readonly IPdfFirstPageTableValueExtractor _tableValueExtractor;

    public LongTermCareSettlementPdfExtractor()
        : this(new PdfFirstPageFieldExtractor(), new PdfFirstPageTableValueExtractor())
    {
    }

    public LongTermCareSettlementPdfExtractor(
        IPdfFirstPageFieldExtractor fieldExtractor,
        IPdfFirstPageTableValueExtractor tableValueExtractor)
    {
        _fieldExtractor = fieldExtractor ?? throw new ArgumentNullException(nameof(fieldExtractor));
        _tableValueExtractor = tableValueExtractor ?? throw new ArgumentNullException(nameof(tableValueExtractor));
    }

    public LongTermCareSettlementExtractResult Extract(string pdfPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);

        var fileTime = File.Exists(pdfPath)
            ? File.GetLastWriteTime(pdfPath)
            : DateTime.MinValue;

        var fieldResult = _fieldExtractor.Extract(pdfPath, FieldDefinitions);
        var tableResult = _tableValueExtractor.Extract(pdfPath, "照护费合计", "全额统筹");

        fieldResult.Fields.TryGetValue("姓名", out var personName);
        var amount = tableResult.Value ?? string.Empty;

        var success = !string.IsNullOrWhiteSpace(personName) && !string.IsNullOrWhiteSpace(amount);
        var messageParts = new List<string>();

        if (!fieldResult.Success)
        {
            messageParts.Add(fieldResult.Message);
        }
        else if (fieldResult.MissingFields.Count > 0)
        {
            messageParts.Add($"缺失字段: {string.Join(", ", fieldResult.MissingFields)}");
        }

        if (!tableResult.Success)
        {
            messageParts.Add(tableResult.Message);
        }

        if (string.IsNullOrWhiteSpace(personName))
        {
            messageParts.Add("未提取到姓名");
        }

        if (string.IsNullOrWhiteSpace(amount))
        {
            messageParts.Add("未提取到照护费合计/全额统筹");
        }

        var message = messageParts.Count == 0
            ? "提取成功"
            : string.Join("；", messageParts.Distinct(StringComparer.Ordinal));

        return new LongTermCareSettlementExtractResult(
            pdfPath,
            fileTime,
            success,
            personName ?? string.Empty,
            amount,
            message);
    }

    public IReadOnlyList<LongTermCareSettlementExtractResult> ExtractDirectory(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            return
            [
                new LongTermCareSettlementExtractResult(
                    directoryPath,
                    DateTime.MinValue,
                    false,
                    string.Empty,
                    string.Empty,
                    "目录不存在。")
            ];
        }

        return Directory
            .EnumerateFiles(directoryPath, "*.pdf", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderBy(static file => file.LastWriteTime)
            .ThenBy(static file => file.Name, StringComparer.OrdinalIgnoreCase)
            .Select(file => Extract(file.FullName))
            .ToArray();
    }
}
