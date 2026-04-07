namespace Anjian;

public sealed record TableOcrResult(
    bool Success,
    string Json,
    string Html,
    long ElapsedMilliseconds,
    string Message);
