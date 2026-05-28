namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that resource values do not start or end with newlines.
    /// </summary>
    [Obsolete("Use NewlineStartEndCheck instead.", error: false)]
    public sealed class NewlineValueCheck : IResxCheck
    {
        private readonly NewlineStartEndCheck innerCheck = new();

        /// <inheritdoc />
        public string Description => this.innerCheck.Description;

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            return this.innerCheck.Analyze(context);
        }
    }
}
