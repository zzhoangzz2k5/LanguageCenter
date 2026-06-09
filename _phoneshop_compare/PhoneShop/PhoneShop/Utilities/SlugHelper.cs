using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PhoneShop.Utilities;

public static partial class SlugHelper
{
    public static string Generate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "product";
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = NonAlphaNumericRegex()
            .Replace(builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(), "-")
            .Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "product" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();
}
