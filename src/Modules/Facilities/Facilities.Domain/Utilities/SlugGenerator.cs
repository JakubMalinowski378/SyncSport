using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Facilities.Domain.Utilities;

public static partial class SlugGenerator
{
    private static readonly Regex InvalidCharsRegex = InvalidChars();
    private static readonly Regex MultiDashRegex = MultiDash();

    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Slug source value cannot be empty.");
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(character);
            }
        }

        var withoutDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);
        var slug = InvalidCharsRegex.Replace(withoutDiacritics, "-");
        slug = MultiDashRegex.Replace(slug, "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex InvalidChars();

    [GeneratedRegex("-+", RegexOptions.Compiled)]
    private static partial Regex MultiDash();
}
