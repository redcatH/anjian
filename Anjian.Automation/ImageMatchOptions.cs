using System.Drawing;

namespace Anjian;

public sealed record ImageMatchOptions(
    Rectangle SearchRegion,
    double Threshold,
    bool UseGrayscale = true,
    int Step = 1,
    ImageMatchMode MatchMode = ImageMatchMode.First,
    ImageScanDirection ScanDirection = ImageScanDirection.TopToBottom);
