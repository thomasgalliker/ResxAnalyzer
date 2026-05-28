namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that each resource key appears in configured source files.
    /// </summary>
    public sealed class UnusedKeysCheck : IResxCheck
    {
        private readonly IReadOnlyList<DirectoryInfo> searchDirectories;
        private readonly IReadOnlyList<Regex> ignoredKeyPatterns;
        private readonly Lazy<string> sourceText;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnusedKeysCheck"/> class without source directories.
        /// </summary>
        public UnusedKeysCheck()
            : this(_ => { })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnusedKeysCheck"/> class.
        /// </summary>
        /// <param name="configure">The usage scan configuration callback.</param>
        public UnusedKeysCheck(Action<UnusedKeysCheckBuilder> configure)
        {
            var searchDirectories = new List<DirectoryInfo>();
            var ignoredKeyPatterns = new List<Regex>();
            configure(new UnusedKeysCheckBuilder(searchDirectories, ignoredKeyPatterns));
            this.searchDirectories = searchDirectories;
            this.ignoredKeyPatterns = ignoredKeyPatterns;
            this.sourceText = new Lazy<string>(() => ReadSourceText(this.searchDirectories));
        }

        /// <inheritdoc />
        public string Description => "Checks that every resource key appears in the configured source files unless explicitly ignored.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            if (this.searchDirectories.Count == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var unusedKeys = context.AllKeys
                .Where(key => !this.IsIgnored(key))
                .Where(key => !this.ContainsKey(key))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            if (unusedKeys.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Usage scan:");
            stringBuilder.AppendLine("searchDirectories:");
            foreach (var directory in this.searchDirectories)
            {
                stringBuilder.AppendLine($"> {directory.FullName}");
            }

            stringBuilder.AppendLine("searchFileExtensions: .cs, .xaml");
            stringBuilder.AppendLine($"ignoredUsageKeyPatterns: {string.Join(", ", this.ignoredKeyPatterns.Select(pattern => pattern.ToString()))}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Following keys are not used ({unusedKeys.Length}):");
            foreach (var key in unusedKeys)
            {
                stringBuilder.AppendLine($"> {key}");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private bool IsIgnored(string key)
        {
            return this.ignoredKeyPatterns.Any(pattern => pattern.IsMatch(key));
        }

        private bool ContainsKey(string key)
        {
            return this.sourceText.Value.Contains(key, StringComparison.Ordinal);
        }

        private static string ReadSourceText(IEnumerable<DirectoryInfo> searchDirectories)
        {
            var stringBuilder = new StringBuilder();
            foreach (var searchDirectory in searchDirectories)
            {
                if (!searchDirectory.Exists)
                {
                    throw new DirectoryNotFoundException($"Usage search directory does not exist: {searchDirectory.FullName}");
                }

                foreach (var file in searchDirectory.EnumerateFiles("*", SearchOption.AllDirectories).Where(IsUsageSearchFile))
                {
                    stringBuilder.AppendLine(File.ReadAllText(file.FullName));
                }
            }

            return stringBuilder.ToString();
        }

        private static bool IsUsageSearchFile(FileInfo file)
        {
            if (file.DirectoryName?.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) == true ||
                file.DirectoryName?.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }

            if (file.Name.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) ||
                file.Extension.Equals(".resx", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return file.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
                   file.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase);
        }
    }
}
