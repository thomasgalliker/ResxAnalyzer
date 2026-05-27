using System.Text;

namespace Superdev.ResxAnalyzer.Checks
{
    /// <summary>
    /// Checks that discovered localized resource files are part of the configured culture set.
    /// </summary>
    public sealed class UnusedCultureFileCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that discovered localized resource files belong to the configured culture set.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var expectedFilePaths = context.ExpectedLocalizedResourceFiles
                .Select(file => file.FilePath)
                .ToHashSet(StringComparer.Ordinal);

            if (expectedFilePaths.Count == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var unusedFiles = context.DiscoveredLocalizedResourceFiles
                .Where(file => !expectedFilePaths.Contains(file.FilePath))
                .OrderBy(file => file.CultureName, StringComparer.Ordinal)
                .ToArray();

            if (unusedFiles.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following localized resource files are not part of the configured culture set ({unusedFiles.Length}):");
            foreach (var unusedFile in unusedFiles)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{unusedFile.CultureName}\", File='{unusedFile.FilePath}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
