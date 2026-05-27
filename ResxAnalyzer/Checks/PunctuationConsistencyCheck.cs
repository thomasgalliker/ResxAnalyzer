using System.Text;

namespace Superdev.ResxAnalyzer.Checks
{
    /// <summary>
    /// Checks that localized values preserve terminal punctuation.
    /// </summary>
    public sealed class PunctuationConsistencyCheck : IResxCheck
    {
        private static readonly char[] TerminalPunctuation = ['.', '?', '!', ':', ';', '…'];

        /// <inheritdoc />
        public string Description => "Checks that localized values preserve terminal punctuation from the neutral value.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var mismatches = context.LocalizedResources
                .SelectMany(localizedResource => context.NeutralResource.Entries.Values
                    .Where(entry => !context.InvariantKeys.Contains(entry.Key))
                    .Where(entry => localizedResource.Entries.TryGetValue(entry.Key, out _))
                    .Select(entry =>
                    {
                        var localizedEntry = localizedResource.Entries[entry.Key];
                        return new
                        {
                            CultureName = localizedResource.CultureName ?? string.Empty,
                            entry.Key,
                            NeutralPunctuation = GetTerminalPunctuation(entry.Value),
                            LocalizedPunctuation = GetTerminalPunctuation(localizedEntry.Value)
                        };
                    })
                    .Where(item => !string.Equals(item.NeutralPunctuation, item.LocalizedPunctuation, StringComparison.Ordinal)))
                .OrderBy(item => item.CultureName, StringComparer.Ordinal)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();

            if (mismatches.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following localized values have inconsistent terminal punctuation ({mismatches.Length}):");
            foreach (var mismatch in mismatches)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{mismatch.CultureName}\", Key='{mismatch.Key}', Neutral='{FormatPunctuation(mismatch.NeutralPunctuation)}', Localized='{FormatPunctuation(mismatch.LocalizedPunctuation)}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private static string GetTerminalPunctuation(string value)
        {
            var trimmedValue = value.TrimEnd();
            if (trimmedValue.EndsWith("...", StringComparison.Ordinal))
            {
                return "...";
            }

            if (trimmedValue.Length == 0)
            {
                return string.Empty;
            }

            var lastCharacter = trimmedValue[trimmedValue.Length - 1];
            return TerminalPunctuation.Contains(lastCharacter)
                ? lastCharacter.ToString()
                : string.Empty;
        }

        private static string FormatPunctuation(string punctuation)
        {
            return string.IsNullOrEmpty(punctuation) ? "<none>" : punctuation;
        }
    }
}
