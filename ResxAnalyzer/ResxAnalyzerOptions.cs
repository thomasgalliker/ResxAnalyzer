namespace Superdev.ResxAnalyzer
{
    /// <summary>
    /// JSON-serializable configuration for <see cref="ResxAnalyzer"/>.
    /// </summary>
    public sealed class ResxAnalyzerOptions
    {
        /// <summary>
        /// Gets or sets the resource path. This can be a neutral .resx file, a directory containing .resx files,
        /// or a glob pattern with * and ** wildcards. When localized resources are not configured explicitly,
        /// localized sibling files are discovered automatically.
        /// </summary>
        public string ResourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets localized .resx file paths keyed by culture name.
        /// When empty, localized sibling resources are discovered next to each neutral file.
        /// When populated, only these localized resources are used as expected localized files.
        /// </summary>
        public Dictionary<string, string> LocalizedResourceFiles { get; set; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the comment marker that identifies keys which do not require localization.
        /// </summary>
        public string InvariantComment { get; set; } = "@Invariant";
    }
}
