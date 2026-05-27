namespace System.Resources
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
            if (check is null)
            {
                throw new ArgumentNullException(nameof(check));
            }

            var checkType = check.GetType();
            if (this.checks.Any(existingCheck => existingCheck.GetType() == checkType))
            {
                throw new InvalidOperationException($"A check of type {checkType.Name} is already configured. Each check type can only be registered once.");
            }

            this.checks.Add(check);
            return this;
        }
    }
}
