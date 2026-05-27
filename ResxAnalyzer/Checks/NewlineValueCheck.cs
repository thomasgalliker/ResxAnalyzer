using System.Text;

namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that resource values do not start or end with newlines.
    /// </summary>
    public sealed class NewlineValueCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that resource values do not start or end with newlines.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var invalidValues = context.AllResources
                .SelectMany(resource => resource.Entries.Values.Select(entry => new
                {
                    resource.CultureName,
                    Entry = entry,
                    Violations = GetViolations(entry.Value)
                }))
                .Where(value => value.Violations.Length > 0)
                .OrderBy(value => value.CultureName ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(value => value.Entry.Key, StringComparer.Ordinal)
                .ToArray();

            if (invalidValues.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Following translation values are invalid:");
            foreach (var invalidValue in invalidValues)
            {
                var culture = invalidValue.CultureName ?? "neutral";
                stringBuilder.AppendLine($"> CultureInfo {culture}, Key='{invalidValue.Entry.Key}', Violations={string.Join(", ", invalidValue.Violations)}, Value='{invalidValue.Entry.Value}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private static string[] GetViolations(string value)
        {
            var violations = new List<string>();
            if (value.StartsWith("\n", StringComparison.Ordinal) ||
                value.StartsWith("\n\r", StringComparison.Ordinal) ||
                value.StartsWith("\r", StringComparison.Ordinal))
            {
                violations.Add("must not start with a newline");
            }

            if (value.EndsWith("\n", StringComparison.Ordinal) ||
                value.EndsWith("\n\r", StringComparison.Ordinal) ||
                value.EndsWith("\r", StringComparison.Ordinal))
            {
                violations.Add("must not end with a newline");
            }

            return violations.ToArray();
        }
    }
}
