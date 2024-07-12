using System.Globalization;
using System.Text;

namespace DatabaseService.Utils;

public class SanitizeTitle
{
    public static string Sanitize(string input)
    {
        var normalized = input.Trim();
        normalized = normalized.ToLowerInvariant();
        normalized = RemoveDiacritics(normalized);
        return normalized;
    }

    /// <summary>
    /// Method to remove the variances in diacritics and text
    /// and normalizes the text to a basis form for reduced-size
    /// trie searching.
    /// </summary>
    /// <param name="input">The input to sanitize.</param>
    /// <returns>The basic alphanumeric title.</returns>
    private static string RemoveDiacritics(string input)
    {
        var sanitized = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in sanitized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var lowerString = stringBuilder.ToString().ToLower();
        return lowerString.Normalize(NormalizationForm.FormC);
    }
}