namespace Anjian;

public sealed record AmountOcrOptions(
    bool UseGrayscale = true,
    bool UseBinarization = false,
    int BinarizationThreshold = 160,
    bool Scale2x = false);
