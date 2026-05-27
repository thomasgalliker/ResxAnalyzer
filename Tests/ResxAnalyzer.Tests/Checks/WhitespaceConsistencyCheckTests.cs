namespace System.Resources.Tests.Checks
{
    public sealed class WhitespaceConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public WhitespaceConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "WhitespaceConsistencyCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "WhitespaceConsistencyCheck.de.resx"))
                .WithChecks(checks => checks.Add(new WhitespaceConsistencyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_WhitespaceConsistencyCheck_ReturnsWhitespaceMismatches()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent leading or trailing whitespace (1):");
            result.Report.Should().Contain("Key='WhitespaceValue'");
        }
    }
}
