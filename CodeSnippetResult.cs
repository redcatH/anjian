namespace Anjian;

public sealed record CodeSnippetResult(
    string Title,
    string Code,
    string? Description = null);
