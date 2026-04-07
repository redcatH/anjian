using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace Anjian;

public sealed class PaddleSharpTableOcrService : ITableOcrService, IDisposable
{
    private readonly object _syncRoot = new();
    private readonly GeneralOcrRuntimeOptions _runtimeOptions;
    private PaddleOcrAll? _ocr;
    private PaddleOcrTableRecognizer? _tableRecognizer;
    private bool _disposed;

    public PaddleSharpTableOcrService(GeneralOcrRuntimeOptions? runtimeOptions = null)
    {
        _runtimeOptions = runtimeOptions ?? new GeneralOcrRuntimeOptions();
    }

    public Task<TableOcrResult> RecognizeTableAsJsonAsync(Bitmap image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var watch = Stopwatch.StartNew();
        try
        {
            lock (_syncRoot)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                using var mat = BitmapToMat(image);
                var tableResult = GetOrCreateTableRecognizer().Run(mat);
                var ocrResult = GetOrCreateOcrEngine().Run(mat);
                var html = tableResult.RebuildTable(ocrResult);
                var conversion = ConvertHtmlToJson(html);

                watch.Stop();
                return Task.FromResult(new TableOcrResult(
                    true,
                    conversion.Json,
                    html,
                    watch.ElapsedMilliseconds,
                    $"表格识别成功（PaddleSharp，{GetDeviceLabel(_runtimeOptions.Device)}）。{conversion.Message}"));
            }
        }
        catch (Exception ex)
        {
            watch.Stop();
            return Task.FromResult(new TableOcrResult(
                false,
                string.Empty,
                string.Empty,
                watch.ElapsedMilliseconds,
                BuildFriendlyErrorMessage(ex)));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _ocr?.Dispose();
            _ocr = null;
            _tableRecognizer?.Dispose();
            _tableRecognizer = null;
            _disposed = true;
        }
    }

    private PaddleOcrAll GetOrCreateOcrEngine()
    {
        if (_ocr is not null)
        {
            return _ocr;
        }

        _ocr = new PaddleOcrAll(LocalFullModels.ChineseV3, CreateDevice(_runtimeOptions.Device))
        {
            AllowRotateDetection = true,
            Enable180Classification = false,
        };
        _ocr.Detector.UnclipRatio = 1.2f;
        return _ocr;
    }

    private PaddleOcrTableRecognizer GetOrCreateTableRecognizer()
    {
        if (_tableRecognizer is not null)
        {
            return _tableRecognizer;
        }

        _tableRecognizer = new PaddleOcrTableRecognizer(
            LocalTableRecognitionModel.ChineseMobileV2_SLANET,
            CreateDevice(_runtimeOptions.Device));
        return _tableRecognizer;
    }

    private static ConversionResult ConvertHtmlToJson(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return new ConversionResult("[]", "未返回表格 HTML，已输出空数组。");
        }

        var document = XDocument.Parse(html, LoadOptions.PreserveWhitespace);
        var rows = document
            .Descendants()
            .Where(static x => string.Equals(x.Name.LocalName, "tr", StringComparison.OrdinalIgnoreCase))
            .Select(ParseRow)
            .Where(static row => row.Count > 0)
            .ToList();

        if (rows.Count == 0)
        {
            return new ConversionResult("[]", "未识别到有效表格行，已输出空数组。");
        }

        if (rows.Count <= 1)
        {
            return new ConversionResult(
                Serialize(rows),
                "表格行数不足 2 行，已回退为二维数组 JSON。");
        }

        var header = BuildHeader(rows[0]);
        if (header.Count == 0 || header.All(static x => x.StartsWith("Column", StringComparison.Ordinal)))
        {
            return new ConversionResult(
                Serialize(rows),
                "首行无法稳定作为表头，已回退为二维数组 JSON。");
        }

        var objects = new List<Dictionary<string, string?>>();
        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var item = new Dictionary<string, string?>(StringComparer.Ordinal);
            for (var columnIndex = 0; columnIndex < header.Count; columnIndex++)
            {
                var value = columnIndex < row.Count ? row[columnIndex] : string.Empty;
                item[header[columnIndex]] = value;
            }

            if (item.Values.Any(static value => !string.IsNullOrWhiteSpace(value)))
            {
                objects.Add(item);
            }
        }

        return new ConversionResult(
            Serialize(objects),
            $"已输出对象数组 JSON，共 {objects.Count} 行。");
    }

    private static List<string> ParseRow(XElement rowElement)
    {
        return rowElement
            .Elements()
            .Where(static x =>
                string.Equals(x.Name.LocalName, "td", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Name.LocalName, "th", StringComparison.OrdinalIgnoreCase))
            .Select(GetCellText)
            .ToList();
    }

    private static string GetCellText(XElement cell)
    {
        var text = string.Concat(cell.DescendantNodes().OfType<XText>().Select(static x => x.Value));
        return WebUtility.HtmlDecode(text).Trim();
    }

    private static List<string> BuildHeader(IReadOnlyList<string> sourceHeader)
    {
        var header = new List<string>(sourceHeader.Count);
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var index = 0; index < sourceHeader.Count; index++)
        {
            var name = string.IsNullOrWhiteSpace(sourceHeader[index])
                ? $"Column{index + 1}"
                : sourceHeader[index].Trim();

            if (counts.TryGetValue(name, out var count))
            {
                count++;
                counts[name] = count;
                name = $"{name}_{count}";
            }
            else
            {
                counts[name] = 1;
            }

            header.Add(name);
        }

        return header;
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    private static Action<PaddleConfig> CreateDevice(GeneralOcrDeviceType deviceType)
    {
        return deviceType switch
        {
            GeneralOcrDeviceType.CpuMkl => PaddleDevice.Mkldnn(),
            GeneralOcrDeviceType.Gpu => PaddleDevice.Gpu(),
            _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, "Unsupported Paddle device."),
        };
    }

    private static Mat BitmapToMat(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);
    }

    private string BuildFriendlyErrorMessage(Exception ex)
    {
        var root = ex.GetBaseException();
        var message = root.Message;

        if (root is DllNotFoundException)
        {
            return "PaddleSharp 表格识别初始化失败：缺少原生依赖。请确认已安装 Visual C++ Redistributable，并且当前输出目录包含 PaddleInference/OpenCv 运行时文件。";
        }

        if (message.Contains("paddle_inference_c", StringComparison.OrdinalIgnoreCase))
        {
            return "PaddleSharp 表格识别初始化失败：未能加载 paddle_inference_c。请检查运行时包是否已复制到输出目录，并确认机器已安装 Visual C++ Redistributable。";
        }

        if (message.Contains("OpenCvSharpExtern", StringComparison.OrdinalIgnoreCase))
        {
            return "PaddleSharp 表格识别初始化失败：未能加载 OpenCvSharpExtern。请检查 OpenCvSharp4.runtime.win 是否已随程序部署。";
        }

        if (_runtimeOptions.Device == GeneralOcrDeviceType.Gpu)
        {
            return $"PaddleSharp GPU 表格识别失败：{message}。请确认已安装与运行时包匹配的 CUDA/cuDNN/TensorRT，或切回 CPU MKL 模式。";
        }

        return $"PaddleSharp 表格识别失败：{message}";
    }

    private static string GetDeviceLabel(GeneralOcrDeviceType deviceType)
    {
        return deviceType switch
        {
            GeneralOcrDeviceType.CpuMkl => "CPU MKL",
            GeneralOcrDeviceType.Gpu => "GPU",
            _ => deviceType.ToString(),
        };
    }

    private sealed record ConversionResult(string Json, string Message);
}
