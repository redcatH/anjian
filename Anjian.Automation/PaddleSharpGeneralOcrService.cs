using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace Anjian;

public sealed class PaddleSharpGeneralOcrService : IGeneralOcrService, IDisposable
{
    private readonly object _syncRoot = new();
    private readonly GeneralOcrRuntimeOptions _runtimeOptions;
    private PaddleOcrAll? _ocr;
    private bool _disposed;

    public PaddleSharpGeneralOcrService(GeneralOcrRuntimeOptions? runtimeOptions = null)
    {
        _runtimeOptions = runtimeOptions ?? new GeneralOcrRuntimeOptions();
    }

    public async Task<GeneralOcrResult> RecognizeTextAsync(Bitmap image, GeneralOcrOptions? options = null)
    {
        return await RecognizeRegionsAsync(image, options);
    }

    public Task<GeneralOcrResult> RecognizeRegionsAsync(Bitmap image, GeneralOcrOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(image);

        var watch = Stopwatch.StartNew();
        try
        {
            lock (_syncRoot)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                var ocr = GetOrCreateEngine();
                using var mat = BitmapToMat(image);
                var result = ocr.Run(mat);
                var regions = MapRegions(result).ToArray();
                var rawText = result.Text ?? string.Join(Environment.NewLine, regions.Select(static x => x.Text));

                watch.Stop();
                return Task.FromResult(GeneralTextNormalizer.Normalize(
                    rawText,
                    watch.ElapsedMilliseconds,
                    $"文本识别成功（PaddleSharp，{GetDeviceLabel(_runtimeOptions.Device)}）。",
                    regions));
            }
        }
        catch (Exception ex)
        {
            watch.Stop();
            return Task.FromResult(new GeneralOcrResult(
                false,
                string.Empty,
                string.Empty,
                watch.ElapsedMilliseconds,
                BuildFriendlyErrorMessage(ex),
                Array.Empty<GeneralOcrRegion>()));
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
            _disposed = true;
        }
    }

    private PaddleOcrAll GetOrCreateEngine()
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

        return _ocr;
    }

    private static IEnumerable<GeneralOcrRegion> MapRegions(PaddleOcrResult result)
    {
        foreach (var region in result.Regions ?? Array.Empty<PaddleOcrResultRegion>())
        {
            var bounds = region.Rect.BoundingRect();
            yield return new GeneralOcrRegion(
                region.Text ?? string.Empty,
                GeneralTextNormalizer.NormalizeText(region.Text ?? string.Empty),
                region.Score,
                new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height),
                new GeneralOcrPoint(region.Rect.Center.X, region.Rect.Center.Y));
        }
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
            return "PaddleSharp 初始化失败：缺少原生依赖。请确认已安装 Visual C++ Redistributable，并且当前输出目录包含 PaddleInference/OpenCv 运行时文件。";
        }

        if (message.Contains("paddle_inference_c", StringComparison.OrdinalIgnoreCase))
        {
            return "PaddleSharp 初始化失败：未能加载 paddle_inference_c。请检查运行时包是否已复制到输出目录，并确认机器已安装 Visual C++ Redistributable。";
        }

        if (message.Contains("OpenCvSharpExtern", StringComparison.OrdinalIgnoreCase))
        {
            return "PaddleSharp 初始化失败：未能加载 OpenCvSharpExtern。请检查 OpenCvSharp4.runtime.win 是否已随程序部署。";
        }

        if (_runtimeOptions.Device == GeneralOcrDeviceType.Gpu)
        {
            return $"PaddleSharp GPU 文本识别失败：{message}。请确认已安装与运行时包匹配的 CUDA/cuDNN/TensorRT，或切回 CPU MKL 模式。";
        }

        return $"PaddleSharp 文本识别失败：{message}";
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
}
