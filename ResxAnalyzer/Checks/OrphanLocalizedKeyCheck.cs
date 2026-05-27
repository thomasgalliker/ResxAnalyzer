using System.Text;

namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that localized resources do not contain keys missing from neutral resources.
    /// </summary>
    public sealed class OrphanLocalizedKeyCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that localized resources do not contain keys missing from the neutral resource.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var neutralKeys = context.NeutralResource.Entries.Keys.ToHashSet(StringComparer.Ordinal);
            var orphanKeysByCulture = context.LocalizedResources
                .Select(resource => new
                {
                    CultureName = resource.CultureName ?? string.Empty,
                    Keys = resource.Entries.Keys
                        .Where(key => !neutralKeys.Contains(key))
                        .OrderBy(key => key, StringComparer.Ordinal)
                        .ToArray()
                })
                .Where(item => item.Keys.Length > 0)
                .OrderBy(item => item.CultureName, StringComparer.Ordinal)
                .ToArray();

            if (orphanKeysByCulture.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Following localized keys do not exist in the neutral resource:");
            foreach (var cultureWithKeys in orphanKeysByCulture)
            {
                stringBuilder.AppendLine($"CultureInfo \"{cultureWithKeys.CultureName}\" ({cultureWithKeys.Keys.Length}):");
                foreach (var key in cultureWithKeys.Keys)
                {
                    stringBuilder.AppendLine($"> {key}");
                }

                stringBuilder.AppendLine();
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
