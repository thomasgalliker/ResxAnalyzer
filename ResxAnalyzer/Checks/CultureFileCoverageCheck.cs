namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that configured localized resource files exist.
    /// </summary>
    public sealed class CultureFileCoverageCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that every configured culture has a localized resource file.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var missingFiles = context.ExpectedLocalizedResourceFiles
                .Where(file => !file.Exists)
                .OrderBy(file => file.CultureName, StringComparer.Ordinal)
                .ToArray();

            if (missingFiles.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following configured culture resource files are missing ({missingFiles.Length}):");
            foreach (var missingFile in missingFiles)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{missingFile.CultureName}\", File='{missingFile.FilePath}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
