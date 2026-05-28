namespace System.Resources.Tests.Checks
{
    public sealed class NewlineStartEndCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public NewlineStartEndCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "NewlineStartEndCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "NewlineStartEndCheck.de.resx"))
                .WithChecks(checks => checks.Clear().Add(new NewlineStartEndCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_NewlineStartEndCheck_ReturnsValuesStartingOrEndingWithNewlines()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation values start or end with newlines (4):");
            result.Report.Should().Contain("CultureInfo neutral, Key='NeutralStartsWithNewline'");
            result.Report.Should().Contain("CultureInfo neutral, Key='NeutralEndsWithNewline'");
            result.Report.Should().Contain("CultureInfo de-CH, Key='LocalizedStartsWithNewline'");
            result.Report.Should().Contain("CultureInfo de-CH, Key='LocalizedEndsWithNewline'");
        }
    }
}
