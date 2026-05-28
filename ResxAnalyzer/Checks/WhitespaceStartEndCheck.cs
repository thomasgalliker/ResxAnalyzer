namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that resource values do not start or end with non-newline whitespace.
    /// </summary>
    public sealed class WhitespaceStartEndCheck : EdgeStartEndCheck
    {
        /// <inheritdoc />
        public override string Description => "Checks that resource values do not start or end with non-newline whitespace.";

        /// <inheritdoc />
        protected override string CharacterKind => "whitespace";

        /// <inheritdoc />
        protected override string GetLeading(string value)
        {
            return EdgeSequence.GetLeadingHorizontalWhitespace(value);
        }

        /// <inheritdoc />
        protected override string GetTrailing(string value)
        {
            return EdgeSequence.GetTrailingHorizontalWhitespace(value);
        }
    }
}
