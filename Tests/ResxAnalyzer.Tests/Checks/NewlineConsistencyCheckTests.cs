namespace System.Resources.Tests.Checks
{
    public sealed class NewlineConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public NewlineConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "NewlineConsistencyCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "NewlineConsistencyCheck.de.resx"))
                .WithChecks(checks => checks.Clear().Add(new NewlineConsistencyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_NewlineConsistencyCheck_ReturnsNewlineMismatches()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent leading or trailing newlines (3):");
            result.Report.Should().Contain("Key='LeadingNewlineValue'");
            result.Report.Should().Contain("Key='TrailingNewlineValue'");
            result.Report.Should().Contain("Key='LocalizedTrailingNewlineValue'");
        }
    }
}
