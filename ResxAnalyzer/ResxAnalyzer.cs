using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace System.Resources
{
    /// <summary>
    /// Analyzes .resx files with configurable built-in and custom checks.
    /// </summary>
    public sealed class ResxAnalyzer
    {
        private readonly InternalResxAnalyzerOptions options;
        private readonly IReadOnlyList<IResxCheck> checks;

        internal ResxAnalyzer(InternalResxAnalyzerOptions options, IReadOnlyList<IResxCheck> checks)
        {
            this.options = options;
            this.checks = checks.ToArray();
        }

        /// <summary>
        /// Creates a fluent analysis builder for a resource file, directory, or glob pattern.
        /// When no explicit cultures or localized resources are configured, localized sibling files
        /// such as Strings.de.resx are discovered automatically for each neutral resource file.
        /// </summary>
        /// <param name="resourcePath">
        /// A neutral .resx file path, a directory containing neutral .resx files, or a glob pattern with * and ** wildcards.
        /// Localized .resx files that match the neutral resource name plus a culture suffix are treated as localized siblings.
        /// </param>
        /// <returns>A builder for configuring and running the analysis.</returns>
        public static ResxAnalyzerBuilder ForResource(string resourcePath)
        {
            return new ResxAnalyzerBuilder(resourcePath);
        }

        /// <summary>
        /// Creates a fluent analysis builder from JSON-serializable options.
        /// </summary>
        /// <param name="options">The resource options.</param>
        /// <returns>A builder for configuring checks and building the analyzer.</returns>
        public static ResxAnalyzerBuilder ForOptions(ResxAnalyzerOptions options)
        {
            return new ResxAnalyzerBuilder(options);
        }

        /// <summary>
        /// Runs the configured analysis.
        /// </summary>
        /// <returns>The complete analysis result.</returns>
        public ResxAnalysisResult Analyze()
        {
            return this.Analyze(this.checks);
        }

        /// <summary>
        /// Runs the configured analysis with the configured check of the specified type.
        /// </summary>
        /// <typeparam name="TCheck">The type of the configured check to run.</typeparam>
        /// <returns>The complete analysis result.</returns>
        public ResxAnalysisResult Analyze<TCheck>()
            where TCheck : IResxCheck
        {
            return this.Analyze(typeof(TCheck));
        }

        /// <summary>
        /// Runs the configured analysis with the configured check of the specified type.
        /// </summary>
        /// <param name="checkType">The type of the configured check to run.</param>
        /// <returns>The complete analysis result.</returns>
        public ResxAnalysisResult Analyze(Type checkType)
        {
            if (checkType is null)
            {
                throw new ArgumentNullException(nameof(checkType));
            }

            return this.Analyze(new[] { checkType });
        }

        /// <summary>
        /// Runs the configured analysis with the configured checks of the specified types.
        /// </summary>
        /// <param name="checkTypes">The types of the configured checks to run.</param>
        /// <returns>The complete analysis result.</returns>
        public ResxAnalysisResult Analyze(params Type[] checkTypes)
        {
            if (checkTypes is null)
            {
                throw new ArgumentNullException(nameof(checkTypes));
            }

            if (checkTypes.Length == 0)
            {
                throw new ArgumentException("At least one check type must be specified.", nameof(checkTypes));
            }

            return this.Analyze(checkTypes.Select(this.GetConfiguredCheck).ToArray());
        }

        /// <summary>
        /// Runs the configured analysis with the configured check that has the specified type name.
        /// </summary>
        /// <param name="checkName">The configured check type name to run.</param>
        /// <returns>The complete analysis result.</returns>
        public ResxAnalysisResult Analyze(string checkName)
        {
            if (checkName is null)
            {
                throw new ArgumentNullException(nameof(checkName));
            }

            return this.Analyze(new[] { checkName });
        }

        /// <summary>
        /// Runs the configured analysis with the configured checks that have the specified type names.
        /// </summary>
        /// <param name="checkNames">The configured check type names to run.</param>
        /// <returns>The complete analysis result.</returns>
        public ResxAnalysisResult Analyze(params string[] checkNames)
        {
            if (checkNames is null)
            {
                throw new ArgumentNullException(nameof(checkNames));
            }

            if (checkNames.Length == 0)
            {
                throw new ArgumentException("At least one check name must be specified.", nameof(checkNames));
            }

            return this.Analyze(checkNames.Select(this.GetConfiguredCheck).ToArray());
        }

        private ResxAnalysisResult Analyze(IReadOnlyList<IResxCheck> checksToRun)
        {
            var groupResults = this.options.ResourceGroups
                .Select(group => new ResourceGroupAnalysisResult(
                    group.NeutralResourceFile.FullName,
                    checksToRun.Select(check => new CheckRunResult(
                        check,
                        check.Analyze(CreateContext(this.options, group)).WithCheckName(check.GetType().Name))).ToArray()))
                .ToArray();
            var checkResults = groupResults.SelectMany(result => result.CheckRuns.Select(run => run.Result)).ToArray();
            var succeeded = checkResults.All(result => result.Succeeded);
            var report = FormatReport(groupResults);

            return new ResxAnalysisResult(succeeded, report, checkResults);
        }

        private IResxCheck GetConfiguredCheck(Type checkType)
        {
            if (checkType is null)
            {
                throw new ArgumentNullException(nameof(checkType));
            }

            if (!typeof(IResxCheck).IsAssignableFrom(checkType))
            {
                throw new ArgumentException($"Check type {checkType.Name} must implement {nameof(IResxCheck)}.", nameof(checkType));
            }

            var check = this.checks.SingleOrDefault(configuredCheck => configuredCheck.GetType() == checkType);
            if (check is null)
            {
                throw new InvalidOperationException($"No configured check of type {checkType.Name} was found.");
            }

            return check;
        }

        private IResxCheck GetConfiguredCheck(string checkName)
        {
            if (checkName is null)
            {
                throw new ArgumentNullException(nameof(checkName));
            }

            if (string.IsNullOrWhiteSpace(checkName))
            {
                throw new ArgumentException("Check name must not be empty.", nameof(checkName));
            }

            var check = this.checks.SingleOrDefault(configuredCheck => string.Equals(configuredCheck.GetType().Name, checkName, StringComparison.Ordinal));
            if (check is null)
            {
                throw new InvalidOperationException($"No configured check named {checkName} was found.");
            }

            return check;
        }

        private static string FormatReport(IReadOnlyList<ResourceGroupAnalysisResult> groupResults)
        {
            var stringBuilder = new StringBuilder();

            foreach (var groupResult in groupResults)
            {
                if (groupResults.Count > 1)
                {
                    stringBuilder.AppendLine($"Resource \"{groupResult.ResourceFilePath}\"");
                    stringBuilder.AppendLine();
                }

                foreach (var checkRun in groupResult.CheckRuns)
                {
                    var checkResult = checkRun.Result;
                    if (checkResult.Succeeded)
                    {
                        stringBuilder.AppendLine($"Check \"{checkResult.CheckName}\" succeeded");
                        AppendDescription(stringBuilder, checkRun.Check);
                        stringBuilder.AppendLine();
                        continue;
                    }

                    stringBuilder.AppendLine($"Check \"{checkResult.CheckName}\" failed");
                    AppendDescription(stringBuilder, checkRun.Check);
                    if (!string.IsNullOrWhiteSpace(checkResult.Report))
                    {
                        stringBuilder.AppendLine(checkResult.Report);
                    }

                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString().TrimEnd();
        }

        private static void AppendDescription(StringBuilder stringBuilder, IResxCheck check)
        {
            if (!string.IsNullOrWhiteSpace(check.Description))
            {
                stringBuilder.AppendLine(check.Description);
            }
        }

        internal static IReadOnlyDictionary<CultureInfo, FileInfo> DiscoverLocalizedResourceFiles(FileInfo neutralResourceFile)
        {
            var directory = neutralResourceFile.Directory;
            if (directory is null || !directory.Exists)
            {
                return new Dictionary<CultureInfo, FileInfo>();
            }

            var neutralName = Path.GetFileNameWithoutExtension(neutralResourceFile.Name);
            return directory
                .EnumerateFiles($"{neutralName}.*.resx", SearchOption.TopDirectoryOnly)
                .Select(file => new
                {
                    Culture = TryGetCultureInfo(file, neutralName),
                    File = file
                })
                .Where(item => item.Culture is not null)
                .ToDictionary(item => item.Culture!, item => item.File);
        }

        internal static FileInfo GetLocalizedResourceFileForCulture(FileInfo neutralResourceFile, CultureInfo culture)
        {
            var cultureName = string.IsNullOrEmpty(culture.Parent.Name) ? culture.Name : culture.Parent.Name;
            return new FileInfo(Path.Combine(
                neutralResourceFile.DirectoryName ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(neutralResourceFile.Name)}.{cultureName}.resx"));
        }

        internal static InternalResxAnalyzerOptions CreateInternalOptions(ResxAnalyzerOptions options)
        {
            var neutralResourceFiles = ResolveNeutralResourceFiles(options);
            if (neutralResourceFiles.Count == 0)
            {
                throw new FileNotFoundException($"No neutral .resx files found for resource path: {GetResourcePath(options)}");
            }

            if (options.LocalizedResourceFiles.Count > 0 && neutralResourceFiles.Count != 1)
            {
                throw new InvalidOperationException("Explicit localized resource files can be configured only when the resource path resolves to exactly one neutral .resx file.");
            }

            var resourceGroups = neutralResourceFiles
                .Select(neutralResourceFile =>
                {
                    var discoveredLocalizedResourceFiles = DiscoverLocalizedResourceFiles(neutralResourceFile);
                    var expectedLocalizedResourceFiles = options.LocalizedResourceFiles.Count == 0
                        ? discoveredLocalizedResourceFiles
                        : options.LocalizedResourceFiles.ToDictionary(
                            pair => new CultureInfo(pair.Key),
                            pair => new FileInfo(pair.Value));

                    return new NormalizedResourceGroup(
                        neutralResourceFile,
                        expectedLocalizedResourceFiles,
                        discoveredLocalizedResourceFiles);
                })
                .ToArray();

            return new InternalResxAnalyzerOptions(
                resourceGroups,
                options.InvariantComment);
        }

        internal static IReadOnlyList<FileInfo> ResolveNeutralResourceFiles(string resourcePath)
        {
            return ResolveNeutralResourceFiles(resourcePath, false);
        }

        private static IReadOnlyList<FileInfo> ResolveNeutralResourceFiles(ResxAnalyzerOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ResourcePath))
            {
                return ResolveNeutralResourceFiles(options.ResourcePath, false);
            }

            throw new InvalidOperationException("ResourcePath must be configured.");
        }

        private static IReadOnlyList<FileInfo> ResolveNeutralResourceFiles(string resourcePath, bool explicitFile)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new ArgumentException("Resource path must not be empty.", nameof(resourcePath));
            }

            if (ContainsWildcard(resourcePath))
            {
                return ResolveGlobResourceFiles(resourcePath);
            }

            var fullPath = Path.GetFullPath(resourcePath);
            if (Directory.Exists(fullPath))
            {
                return new DirectoryInfo(fullPath)
                    .EnumerateFiles("*.resx", SearchOption.TopDirectoryOnly)
                    .Where(IsNeutralResourceFile)
                    .OrderBy(file => file.FullName, StringComparer.Ordinal)
                    .ToArray();
            }

            return explicitFile || Path.GetExtension(fullPath).Equals(".resx", StringComparison.OrdinalIgnoreCase)
                ? new[] { new FileInfo(fullPath) }
                : Array.Empty<FileInfo>();
        }

        private static IReadOnlyList<FileInfo> ResolveGlobResourceFiles(string resourcePath)
        {
            var fullPattern = Path.GetFullPath(resourcePath);
            var searchRoot = GetGlobSearchRoot(fullPattern);
            if (!Directory.Exists(searchRoot))
            {
                return Array.Empty<FileInfo>();
            }

            var matcher = CreateGlobMatcher(fullPattern);
            return new DirectoryInfo(searchRoot)
                .EnumerateFiles("*.resx", SearchOption.AllDirectories)
                .Where(file => matcher.IsMatch(NormalizePath(file.FullName)))
                .Where(IsNeutralResourceFile)
                .OrderBy(file => file.FullName, StringComparer.Ordinal)
                .ToArray();
        }

        private static string GetResourcePath(ResxAnalyzerOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ResourcePath))
            {
                return options.ResourcePath;
            }

            return string.Empty;
        }

        private static ResxAnalysisContext CreateContext(InternalResxAnalyzerOptions options, NormalizedResourceGroup resourceGroup)
        {
            var neutral = LoadResource(resourceGroup.NeutralResourceFile, null);
            var localized = resourceGroup.ExpectedLocalizedResourceFiles
                .Where(pair => pair.Value.Exists)
                .Select(pair => LoadResource(pair.Value, pair.Key))
                .ToArray();
            var invariantKeys = neutral.Entries
                .Where(entry => entry.Value.Comment.Contains(options.InvariantComment, StringComparison.Ordinal))
                .Select(entry => entry.Key)
                .ToHashSet(StringComparer.Ordinal);

            return new ResxAnalysisContext(
                neutral,
                localized,
                ToLocalizedResourceFiles(resourceGroup.ExpectedLocalizedResourceFiles),
                ToLocalizedResourceFiles(resourceGroup.DiscoveredLocalizedResourceFiles),
                invariantKeys);
        }

        private static ResxResource LoadResource(FileInfo file, CultureInfo? culture)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException($"Resource file does not exist: {file.FullName}", file.FullName);
            }

            var allEntries = XDocument.Load(file.FullName)
                .Root!
                .Elements("data")
                .Select(element =>
                {
                    var key = element.Attribute("name")?.Value ?? string.Empty;
                    return new ResxResourceEntry(
                        key,
                        element.Element("value")?.Value ?? string.Empty,
                        element.Element("comment")?.Value ?? string.Empty);
                })
                .ToArray();
            var entries = allEntries
                .GroupBy(entry => entry.Key, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.Ordinal);

            return new ResxResource(file.FullName, culture?.Name, entries, allEntries);
        }

        private static ResxLocalizedResourceFile[] ToLocalizedResourceFiles(IReadOnlyDictionary<CultureInfo, FileInfo> localizedResourceFiles)
        {
            return localizedResourceFiles
                .OrderBy(pair => pair.Key.Name, StringComparer.Ordinal)
                .Select(pair => new ResxLocalizedResourceFile(pair.Key.Name, pair.Value.FullName, pair.Value.Exists))
                .ToArray();
        }

        private static CultureInfo? TryGetCultureInfo(FileInfo file, string neutralName)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);
            var cultureName = name.Substring(neutralName.Length + 1);
            try
            {
                return new CultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }

        private static bool IsNeutralResourceFile(FileInfo file)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
            var lastDotIndex = fileNameWithoutExtension.LastIndexOf('.');
            if (lastDotIndex < 0)
            {
                return true;
            }

            var suffix = fileNameWithoutExtension.Substring(lastDotIndex + 1);
            try
            {
                _ = new CultureInfo(suffix);
                return false;
            }
            catch (CultureNotFoundException)
            {
                return true;
            }
        }

        private static bool ContainsWildcard(string path)
        {
            return path.IndexOf('*') >= 0;
        }

        private static string GetGlobSearchRoot(string fullPattern)
        {
            var wildcardIndex = fullPattern.IndexOf('*');
            var prefix = wildcardIndex < 0 ? fullPattern : fullPattern.Substring(0, wildcardIndex);
            var separatorIndex = prefix.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            if (separatorIndex < 0)
            {
                return Directory.GetCurrentDirectory();
            }

            var root = prefix.Substring(0, separatorIndex + 1);
            return string.IsNullOrWhiteSpace(root)
                ? Directory.GetCurrentDirectory()
                : root;
        }

        private static Regex CreateGlobMatcher(string fullPattern)
        {
            var normalizedPattern = NormalizePath(fullPattern);
            var stringBuilder = new StringBuilder("^");

            for (var index = 0; index < normalizedPattern.Length; index++)
            {
                var character = normalizedPattern[index];
                if (character == '*')
                {
                    if (index + 1 < normalizedPattern.Length && normalizedPattern[index + 1] == '*')
                    {
                        if (index + 2 < normalizedPattern.Length && normalizedPattern[index + 2] == '/')
                        {
                            stringBuilder.Append("(?:.*/)?");
                            index += 2;
                        }
                        else
                        {
                            stringBuilder.Append(".*");
                            index++;
                        }
                    }
                    else
                    {
                        stringBuilder.Append("[^/]*");
                    }

                    continue;
                }

                stringBuilder.Append(Regex.Escape(character.ToString()));
            }

            stringBuilder.Append('$');
            return new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private static string NormalizePath(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }

        internal sealed record InternalResxAnalyzerOptions(
            IReadOnlyList<NormalizedResourceGroup> ResourceGroups,
            string InvariantComment);

        internal sealed record NormalizedResourceGroup(
            FileInfo NeutralResourceFile,
            IReadOnlyDictionary<CultureInfo, FileInfo> ExpectedLocalizedResourceFiles,
            IReadOnlyDictionary<CultureInfo, FileInfo> DiscoveredLocalizedResourceFiles);

        private sealed record ResourceGroupAnalysisResult(
            string ResourceFilePath,
            IReadOnlyList<CheckRunResult> CheckRuns);

        private sealed record CheckRunResult(
            IResxCheck Check,
            ResxCheckResult Result);

    }
}
