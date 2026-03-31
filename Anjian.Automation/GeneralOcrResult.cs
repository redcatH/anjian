using System;
using System.Collections.Generic;

namespace Anjian;

public sealed record GeneralOcrResult(
    bool Success,
    string RawText,
    string NormalizedText,
    long ElapsedMilliseconds,
    string Message,
    IReadOnlyList<GeneralOcrRegion>? Regions = null)
{
    public IReadOnlyList<GeneralOcrRegion> SafeRegions => Regions ?? Array.Empty<GeneralOcrRegion>();
}
