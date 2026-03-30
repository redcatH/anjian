using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Tesseract;

namespace Anjian;

public sealed class TesseractAmountOcrService : IOcrService
{
    private static readonly object EngineLock = new();
    private static TesseractEngine? _engine;

    public Task<AmountOcrResult> RecognizeAmountAsync(Bitmap image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var watch = Stopwatch.StartNew();
        try
        {
            using var pix = PixConverter.ToPix(image);
            lock (EngineLock)
            {
                var engine = GetEngine();
                using var page = engine.Process(pix, PageSegMode.SingleLine);
                var text = page.GetText() ?? string.Empty;
                watch.Stop();
                return Task.FromResult(AmountTextNormalizer.Normalize(text, watch.ElapsedMilliseconds, "金额识别成功（Tesseract）。"));
            }
        }
        catch (Exception ex)
        {
            watch.Stop();
            return Task.FromResult(new AmountOcrResult(false, string.Empty, string.Empty, null, watch.ElapsedMilliseconds, $"Tesseract 金额识别失败：{ex.Message}"));
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

        _engine = new TesseractEngine(tessdataPath, "eng", EngineMode.LstmOnly);
        _engine.SetVariable("tessedit_char_whitelist", "0123456789.,");
        _engine.DefaultPageSegMode = PageSegMode.SingleLine;
        return _engine;
    }
}
