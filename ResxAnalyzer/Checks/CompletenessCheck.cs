using System.Text;

namespace Superdev.ResxAnalyzer.Checks
{
    /// <summary>
    /// Checks that every non-invariant resource key exists in every localized resource.
    /// </summary>
    public sealed class CompletenessCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that every non-invariant resource key exists with a non-empty value in every localized resource.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var missingTranslations = new Dictionary<string, string[]>(StringComparer.Ordinal);
            var requiredKeys = context.AllKeys
                .Where(key => !context.InvariantKeys.Contains(key))
                .ToArray();

            foreach (var localizedResource in context.LocalizedResources)
            {
                var cultureName = localizedResource.CultureName ?? string.Empty;
                var missingKeys = requiredKeys
                    .Where(key => !localizedResource.Entries.TryGetValue(key, out var entry) || string.IsNullOrWhiteSpace(entry.Value))
                    .OrderBy(key => key, StringComparer.Ordinal)
                    .ToArray();

                if (missingKeys.Length > 0)
                {
                    missingTranslations.Add(cultureName, missingKeys);
                }
            }

            if (missingTranslations.Count == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            AppendCultureKeys(
                stringBuilder,
                "Following translation keys are missing:",
                missingTranslations.Select(pair => new KeyValuePair<string, IEnumerable<string>>(pair.Key, pair.Value)));

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private static void AppendCultureKeys(
            StringBuilder stringBuilder,
            string heading,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> culturesWithKeys)
        {
            stringBuilder.AppendLine(heading);
            foreach (var cultureWithKeys in culturesWithKeys)
            {
                var keys = cultureWithKeys.Value.ToArray();
                stringBuilder.AppendLine($"CultureInfo \"{cultureWithKeys.Key}\" ({keys.Length}):");
                foreach (var key in keys)
                {
                    stringBuilder.AppendLine($"> {key}");
                }

                stringBuilder.AppendLine();
            }
        }
    }
}
