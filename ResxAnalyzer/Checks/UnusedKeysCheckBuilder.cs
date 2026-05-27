namespace System.Resources.Checks
{
    /// <summary>
    /// Fluent builder for configuring <see cref="UnusedKeysCheck"/>.
    /// </summary>
    public sealed class UnusedKeysCheckBuilder
    {
        private readonly List<DirectoryInfo> searchDirectories;
        private readonly List<Regex> ignoredKeyPatterns;

        internal UnusedKeysCheckBuilder(List<DirectoryInfo> searchDirectories, List<Regex> ignoredKeyPatterns)
        {
            this.searchDirectories = searchDirectories;
            this.ignoredKeyPatterns = ignoredKeyPatterns;
        }

        /// <summary>
        /// Adds a directory to scan for resource key usage.
        /// </summary>
        /// <param name="directory">The source directory path.</param>
        /// <returns>The current builder.</returns>
        public UnusedKeysCheckBuilder In(string directory)
        {
            return this.In(new DirectoryInfo(directory));
        }

        /// <summary>
        /// Adds a directory to scan for resource key usage.
        /// </summary>
        /// <param name="directory">The source directory.</param>
        /// <returns>The current builder.</returns>
        public UnusedKeysCheckBuilder In(DirectoryInfo directory)
        {
            this.searchDirectories.Add(directory);
            return this;
        }

        /// <summary>
        /// Excludes resource keys matching the given regex pattern from unused-key analysis.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns>The current builder.</returns>
        public UnusedKeysCheckBuilder IgnoreKeys(string pattern)
        {
            return this.IgnoreKeys(new Regex(pattern, RegexOptions.Compiled));
        }

        /// <summary>
        /// Excludes resource keys matching the given regex pattern from unused-key analysis.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns>The current builder.</returns>
        public UnusedKeysCheckBuilder IgnoreKeys(Regex pattern)
        {
            this.ignoredKeyPatterns.Add(pattern);
            return this;
        }
    }
}
