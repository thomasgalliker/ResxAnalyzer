namespace System.Resources.Tests.Checks
{
    public sealed class PlaceholderConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public PlaceholderConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "PlaceholderConsistencyCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "PlaceholderConsistencyCheck.de.resx"))
                .WithChecks(checks => checks.Add(new PlaceholderConsistencyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_PlaceholderConsistencyCheck_ReturnsNamedAndMalformedPlaceholderProblems()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("NamedPlaceholder (neutral: {name}, localized: {firstName})");
            result.Report.Should().Contain("Following translation values contain malformed placeholders:");
            result.Report.Should().Contain("Key='MalformedPlaceholder'");
        }
    }
}
