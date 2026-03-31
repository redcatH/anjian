namespace Anjian;

public sealed record GeneralOcrOptions(
    bool UseGrayscale = true,
    bool UseBinarization = false,
    int BinarizationThreshold = 170,
    bool Scale2x = true);
