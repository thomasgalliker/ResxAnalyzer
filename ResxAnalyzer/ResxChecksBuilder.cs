namespace Superdev.ResxAnalyzer
{
    /// <summary>
    /// Fluent builder for configuring analyzer checks.
    /// </summary>
    public sealed class ResxChecksBuilder
    {
        private readonly List<IResxCheck> checks;

        internal ResxChecksBuilder(List<IResxCheck> checks)
        {
            this.checks = checks;
        }

        /// <summary>
        /// Adds a custom analyzer check.
        /// </summary>
        /// <param name="check">The custom check.</param>
        /// <returns>The current builder.</returns>
        public ResxChecksBuilder Add(IResxCheck check)
        {
            this.checks.Add(check);
            return this;
        }
    }
}
