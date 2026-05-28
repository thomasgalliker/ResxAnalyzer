namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that localized values preserve leading and trailing newline characters.
    /// </summary>
    public sealed class NewlineConsistencyCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that localized values preserve the leading and trailing newline characters of the neutral value.";

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
                            NeutralLeading = EdgeSequence.GetLeadingNewlines(entry.Value),
                            LocalizedLeading = EdgeSequence.GetLeadingNewlines(localizedEntry.Value),
                            NeutralTrailing = EdgeSequence.GetTrailingNewlines(entry.Value),
                            LocalizedTrailing = EdgeSequence.GetTrailingNewlines(localizedEntry.Value)
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
            stringBuilder.AppendLine($"Following localized values have inconsistent leading or trailing newlines ({mismatches.Length}):");
            foreach (var mismatch in mismatches)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{mismatch.CultureName}\", Key='{mismatch.Key}', NeutralLeading='{EdgeSequence.Escape(mismatch.NeutralLeading)}', LocalizedLeading='{EdgeSequence.Escape(mismatch.LocalizedLeading)}', NeutralTrailing='{EdgeSequence.Escape(mismatch.NeutralTrailing)}', LocalizedTrailing='{EdgeSequence.Escape(mismatch.LocalizedTrailing)}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
