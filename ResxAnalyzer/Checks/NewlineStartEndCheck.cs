namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that resource values do not start or end with newline characters.
    /// </summary>
    public sealed class NewlineStartEndCheck : EdgeStartEndCheck
    {
        /// <inheritdoc />
        public override string Description => "Checks that resource values do not start or end with newline characters.";

        /// <inheritdoc />
        protected override string CharacterKind => "newlines";

        /// <inheritdoc />
        protected override string GetLeading(string value)
        {
            return EdgeSequence.GetLeadingNewlines(value);
        }

        /// <inheritdoc />
        protected override string GetTrailing(string value)
        {
            return EdgeSequence.GetTrailingNewlines(value);
        }
    }
}
