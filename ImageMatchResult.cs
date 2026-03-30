using System.Collections.Generic;
using System.Drawing;

namespace Anjian;

public sealed record ImageMatchHit(
    Point Location,
    double Score);

public sealed record ImageMatchResult(
    bool Success,
    Point? Location,
    double Score,
    long ElapsedMilliseconds,
    string Message,
    IReadOnlyList<ImageMatchHit> Hits);
