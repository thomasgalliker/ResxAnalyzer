namespace System.Resources.Tests.Checks
{
    public sealed class UnusedKeysCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public UnusedKeysCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();
            var sourceDirectory = Path.Combine(TestDataPaths.GetTestDataDirectory().FullName, "Source");

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "UnusedKeysCheck.resx"))
                .WithChecks(checks => checks.Add(new UnusedKeysCheck(scan => scan
                    .In(sourceDirectory)
                    .IgnoreKeys("^IgnoredDynamic_"))))
                .Build();
        }

        [Fact]
        public void Analyze_UnusedKeysCheck_ReturnsKeysMissingFromUsageFiles()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following keys are not used (2):");
            result.Report.Should().Contain("> DesignerOnlyKey");
            result.Report.Should().Contain("> UnusedKey");
            result.Report.Should().NotContain("> IgnoredDynamic_Key");
            result.Report.Should().NotContain("> UsedKey");
            result.Report.Should().NotContain("> XamlUsedKey");
        }

        [Fact]
        public void Analyze_SearchDirectoriesAreNotConfigured_ReturnsSuccess()
        {
            // Arrange
            var checksDirectory = TestDataPaths.GetChecksDirectory();
            var analyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "UnusedKeysCheck.resx"))
                .WithChecks(checks => checks.Add(new UnusedKeysCheck()))
                .Build();

            // Act
            var result = analyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeTrue();
            result.Report.Should().NotContain("Following keys are not used");
        }
    }
}
