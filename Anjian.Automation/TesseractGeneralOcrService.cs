using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Tesseract;

namespace Anjian;

public sealed class TesseractGeneralOcrService : IGeneralOcrService
{
    private static readonly object EngineLock = new();
    private static TesseractEngine? _engine;

    private readonly ImagePreprocessService _imagePreprocess;

    public TesseractGeneralOcrService(ImagePreprocessService imagePreprocess)
    {
        _imagePreprocess = imagePreprocess ?? throw new ArgumentNullException(nameof(imagePreprocess));
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
            using var processed = _imagePreprocess.Preprocess(image, options ?? new GeneralOcrOptions());
            using var pix = PixConverter.ToPix(processed);
            lock (EngineLock)
            {
                var engine = GetEngine();
                using var page = engine.Process(pix, PageSegMode.SingleBlock);
                var text = page.GetText() ?? string.Empty;
                watch.Stop();
                return Task.FromResult(GeneralTextNormalizer.Normalize(
                    text,
                    watch.ElapsedMilliseconds,
                    "文本识别成功（Tesseract）。",
                    Array.Empty<GeneralOcrRegion>()));
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
                $"Tesseract 文本识别失败：{ex.Message}",
                Array.Empty<GeneralOcrRegion>()));
        }
    }

    private static TesseractEngine GetEngine()
    {
        if (_engine is not null)
        {
            return _engine;
        }

        var tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
        if (!Directory.Exists(tessdataPath))
        {
            throw new DirectoryNotFoundException($"未找到 tessdata 目录：{tessdataPath}");
        }

        var chiSimPath = Path.Combine(tessdataPath, "chi_sim.traineddata");
        if (!File.Exists(chiSimPath))
        {
            throw new FileNotFoundException("缺少中文 OCR 训练数据 chi_sim.traineddata。请将该文件放入 tessdata 目录并重新运行。", chiSimPath);
        }

        _engine = new TesseractEngine(tessdataPath, "chi_sim+eng", EngineMode.LstmOnly)
        {
            DefaultPageSegMode = PageSegMode.SingleBlock,
        };
        return _engine;
    }
}
