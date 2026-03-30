using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Anjian;

public static partial class AmountTextNormalizer
{
    public static AmountOcrResult Normalize(string rawText, long elapsedMilliseconds, string successMessage = "金额识别成功。")
    {
        rawText ??= string.Empty;

        var cleanedText = rawText
            .Replace(" ", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);

        var matches = AmountPattern().Matches(cleanedText);
        if (matches.Count == 0)
        {
            return new AmountOcrResult(false, rawText, string.Empty, null, elapsedMilliseconds, "未提取到金额格式文本。");
        }

        var candidate = matches
            .Select(static match => match.Value)
            .OrderByDescending(static text => text.Length)
            .First();

        var normalizedText = candidate.Replace(",", string.Empty);
        if (!decimal.TryParse(normalizedText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return new AmountOcrResult(false, rawText, normalizedText, null, elapsedMilliseconds, "金额文本无法解析为数字。");
        }

        return new AmountOcrResult(true, rawText, normalizedText, amount, elapsedMilliseconds, successMessage);
    }

    [GeneratedRegex(@"\d[\d,]*\.?\d*")]
    private static partial Regex AmountPattern();
}
