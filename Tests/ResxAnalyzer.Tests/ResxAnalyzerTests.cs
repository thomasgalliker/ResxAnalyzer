namespace Superdev.ResxAnalyzer.Tests
{
    public class ResxAnalyzerTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private static readonly CultureInfo GermanCultureInfo = new("de-CH");

        public ResxAnalyzerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_MissingLocalizedKeyIsNotInvariant_ReturnsMissingTranslation()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation keys are missing:");
            result.Report.Should().Contain("CultureInfo \"de-CH\" (1):");
            result.Report.Should().Contain("> MissingNonInvariant");
        }

        [Fact]
        public void Analyze_MissingLocalizedKeyIsInvariant_DoesNotReturnMissingTranslation()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Report.Should().NotContain("> InvariantOnly");
        }

        [Fact]
        public void Analyze_LocalizedPlaceholderTokensDiffer_ReturnsPlaceholderMismatch()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation keys have inconsistent placeholders:");
            result.Report.Should().Contain("PlaceholderMessage (neutral: {0}, {1:N2}, localized: {0})");
        }

        [Fact]
        public void Analyze_ValuesHaveLeadingOrTrailingNewlines_ReturnsInvalidValues()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation values are invalid:");
            result.Report.Should().Contain("Key='NewlineStart'");
            result.Report.Should().Contain("Key='NewlineEnd'");
        }

        [Fact]
        public void Analyze_KeyAppearsOnlyInDesignerFile_ReturnsUnusedKey()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following keys are not used");
            result.Report.Should().Contain("> DesignerOnlyKey");
        }

        [Fact]
        public void Analyze_KeyMatchesIgnoredUsagePattern_DoesNotReturnUnusedKey()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Report.Should().NotContain("> IgnoredDynamic_Key");
        }

        [Fact]
        public void Analyze_KeyAppearsInSource_DoesNotReturnUnusedKey()
        {
            // Arrange
            var analyzer = CreateAnalyzer();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Report.Should().NotContain("> UsedKey");
            result.Report.Should().NotContain("> XamlUsedKey");
        }

        [Fact]
        public void Analyze_CulturesAreNotConfigured_DiscoversLocalizedSiblingResources()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            // Act
            var result = ResxAnalyzer
                .ForResource(neutralResourceFile)
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("CultureInfo \"de\" (1):");
        }

        [Fact]
        public void Analyze_ResourceDirectoryIsConfigured_UsesStringsResxFromDirectory()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = Path.Combine(testDataDirectory.FullName, "Resources");

            // Act
            var result = ResxAnalyzer
                .ForResource(resourceDirectory)
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("CultureInfo \"de\" (1):");
        }

        [Fact]
        public void Analyze_ResourceDirectoryContainsMultipleResourceGroups_AnalyzesAllNeutralResources()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = Path.Combine(testDataDirectory.FullName, "Resources");

            // Act
            var result = ResxAnalyzer
                .ForResource(resourceDirectory)
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Report.Should().Contain("Resource \"" + Path.Combine(resourceDirectory, "Clean.resx"));
            result.Report.Should().Contain("Resource \"" + Path.Combine(resourceDirectory, "Strings.resx"));
            result.Report.Should().NotContain("Resource \"" + Path.Combine(resourceDirectory, "Clean.de.resx"));
            result.Report.Should().NotContain("Resource \"" + Path.Combine(resourceDirectory, "Strings.de.resx"));
        }

        [Fact]
        public void Analyze_RecursiveGlobIsConfigured_AnalyzesNeutralResourcesInSubfolders()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceGlob = Path.Combine(testDataDirectory.FullName, "Resources", "**", "Nested*.resx");

            // Act
            var result = ResxAnalyzer
                .ForResource(resourceGlob)
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("> NestedMissing");
            result.Report.Should().NotContain("Nested.de.resx");
        }

        [Fact]
        public void Analyze_CulturesAreConfigured_InfersLocalizedResourceFromParentCulture()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            // Act
            var result = ResxAnalyzer
                .ForResource(neutralResourceFile)
                .WithCultures([GermanCultureInfo])
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("CultureInfo \"de-CH\" (1):");
        }

        [Fact]
        public void Analyze_UnusedKeysCheckIsNotConfigured_DoesNotReturnUnusedKeys()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            var resxAnalyzer = ResxAnalyzer
                .ForResource(neutralResourceFile)
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(testDataDirectory.FullName, "Resources", "Strings.de.resx"))
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build();

            // Act
            var result = resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Report.Should().NotContain("Following keys are not used");
        }

        [Fact]
        public void Build_ChecksAreNotConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            // Act
            var action = () => ResxAnalyzer
                .ForResource(neutralResourceFile)
                .Build();

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("At least one .resx check must be configured with WithChecks(...).");
        }

        [Fact]
        public void Analyze_OptionsCanRoundTripThroughJson()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));
            var options = new ResxAnalyzerOptions
            {
                ResourcePath = Path.Combine(resourceDirectory.FullName, "Strings.resx"),
                LocalizedResourceFiles =
                {
                    [GermanCultureInfo.Name] = Path.Combine(resourceDirectory.FullName, "Strings.de.resx")
                }
            };

            // Act
            var json = JsonSerializer.Serialize(options);
            var deserializedOptions = JsonSerializer.Deserialize<ResxAnalyzerOptions>(json)!;
            var result = ResxAnalyzer
                .ForOptions(deserializedOptions)
                .WithChecks(checks => checks
                    .Add(new CompletenessCheck())
                    .Add(new UnusedKeysCheck(scan => scan
                        .In(Path.Combine(testDataDirectory.FullName, "Source"))
                        .IgnoreKeys("^IgnoredDynamic_"))))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            deserializedOptions.ResourcePath.Should().Be(options.ResourcePath);
            deserializedOptions.LocalizedResourceFiles[GermanCultureInfo.Name].Should().Be(options.LocalizedResourceFiles[GermanCultureInfo.Name]);
            result.Report.Should().Contain("> MissingNonInvariant");
            result.Report.Should().NotContain("> IgnoredDynamic_Key");
        }

        [Fact]
        public void Analyze_NoProblemsFound_ReturnsSuccessfulResult()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var result = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks
                    .Add(new CompletenessCheck())
                    .Add(new PlaceholderConsistencyCheck())
                    .Add(new NewlineValueCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Report.Should().Contain("Check \"CompletenessCheck\" succeeded");
            result.Report.Should().Contain("Checks that every non-invariant resource key exists with a non-empty value in every localized resource.");
            result.Report.Should().Contain("Check \"PlaceholderConsistencyCheck\" succeeded");
            result.Report.Should().Contain("Checks that translated values use the same placeholder tokens as the neutral resource.");
            result.Report.Should().Contain("Check \"NewlineValueCheck\" succeeded");
            result.Report.Should().Contain("Checks that resource values do not start or end with newlines.");
            result.ToString().Should().Be(result.Report);
        }

        [Fact]
        public void Analyze_CustomCheckIsConfigured_RunsCustomCheck()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var result = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks.Add(new CustomFailingCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Checks.Should().ContainSingle(check => check.CheckName == nameof(CustomFailingCheck));
            result.Report.Should().Be(
                "Check \"CustomFailingCheck\" failed" + Environment.NewLine +
                "Custom check used by analyzer tests." + Environment.NewLine +
                "Custom check saw 1 neutral entries.");
        }

        private static ResxAnalyzer CreateAnalyzer()
        {
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            return ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Strings.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Strings.de.resx"))
                .WithChecks(checks => checks
                    .Add(new CompletenessCheck())
                    .Add(new PlaceholderConsistencyCheck())
                    .Add(new NewlineValueCheck())
                    .Add(new UnusedKeysCheck(scan => scan
                        .In(Path.Combine(testDataDirectory.FullName, "Source"))
                        .IgnoreKeys("^IgnoredDynamic_"))))
                .Build();
        }

        private sealed class CustomFailingCheck : IResxCheck
        {
            public string Description => "Custom check used by analyzer tests.";

            public ResxCheckResult Analyze(ResxAnalysisContext context)
            {
                return new ResxCheckResult(false, $"Custom check saw {context.NeutralResource.Entries.Count} neutral entries.");
            }
        }
    }
}
