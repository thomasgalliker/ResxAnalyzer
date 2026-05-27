using System.Text;

namespace Superdev.ResxAnalyzer.Checks
{
    /// <summary>
    /// Checks that localized values are not suspiciously identical to neutral values.
    /// </summary>
    public sealed class SuspiciousSameAsNeutralCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that localized values are not identical to the neutral value for non-invariant keys.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var matches = context.LocalizedResources
                .SelectMany(localizedResource => context.NeutralResource.Entries.Values
                    .Where(entry => !context.InvariantKeys.Contains(entry.Key))
                    .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
                    .Where(entry => localizedResource.Entries.TryGetValue(entry.Key, out var localizedEntry) &&
                                    string.Equals(entry.Value, localizedEntry.Value, StringComparison.Ordinal))
                    .Select(entry => new
                    {
                        CultureName = localizedResource.CultureName ?? string.Empty,
                        entry.Key,
                        entry.Value
                    }))
                .OrderBy(item => item.CultureName, StringComparer.Ordinal)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();

            if (matches.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following localized values are identical to neutral values ({matches.Length}):");
            foreach (var match in matches)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{match.CultureName}\", Key='{match.Key}', Value='{match.Value}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
