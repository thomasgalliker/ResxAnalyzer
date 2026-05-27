using System.Text;

namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that resource files do not contain duplicate keys.
    /// </summary>
    public sealed class DuplicateKeyCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that each resource file contains every key only once.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var duplicates = context.AllResources
                .SelectMany(resource => resource.AllEntries
                    .GroupBy(entry => entry.Key, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .Select(group => new
                    {
                        CultureName = resource.CultureName ?? "neutral",
                        Key = group.Key,
                        Count = group.Count()
                    }))
                .OrderBy(item => item.CultureName, StringComparer.Ordinal)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();

            if (duplicates.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following resource keys are duplicated ({duplicates.Length}):");
            foreach (var duplicate in duplicates)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{duplicate.CultureName}\", Key='{duplicate.Key}', Count={duplicate.Count}");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
