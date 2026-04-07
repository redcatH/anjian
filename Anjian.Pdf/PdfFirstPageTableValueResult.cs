using System.Text.Encodings.Web;
using System.Text.Json;

namespace Anjian;

public sealed record PdfFirstPageTableValueResult(
    string FilePath,
    bool Success,
    string RowLabel,
    string ColumnLabel,
    string Value,
    string HeaderLine,
    string RowLine,
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
