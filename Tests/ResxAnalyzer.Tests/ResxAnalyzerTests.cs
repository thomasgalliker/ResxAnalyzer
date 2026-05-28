namespace System.Resources.Tests
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
        public void Analyze_CulturesAreNotConfigured_DiscoversLocalizedSiblingResources()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            var resxAnalyzer = ResxAnalyzer
                .ForResource(neutralResourceFile)
                .Build();

            // Act
            var result = resxAnalyzer.Analyze<CompletenessCheck>();

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

            var resxAnalyzer = ResxAnalyzer
                .ForResource(resourceDirectory)
                .Build();

            // Act
            var result = resxAnalyzer.Analyze<CompletenessCheck>();

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

            var resxAnalyzer = ResxAnalyzer
                .ForResource(resourceDirectory)
                .Build();

            // Act
            var result = resxAnalyzer.Analyze<CompletenessCheck>();

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

            var resxAnalyzer = ResxAnalyzer
                .ForResource(resourceGlob)
                .Build();

            // Act
            var result = resxAnalyzer.Analyze<CompletenessCheck>();

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

            var resxAnalyzer = ResxAnalyzer
                .ForResource(neutralResourceFile)
                .WithCultures([GermanCultureInfo])
                .Build();

            // Act
            var result = resxAnalyzer.Analyze<CompletenessCheck>();

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
                .Build();

            // Act
            var result = resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Report.Should().NotContain("Following keys are not used");
        }

        [Fact]
        public void Build_ChecksAreNotConfigured_UsesDefaultBuiltInChecks()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            // Act
            var analyzer = ResxAnalyzer
                .ForResource(neutralResourceFile)
                .Build();

            // Assert
            var result = analyzer.Analyze<CompletenessCheck>();
            result.Checks.Should().ContainSingle(check => check.CheckName == nameof(CompletenessCheck));
        }

        [Fact]
        public void Build_AllChecksAreCleared_ThrowsInvalidOperationException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var neutralResourceFile = Path.Combine(testDataDirectory.FullName, "Resources", "Strings.resx");

            // Act
            var action = () => ResxAnalyzer
                .ForResource(neutralResourceFile)
                .WithChecks(checks => checks.Clear())
                .Build();

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("At least one .resx check must be registered.");
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
                .WithChecks(checks => checks.Add(new UnusedKeysCheck(scan => scan
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
        public void Analyze_GenericCheckIsConfigured_RunsOnlySelectedCheck()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var result = analyzer.Analyze<PlaceholderConsistencyCheck>();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Checks.Should().ContainSingle(check => check.CheckName == nameof(PlaceholderConsistencyCheck));
            result.Report.Should().Contain("Check \"PlaceholderConsistencyCheck\" succeeded");
            result.Report.Should().NotContain("CompletenessCheck");
            result.Report.Should().NotContain("NewlineValueCheck");
        }

        [Fact]
        public void Analyze_CheckTypeIsConfigured_RunsOnlySelectedCheck()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var result = analyzer.Analyze(typeof(NewlineValueCheck));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Checks.Should().ContainSingle(check => check.CheckName == nameof(NewlineValueCheck));
            result.Report.Should().Contain("Check \"NewlineValueCheck\" succeeded");
            result.Report.Should().NotContain("CompletenessCheck");
            result.Report.Should().NotContain("PlaceholderConsistencyCheck");
        }

        [Fact]
        public void Analyze_CheckTypesAreConfigured_RunsSelectedChecksInRequestedOrder()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var result = analyzer.Analyze(typeof(NewlineValueCheck), typeof(CompletenessCheck));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Checks.Select(check => check.CheckName).Should().Equal(nameof(NewlineValueCheck), nameof(CompletenessCheck));
            result.Report.IndexOf("NewlineValueCheck", StringComparison.Ordinal).Should().BeLessThan(result.Report.IndexOf("CompletenessCheck", StringComparison.Ordinal));
            result.Report.Should().NotContain("PlaceholderConsistencyCheck");
        }

        [Fact]
        public void Analyze_CheckNameIsConfigured_RunsOnlySelectedCheck()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var result = analyzer.Analyze(nameof(CompletenessCheck));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Checks.Should().ContainSingle(check => check.CheckName == nameof(CompletenessCheck));
            result.Report.Should().Contain("Check \"CompletenessCheck\" succeeded");
            result.Report.Should().NotContain("PlaceholderConsistencyCheck");
            result.Report.Should().NotContain("NewlineValueCheck");
        }

        [Fact]
        public void Analyze_CheckNamesAreConfigured_RunsSelectedChecksInRequestedOrder()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var result = analyzer.Analyze(nameof(NewlineValueCheck), nameof(CompletenessCheck));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Checks.Select(check => check.CheckName).Should().Equal(nameof(NewlineValueCheck), nameof(CompletenessCheck));
            result.Report.IndexOf("NewlineValueCheck", StringComparison.Ordinal).Should().BeLessThan(result.Report.IndexOf("CompletenessCheck", StringComparison.Ordinal));
            result.Report.Should().NotContain("PlaceholderConsistencyCheck");
        }

        [Fact]
        public void Analyze_NoSelector_RunsDefaultBuiltInChecks()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Checks.Select(check => check.CheckName).Should().Equal(
                nameof(CompletenessCheck),
                nameof(PlaceholderConsistencyCheck),
                nameof(DuplicateKeyCheck),
                nameof(EmptyNeutralValueCheck),
                nameof(SuspiciousSameAsNeutralCheck),
                nameof(WhitespaceConsistencyCheck),
                nameof(AcceleratorKeyCheck),
                nameof(PunctuationConsistencyCheck),
                nameof(OrphanLocalizedKeyCheck),
                nameof(CultureFileCoverageCheck),
                nameof(UnusedCultureFileCheck),
                nameof(NewlineValueCheck));
        }

        [Fact]
        public void Analyze_NoCheckSelectorIsProvided_RunsAllConfiguredChecksAndAggregatesFailures()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));
            var firstCheck = new FirstTrackingCheck();
            var secondCheck = new SecondTrackingCheck();

            var analyzer = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks
                    .Clear()
                    .Add(firstCheck)
                    .Add(secondCheck))
                .Build();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Checks.Select(check => check.CheckName).Should().Equal(nameof(FirstTrackingCheck), nameof(SecondTrackingCheck));
            firstCheck.AnalyzeCount.Should().Be(1);
            secondCheck.AnalyzeCount.Should().Be(1);
            result.Report.Should().Contain("Check \"FirstTrackingCheck\" succeeded");
            result.Report.Should().Contain("Check \"SecondTrackingCheck\" failed");
        }

        [Fact]
        public void Analyze_GenericCheckIsNotConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var action = () => analyzer.Analyze<KeyMaxLengthCheck>();

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No registered check of type KeyMaxLengthCheck was found.");
        }

        [Fact]
        public void Analyze_CheckTypeIsNotConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var action = () => analyzer.Analyze(typeof(KeyMaxLengthCheck));

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No registered check of type KeyMaxLengthCheck was found.");
        }

        [Fact]
        public void Analyze_CheckNameIsNotConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var analyzer = CreateAnalyzerForCleanResx();

            // Act
            var action = () => analyzer.Analyze(nameof(KeyMaxLengthCheck));

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No registered check named KeyMaxLengthCheck was found.");
        }

        [Fact]
        public void WithChecks_CheckTypeIsAlreadyConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var action = () => ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithChecks(checks => checks.Add(new CompletenessCheck()));

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("A check of type CompletenessCheck is already configured. Each check type can only be registered once.");
        }

        [Fact]
        public void WithChecks_DefaultCheckIsRemoved_DoesNotRunRemovedCheck()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            var analyzer = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks.Remove<UnusedCultureFileCheck>())
                .Build();

            // Act
            var result = analyzer.Analyze();

            // Assert
            result.Checks.Select(check => check.CheckName).Should().NotContain(nameof(UnusedCultureFileCheck));
        }

        [Fact]
        public void WithChecks_CheckTypeIsRemoved_RemovesRegisteredCheck()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            var analyzer = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks.Remove(typeof(PunctuationConsistencyCheck)))
                .Build();

            // Act
            var result = analyzer.Analyze();

            // Assert
            result.Checks.Select(check => check.CheckName).Should().NotContain(nameof(PunctuationConsistencyCheck));
        }

        [Fact]
        public void WithChecks_CheckNameIsRemoved_RemovesRegisteredCheck()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            var analyzer = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks.Remove(nameof(WhitespaceConsistencyCheck)))
                .Build();

            // Act
            var result = analyzer.Analyze();

            // Assert
            result.Checks.Select(check => check.CheckName).Should().NotContain(nameof(WhitespaceConsistencyCheck));
        }

        [Fact]
        public void WithChecks_ConfigurableBuiltInCheckIsAdded_CanRunSelectedCheck()
        {
            // Arrange
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            var analyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "KeyMaxLengthCheck.resx"))
                .WithChecks(checks => checks.Add(new KeyMaxLengthCheck(10)))
                .Build();

            // Act
            var result = analyzer.Analyze<KeyMaxLengthCheck>();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Checks.Should().ContainSingle(check => check.CheckName == nameof(KeyMaxLengthCheck));
            result.Report.Should().Contain("Following resource keys exceed 10 characters");
        }

        [Fact]
        public void WithChecks_UnregisteredCheckIsRemoved_ThrowsInvalidOperationException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var action = () => ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithChecks(checks => checks.Remove<KeyMaxLengthCheck>());

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No registered check of type KeyMaxLengthCheck was found.");
        }

        [Fact]
        public void WithChecks_CheckNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var action = () => ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithChecks(checks => checks.Remove(" "));

            // Assert
            action.Should()
                .Throw<ArgumentException>()
                .WithMessage("Check name must not be empty. (Parameter 'checkName')");
        }

        [Fact]
        public void WithChecks_CheckTypeDoesNotImplementIResxCheck_ThrowsArgumentException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var action = () => ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithChecks(checks => checks.Remove(typeof(string)));

            // Assert
            action.Should()
                .Throw<ArgumentException>()
                .WithMessage("Check type String must implement IResxCheck. (Parameter 'checkType')");
        }

        [Fact]
        public void WithChecks_CheckIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var action = () => ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithChecks(checks => checks.Add(null!));

            // Assert
            action.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("check");
        }

        [Fact]
        public void WithChecks_CheckIsClearedAndCustomCheckAdded_RunsOnlyCustomCheck()
        {
            // Arrange
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            // Act
            var result = ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
                .WithChecks(checks => checks.Clear().Add(new CustomFailingCheck()))
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

        private static ResxAnalyzer CreateAnalyzerForCleanResx()
        {
            var testDataDirectory = TestDataPaths.GetTestDataDirectory();
            var resourceDirectory = new DirectoryInfo(Path.Combine(testDataDirectory.FullName, "Resources"));

            return ResxAnalyzer
                .ForResource(Path.Combine(resourceDirectory.FullName, "Clean.resx"))
                .WithLocalizedResource(GermanCultureInfo, Path.Combine(resourceDirectory.FullName, "Clean.de.resx"))
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

        private sealed class FirstTrackingCheck : IResxCheck
        {
            public int AnalyzeCount { get; private set; }

            public string Description => "First tracking check used by analyzer tests.";

            public ResxCheckResult Analyze(ResxAnalysisContext context)
            {
                this.AnalyzeCount++;
                return new ResxCheckResult(true, string.Empty);
            }
        }

        private sealed class SecondTrackingCheck : IResxCheck
        {
            public int AnalyzeCount { get; private set; }

            public string Description => "Second tracking check used by analyzer tests.";

            public ResxCheckResult Analyze(ResxAnalysisContext context)
            {
                this.AnalyzeCount++;
                return new ResxCheckResult(false, "Second tracking check failed.");
            }
        }
    }
}
