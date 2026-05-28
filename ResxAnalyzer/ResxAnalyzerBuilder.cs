namespace System.Resources
{
    /// <summary>
    /// Fluent builder for configuring and running <see cref="ResxAnalyzer"/>.
    /// </summary>
    public sealed class ResxAnalyzerBuilder
    {
        private const string DefaultInvariantComment = "@Invariant";

        private readonly string resourcePath;
        private Dictionary<CultureInfo, FileInfo>? localizedResourceFiles;
        private readonly List<CultureInfo> cultures = [];
        private readonly List<IResxCheck> checks = GetDefaultChecks();
        private string invariantComment = DefaultInvariantComment;
        private bool useCultures;

        internal ResxAnalyzerBuilder(string resourcePath)
        {
            this.resourcePath = resourcePath;
        }

        internal ResxAnalyzerBuilder(ResxAnalyzerOptions options)
        {
            this.resourcePath = options.ResourcePath;
            if (options.LocalizedResourceFiles.Count > 0)
            {
                this.localizedResourceFiles = options.LocalizedResourceFiles
                    .ToDictionary(pair => new CultureInfo(pair.Key), pair => new FileInfo(pair.Value));
            }

            this.invariantComment = options.InvariantComment;
        }

        /// <summary>
        /// Uses the provided cultures to infer localized .resx files next to the neutral file.
        /// </summary>
        /// <param name="cultures">Cultures to analyze.</param>
        /// <returns>The current builder.</returns>
        public ResxAnalyzerBuilder WithCultures(IEnumerable<CultureInfo> cultures)
        {
            if (this.localizedResourceFiles is not null)
            {
                throw new InvalidOperationException("Cannot combine inferred cultures with explicit localized resource files.");
            }

            this.useCultures = true;
            this.cultures.Clear();
            this.cultures.AddRange(cultures);
            return this;
        }

        /// <summary>
        /// Adds one explicit localized .resx file.
        /// </summary>
        /// <param name="culture">The culture represented by the localized resource file.</param>
        /// <param name="localizedResourceFile">The localized .resx file path.</param>
        /// <returns>The current builder.</returns>
        public ResxAnalyzerBuilder WithLocalizedResource(CultureInfo culture, string localizedResourceFile)
        {
            return this.WithLocalizedResource(culture, new FileInfo(localizedResourceFile));
        }

        /// <summary>
        /// Adds one explicit localized .resx file.
        /// </summary>
        /// <param name="culture">The culture represented by the localized resource file.</param>
        /// <param name="localizedResourceFile">The localized .resx file.</param>
        /// <returns>The current builder.</returns>
        public ResxAnalyzerBuilder WithLocalizedResource(CultureInfo culture, FileInfo localizedResourceFile)
        {
            if (this.useCultures)
            {
                throw new InvalidOperationException("Cannot combine explicit localized resource files with inferred cultures.");
            }

            this.localizedResourceFiles ??= new Dictionary<CultureInfo, FileInfo>();
            this.localizedResourceFiles[culture] = localizedResourceFile;
            return this;
        }

        /// <summary>
        /// Uses the provided comment marker for invariant resources.
        /// </summary>
        /// <param name="invariantComment">The comment marker that identifies keys which do not require localization.</param>
        /// <returns>The current builder.</returns>
        public ResxAnalyzerBuilder WithInvariantComment(string invariantComment)
        {
            this.invariantComment = invariantComment;
            return this;
        }

        /// <summary>
        /// Configures the registered checks to run.
        /// </summary>
        /// <param name="configure">
        /// The check configuration callback. Built-in checks are registered by default and can be removed with
        /// <see cref="ResxChecksBuilder.Clear"/> or <see cref="ResxChecksBuilder.Remove{TCheck}"/>.
        /// </param>
        /// <returns>The current builder.</returns>
        public ResxAnalyzerBuilder WithChecks(Action<ResxChecksBuilder> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new ResxChecksBuilder(this.checks);
            configure(builder);
            return this;
        }

        /// <summary>
        /// Builds an immutable analyzer from the current configuration.
        /// </summary>
        /// <returns>The configured analyzer.</returns>
        public ResxAnalyzer Build()
        {
            var options = this.BuildOptions();
            var internalOptions = ResxAnalyzer.CreateInternalOptions(options);
            if (this.checks.Count == 0)
            {
                throw new InvalidOperationException("At least one .resx check must be registered.");
            }

            return new ResxAnalyzer(internalOptions, this.checks.ToArray());
        }

        private static List<IResxCheck> GetDefaultChecks()
        {
            return
            [
                new CompletenessCheck(),
                new PlaceholderConsistencyCheck(),
                new DuplicateKeyCheck(),
                new EmptyNeutralValueCheck(),
                new SuspiciousSameAsNeutralCheck(),
                new WhitespaceConsistencyCheck(),
                new AcceleratorKeyCheck(),
                new PunctuationConsistencyCheck(),
                new OrphanLocalizedKeyCheck(),
                new CultureFileCoverageCheck(),
                new UnusedCultureFileCheck(),
                new NewlineValueCheck()
            ];
        }

        private ResxAnalyzerOptions BuildOptions()
        {
            return new ResxAnalyzerOptions
            {
                ResourcePath = this.resourcePath,
                LocalizedResourceFiles = this.BuildLocalizedResourceFiles()
                    .ToDictionary(pair => pair.Key.Name, pair => pair.Value.FullName, StringComparer.Ordinal),
                InvariantComment = this.invariantComment
            };
        }

        private IReadOnlyDictionary<CultureInfo, FileInfo> BuildLocalizedResourceFiles()
        {
            if (this.localizedResourceFiles is not null)
            {
                return new Dictionary<CultureInfo, FileInfo>(this.localizedResourceFiles);
            }

            if (this.useCultures)
            {
                return this.cultures.ToDictionary(culture => culture, this.GetLocalizedResourceFileForCulture);
            }

            return new Dictionary<CultureInfo, FileInfo>();
        }

        private FileInfo GetLocalizedResourceFileForCulture(CultureInfo culture)
        {
            var neutralResourceFiles = ResxAnalyzer.ResolveNeutralResourceFiles(this.resourcePath);
            if (neutralResourceFiles.Count != 1)
            {
                throw new InvalidOperationException("WithCultures can infer localized resource files only when ForResource resolves to exactly one neutral .resx file.");
            }

            return ResxAnalyzer.GetLocalizedResourceFileForCulture(neutralResourceFiles[0], culture);
        }
    }
}
