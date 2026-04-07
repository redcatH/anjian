using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Anjian;

public sealed record LongTermCareSettlementExtractResult(
    string FilePath,
    DateTime FileTime,
    bool Success,
    string PersonName,
    string FullAmountOverall,
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
