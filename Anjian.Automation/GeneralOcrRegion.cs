using System.Drawing;

namespace Anjian;

public sealed record GeneralOcrRegion(
    string Text,
    string NormalizedText,
    float Score,
    Rectangle Bounds,
    GeneralOcrPoint Center);
