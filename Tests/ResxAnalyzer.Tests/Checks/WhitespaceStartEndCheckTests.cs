namespace System.Resources.Tests.Checks
{
    public sealed class WhitespaceStartEndCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public WhitespaceStartEndCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "WhitespaceStartEndCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "WhitespaceStartEndCheck.de.resx"))
                .WithChecks(checks => checks.Clear().Add(new WhitespaceStartEndCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_WhitespaceStartEndCheck_ReturnsValuesStartingOrEndingWithWhitespace()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation values start or end with whitespace (4):");
            result.Report.Should().Contain("CultureInfo neutral, Key='NeutralStartsWithWhitespace'");
            result.Report.Should().Contain("CultureInfo neutral, Key='NeutralEndsWithWhitespace'");
            result.Report.Should().Contain("CultureInfo de-CH, Key='LocalizedStartsWithWhitespace'");
            result.Report.Should().Contain("CultureInfo de-CH, Key='LocalizedEndsWithWhitespace'");
        }
    }
}
