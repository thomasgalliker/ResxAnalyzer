namespace System.Resources.Checks
{
    /// <summary>
    /// Provides common logic for checks that reject leading or trailing edge sequences.
    /// </summary>
    public abstract class EdgeStartEndCheck : IResxCheck
    {
        /// <inheritdoc />
        public abstract string Description { get; }

        /// <summary>
        /// Gets the displayed character kind.
        /// </summary>
        protected abstract string CharacterKind { get; }

        /// <summary>
        /// Gets the leading sequence to reject.
        /// </summary>
        /// <param name="value">The resource value.</param>
        /// <returns>The leading sequence, or an empty string when none exists.</returns>
        protected abstract string GetLeading(string value);

        /// <summary>
        /// Gets the trailing sequence to reject.
        /// </summary>
        /// <param name="value">The resource value.</param>
        /// <returns>The trailing sequence, or an empty string when none exists.</returns>
        protected abstract string GetTrailing(string value);

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var invalidValues = context.AllResources
                .SelectMany(resource => resource.Entries.Values.Select(entry => new
                {
                    resource.CultureName,
                    Entry = entry,
                    Leading = this.GetLeading(entry.Value),
                    Trailing = this.GetTrailing(entry.Value)
                }))
                .Where(value => value.Leading.Length > 0 || value.Trailing.Length > 0)
                .OrderBy(value => value.CultureName ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(value => value.Entry.Key, StringComparer.Ordinal)
                .ToArray();

            if (invalidValues.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following translation values start or end with {this.CharacterKind} ({invalidValues.Length}):");
            foreach (var invalidValue in invalidValues)
            {
                var culture = invalidValue.CultureName ?? "neutral";
                stringBuilder.Append($"> CultureInfo {culture}, Key='{invalidValue.Entry.Key}'");
                if (invalidValue.Leading.Length > 0)
                {
                    stringBuilder.Append($", Leading='{EdgeSequence.Escape(invalidValue.Leading)}'");
                }

                if (invalidValue.Trailing.Length > 0)
                {
                    stringBuilder.Append($", Trailing='{EdgeSequence.Escape(invalidValue.Trailing)}'");
                }

                stringBuilder.AppendLine();
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
