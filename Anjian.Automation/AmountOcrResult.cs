namespace Anjian;

public sealed record AmountOcrResult(
    bool Success,
    string RawText,
    string NormalizedText,
    decimal? Amount,
    long ElapsedMilliseconds,
    string Message);
