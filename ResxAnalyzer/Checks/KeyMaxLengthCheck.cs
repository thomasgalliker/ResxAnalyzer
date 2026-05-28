namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that resource keys do not exceed a configured length.
    /// </summary>
    public sealed class KeyMaxLengthCheck : IResxCheck
    {
        private readonly int maxLength;

        public KeyMaxLengthCheck(int maxLength)
        {
            if (maxLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "Maximum key length must be greater than zero.");
            }

            this.maxLength = maxLength;
        }

        /// <inheritdoc />
        public string Description => $"Checks that resource keys do not exceed {this.maxLength} characters.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var longKeys = context.AllKeys
                .Where(key => key.Length > this.maxLength)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            if (longKeys.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following resource keys exceed {this.maxLength} characters ({longKeys.Length}):");
            foreach (var key in longKeys)
            {
                stringBuilder.AppendLine($"> {key} ({key.Length})");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }
    }
}
