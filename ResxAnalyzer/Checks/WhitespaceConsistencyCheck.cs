using System.Text;
using System.Text.RegularExpressions;

namespace Superdev.ResxAnalyzer.Checks
{
    /// <summary>
    /// Checks that localized values preserve leading and trailing whitespace.
    /// </summary>
    public sealed class WhitespaceConsistencyCheck : IResxCheck
    {
        private static readonly Regex LeadingWhitespaceRegex = new(@"^\s*", RegexOptions.Compiled);
        private static readonly Regex TrailingWhitespaceRegex = new(@"\s*$", RegexOptions.Compiled);

        /// <inheritdoc />
        public string Description => "Checks that localized values preserve the leading and trailing whitespace of the neutral value.";

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
                            NeutralLeading = GetLeadingWhitespace(entry.Value),
                            LocalizedLeading = GetLeadingWhitespace(localizedEntry.Value),
                            NeutralTrailing = GetTrailingWhitespace(entry.Value),
                            LocalizedTrailing = GetTrailingWhitespace(localizedEntry.Value)
                        };
                    })
                    .Where(item => !string.Equals(item.NeutralLeading, item.LocalizedLeading, StringComparison.Ordinal) ||
                                   !string.Equals(item.NeutralTrailing, item.LocalizedTrailing, StringComparison.Ordinal)))
                .OrderBy(item => item.CultureName, StringComparer.Ordinal)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();

            if (mismatches.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following localized values have inconsistent leading or trailing whitespace ({mismatches.Length}):");
            foreach (var mismatch in mismatches)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{mismatch.CultureName}\", Key='{mismatch.Key}', NeutralLeading='{Escape(mismatch.NeutralLeading)}', LocalizedLeading='{Escape(mismatch.LocalizedLeading)}', NeutralTrailing='{Escape(mismatch.NeutralTrailing)}', LocalizedTrailing='{Escape(mismatch.LocalizedTrailing)}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private static string GetLeadingWhitespace(string value)
        {
            return LeadingWhitespaceRegex.Match(value).Value;
        }

        private static string GetTrailingWhitespace(string value)
        {
            return TrailingWhitespaceRegex.Match(value).Value;
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\t", "\\t", StringComparison.Ordinal);
        }
    }
}
