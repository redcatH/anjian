using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Anjian;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length == 0)
        {
            ShowUsage();
            return 1;
        }

        var inputDirectory = args[0];
        if (!Directory.Exists(inputDirectory))
        {
            Console.WriteLine($"目录不存在: {inputDirectory}");
            return 1;
        }

        var outputCsvPath = args.Length >= 2
            ? args[1]
            : Path.Combine(
                inputDirectory,
                $"pdf-export-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

        var extractor = new LongTermCareSettlementPdfExtractor();
        var results = extractor.ExtractDirectory(inputDirectory);
        if (results.Count == 0)
        {
            Console.WriteLine("未找到任何 PDF 文件。");
            return 1;
        }

        WriteCsv(outputCsvPath, results);

        var successCount = results.Count(static x => x.Success);
        Console.WriteLine($"处理完成，共 {results.Count} 个 PDF，成功 {successCount} 个，失败 {results.Count - successCount} 个。");
        Console.WriteLine($"CSV 已生成: {outputCsvPath}");
        return 0;
    }

    private static void WriteCsv(string outputCsvPath, IReadOnlyList<LongTermCareSettlementExtractResult> results)
    {
        var outputDirectory = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var lines = new List<string>
        {
            "文件时间,文件名,姓名,全额统筹,是否成功,消息,文件路径"
        };

        lines.AddRange(results.Select(static result => string.Join(",",
            EscapeCsv(result.FileTime == DateTime.MinValue ? string.Empty : result.FileTime.ToString("yyyy-MM-dd HH:mm:ss")),
            EscapeCsv(Path.GetFileName(result.FilePath)),
            EscapeCsv(result.PersonName),
            EscapeCsv(result.FullAmountOverall),
            EscapeCsv(result.Success ? "是" : "否"),
            EscapeCsv(result.Message),
            EscapeCsv(result.FilePath))));

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string EscapeCsv(string? value)
    {
        var text = value ?? string.Empty;
        if (!text.Contains('"') && !text.Contains(',') && !text.Contains('\r') && !text.Contains('\n'))
        {
            return text;
        }

        return $"\"{text.Replace("\"", "\"\"")}\"";
    }

    private static void ShowUsage()
    {
        Console.WriteLine("用法:");
        Console.WriteLine(@"  Anjian.PdfExporter <pdf目录> [输出csv路径]");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine(@"  Anjian.PdfExporter D:\pdfs");
        Console.WriteLine(@"  Anjian.PdfExporter D:\pdfs D:\output\result.csv");
        Console.WriteLine();
        Console.WriteLine("默认按 PDF 文件最后修改时间升序排序。");
    }
}
