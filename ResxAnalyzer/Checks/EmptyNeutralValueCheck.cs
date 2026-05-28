namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that neutral resource values are not empty.
    /// </summary>
    public sealed class EmptyNeutralValueCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that neutral resource values are not empty or whitespace.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var emptyKeys = context.NeutralResource.Entries.Values
                .Where(entry => string.IsNullOrWhiteSpace(entry.Value))
                .Select(entry => entry.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            if (emptyKeys.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following neutral resource values are empty ({emptyKeys.Length}):");
            foreach (var key in emptyKeys)
            {
                stringBuilder.AppendLine($"> {key}");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
