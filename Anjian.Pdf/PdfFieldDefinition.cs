using System;
using System.Collections.Generic;
using System.Linq;

namespace Anjian;

public sealed record PdfFieldDefinition
{
    public PdfFieldDefinition(
        string fieldName,
        IReadOnlyList<string> labels,
        IReadOnlyList<string>? stopLabels = null)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        }

        if (labels is null || labels.Count == 0 || labels.All(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one non-empty label is required.", nameof(labels));
        }

        FieldName = fieldName.Trim();
        Labels = NormalizeList(labels);
        StopLabels = stopLabels is null ? Array.Empty<string>() : NormalizeList(stopLabels);
    }

    public string FieldName { get; }

    public IReadOnlyList<string> Labels { get; }

    public IReadOnlyList<string> StopLabels { get; }

    private static IReadOnlyList<string> NormalizeList(IReadOnlyList<string> source)
    {
        return source
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
