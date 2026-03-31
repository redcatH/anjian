using System.Collections.Generic;
using System.Text;

namespace Anjian;

public static class GeneralTextNormalizer
{
    public static GeneralOcrResult Normalize(
        string rawText,
        long elapsedMilliseconds,
        string successMessage = "文本识别成功。",
        IReadOnlyList<GeneralOcrRegion>? regions = null)
    {
        rawText ??= string.Empty;
        var normalized = NormalizeText(rawText);
        return new GeneralOcrResult(
            !string.IsNullOrWhiteSpace(normalized),
            rawText,
            normalized,
            elapsedMilliseconds,
            string.IsNullOrWhiteSpace(normalized) ? "未识别到有效文本。" : successMessage,
            regions);
    }

    public static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                continue;
            }

            builder.Append(ToHalfWidth(ch));
        }

        return builder.ToString();
    }

    private static char ToHalfWidth(char ch)
    {
        return ch switch
        {
            '\u3000' => ' ',
            >= '\uFF01' and <= '\uFF5E' => (char)(ch - 0xFEE0),
            _ => ch
        };
    }
}
