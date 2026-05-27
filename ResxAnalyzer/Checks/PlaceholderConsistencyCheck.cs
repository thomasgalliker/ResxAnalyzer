using System.Text;
using System.Text.RegularExpressions;

namespace Superdev.ResxAnalyzer.Checks
{
    /// <summary>
    /// Checks that translated values use the same placeholder tokens as the neutral resource.
    /// </summary>
    public sealed class PlaceholderConsistencyCheck : IResxCheck
    {
        private static readonly Regex NumericPlaceholderRegex = new(@"^[0-9]+(?::[^}]*)?$", RegexOptions.Compiled);
        private static readonly Regex NamedPlaceholderRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*(?::[^}]*)?$", RegexOptions.Compiled);

        /// <inheritdoc />
        public string Description => "Checks that translated values use the same placeholder tokens as the neutral resource.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var malformedPlaceholders = new List<string>();
            var referencePlaceholders = context.NeutralResource.Entries
                .Where(entry => !context.InvariantKeys.Contains(entry.Key))
                .ToDictionary(
                    entry => entry.Key,
                    entry => GetPlaceholderAnalysis(context.NeutralResource.CultureName ?? "neutral", entry.Key, entry.Value.Value, malformedPlaceholders),
                    StringComparer.Ordinal);
            var mismatchesByCulture = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var localizedResource in context.LocalizedResources)
            {
                var cultureName = localizedResource.CultureName ?? string.Empty;
                foreach (var referenceEntry in referencePlaceholders)
                {
                    if (!localizedResource.Entries.TryGetValue(referenceEntry.Key, out var localizedEntry))
                    {
                        continue;
                    }

                    var localizedPlaceholders = GetPlaceholderAnalysis(cultureName, referenceEntry.Key, localizedEntry.Value, malformedPlaceholders);
                    if (!referenceEntry.Value.Placeholders.SequenceEqual(localizedPlaceholders.Placeholders, StringComparer.Ordinal))
                    {
                        if (!mismatchesByCulture.TryGetValue(cultureName, out var mismatches))
                        {
                            mismatches = [];
                            mismatchesByCulture.Add(cultureName, mismatches);
                        }

                        mismatches.Add($"{referenceEntry.Key} (neutral: {FormatPlaceholders(referenceEntry.Value.Placeholders)}, localized: {FormatPlaceholders(localizedPlaceholders.Placeholders)})");
                    }
                }
            }

            if (mismatchesByCulture.Count == 0 && malformedPlaceholders.Count == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            if (mismatchesByCulture.Count > 0)
            {
                stringBuilder.AppendLine("Following translation keys have inconsistent placeholders:");
                foreach (var cultureWithKeys in mismatchesByCulture.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    var keys = cultureWithKeys.Value.OrderBy(key => key, StringComparer.Ordinal).ToArray();
                    stringBuilder.AppendLine($"CultureInfo \"{cultureWithKeys.Key}\" ({keys.Length}):");
                    foreach (var key in keys)
                    {
                        stringBuilder.AppendLine($"> {key}");
                    }

                    stringBuilder.AppendLine();
                }
            }

            if (malformedPlaceholders.Count > 0)
            {
                stringBuilder.AppendLine("Following translation values contain malformed placeholders:");
                foreach (var malformedPlaceholder in malformedPlaceholders.OrderBy(value => value, StringComparer.Ordinal))
                {
                    stringBuilder.AppendLine($"> {malformedPlaceholder}");
                }

                stringBuilder.AppendLine();
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private static PlaceholderAnalysis GetPlaceholderAnalysis(string cultureName, string key, string value, ICollection<string> malformedPlaceholders)
        {
            var placeholders = new List<string>();
            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] == '{')
                {
                    if (index + 1 < value.Length && value[index + 1] == '{')
                    {
                        index++;
                        continue;
                    }

                    var closeIndex = value.IndexOf('}', index + 1);
                    if (closeIndex < 0)
                    {
                        malformedPlaceholders.Add($"CultureInfo \"{cultureName}\", Key='{key}', Placeholder='{value.Substring(index)}'");
                        break;
                    }

                    var placeholder = value.Substring(index, closeIndex - index + 1);
                    var content = placeholder.Substring(1, placeholder.Length - 2);
                    if (IsValidPlaceholderContent(content))
                    {
                        placeholders.Add(placeholder);
                    }
                    else
                    {
                        malformedPlaceholders.Add($"CultureInfo \"{cultureName}\", Key='{key}', Placeholder='{placeholder}'");
                    }

                    index = closeIndex;
                    continue;
                }

                if (value[index] == '}')
                {
                    if (index + 1 < value.Length && value[index + 1] == '}')
                    {
                        index++;
                        continue;
                    }

                    malformedPlaceholders.Add($"CultureInfo \"{cultureName}\", Key='{key}', Placeholder='}}'");
                }
            }

            return new PlaceholderAnalysis(placeholders
                .OrderBy(placeholder => placeholder, StringComparer.Ordinal)
                .ToArray());
        }

        private static bool IsValidPlaceholderContent(string content)
        {
            return NumericPlaceholderRegex.IsMatch(content) ||
                   NamedPlaceholderRegex.IsMatch(content);
        }

        private static string FormatPlaceholders(IEnumerable<string> placeholders)
        {
            var placeholderArray = placeholders.ToArray();
            return placeholderArray.Length == 0 ? "<none>" : string.Join(", ", placeholderArray);
        }

        private sealed record PlaceholderAnalysis(string[] Placeholders);
    }
}
