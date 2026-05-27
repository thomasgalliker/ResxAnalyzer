namespace Superdev.ResxAnalyzer
{
    /// <summary>
    /// Provides parsed resources and helper services to analyzer checks.
    /// </summary>
    public sealed class ResxAnalysisContext
    {
        internal ResxAnalysisContext(
            ResxResource neutralResource,
            IReadOnlyList<ResxResource> localizedResources,
            IReadOnlyList<ResxLocalizedResourceFile> expectedLocalizedResourceFiles,
            IReadOnlyList<ResxLocalizedResourceFile> discoveredLocalizedResourceFiles,
            IReadOnlyCollection<string> invariantKeys)
        {
            this.NeutralResource = neutralResource;
            this.LocalizedResources = localizedResources;
            this.ExpectedLocalizedResourceFiles = expectedLocalizedResourceFiles;
            this.DiscoveredLocalizedResourceFiles = discoveredLocalizedResourceFiles;
            this.InvariantKeys = invariantKeys;
            this.AllResources = new[] { neutralResource }.Concat(localizedResources).ToArray();
            this.AllKeys = this.AllResources
                .SelectMany(resource => resource.Entries.Keys)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// Gets the neutral resource.
        /// </summary>
        public ResxResource NeutralResource { get; }

        /// <summary>
        /// Gets localized resources.
        /// </summary>
        public IReadOnlyList<ResxResource> LocalizedResources { get; }

        /// <summary>
        /// Gets localized resource files expected by the analyzer configuration.
        /// </summary>
        public IReadOnlyList<ResxLocalizedResourceFile> ExpectedLocalizedResourceFiles { get; }

        /// <summary>
        /// Gets localized sibling resource files discovered next to the neutral resource.
        /// </summary>
        public IReadOnlyList<ResxLocalizedResourceFile> DiscoveredLocalizedResourceFiles { get; }

        /// <summary>
        /// Gets all resources, starting with the neutral resource.
        /// </summary>
        public IReadOnlyList<ResxResource> AllResources { get; }

        /// <summary>
        /// Gets all resource keys found in neutral and localized resources.
        /// </summary>
        public IReadOnlyList<string> AllKeys { get; }

        /// <summary>
        /// Gets keys marked invariant in the neutral resource.
        /// </summary>
        public IReadOnlyCollection<string> InvariantKeys { get; }
    }
}
