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
        /// Removes all registered checks.
        /// </summary>
        /// <returns>The current builder.</returns>
        public ResxChecksBuilder Clear()
        {
            this.checks.Clear();
            return this;
        }

        /// <summary>
        /// Adds an analyzer check.
        /// </summary>
        /// <param name="check">The check to add.</param>
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

        /// <summary>
        /// Removes the registered check of the specified type.
        /// </summary>
        /// <typeparam name="TCheck">The type of the registered check to remove.</typeparam>
        /// <returns>The current builder.</returns>
        public ResxChecksBuilder Remove<TCheck>()
            where TCheck : IResxCheck
        {
            return this.Remove(typeof(TCheck));
        }

        /// <summary>
        /// Removes the registered check of the specified type.
        /// </summary>
        /// <param name="checkType">The type of the registered check to remove.</param>
        /// <returns>The current builder.</returns>
        public ResxChecksBuilder Remove(Type checkType)
        {
            var check = GetCheck(this.checks, checkType);
            this.checks.Remove(check);
            return this;
        }

        /// <summary>
        /// Removes the registered check that has the specified type name.
        /// </summary>
        /// <param name="checkName">The registered check type name to remove.</param>
        /// <returns>The current builder.</returns>
        public ResxChecksBuilder Remove(string checkName)
        {
            var check = GetCheck(this.checks, checkName);
            this.checks.Remove(check);
            return this;
        }

        internal static IResxCheck GetCheck(IEnumerable<IResxCheck> checks, Type checkType)
        {
            if (checkType is null)
            {
                throw new ArgumentNullException(nameof(checkType));
            }

            if (!typeof(IResxCheck).IsAssignableFrom(checkType))
            {
                throw new ArgumentException($"Check type {checkType.Name} must implement {nameof(IResxCheck)}.", nameof(checkType));
            }

            var check = checks.SingleOrDefault(configuredCheck => configuredCheck.GetType() == checkType);
            if (check is null)
            {
                throw new InvalidOperationException($"No registered check of type {checkType.Name} was found.");
            }

            return check;
        }

        internal static IResxCheck GetCheck(IEnumerable<IResxCheck> checks, string checkName)
        {
            if (checkName is null)
            {
                throw new ArgumentNullException(nameof(checkName));
            }

            if (string.IsNullOrWhiteSpace(checkName))
            {
                throw new ArgumentException("Check name must not be empty.", nameof(checkName));
            }

            var check = checks.SingleOrDefault(configuredCheck => string.Equals(configuredCheck.GetType().Name, checkName, StringComparison.Ordinal));
            if (check is null)
            {
                throw new InvalidOperationException($"No registered check named {checkName} was found.");
            }

            return check;
        }
    }
}
