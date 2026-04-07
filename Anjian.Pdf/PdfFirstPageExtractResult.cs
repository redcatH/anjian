using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Anjian;

public sealed record PdfFirstPageExtractResult(
    string FilePath,
    bool Success,
    IReadOnlyDictionary<string, string> Fields,
    IReadOnlyList<string> MissingFields,
    string FirstPageText,
    string Message)
{
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }
}
