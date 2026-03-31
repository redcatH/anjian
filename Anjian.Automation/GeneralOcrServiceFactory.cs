using System;

namespace Anjian;

public static class GeneralOcrServiceFactory
{
    public static IGeneralOcrService Create(
        ImagePreprocessService imagePreprocess,
        GeneralOcrRuntimeOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(imagePreprocess);

        var actualOptions = options ?? new GeneralOcrRuntimeOptions();
        return actualOptions.Provider switch
        {
            OcrEngineType.PaddleSharp => new PaddleSharpGeneralOcrService(actualOptions),
            OcrEngineType.Tesseract => new TesseractGeneralOcrService(imagePreprocess),
            _ => throw new ArgumentOutOfRangeException(nameof(options), actualOptions.Provider, "Unsupported general OCR provider."),
        };
    }
}
