namespace System.Resources.Tests.Checks
{
    public sealed class NewlineValueCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public NewlineValueCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "NewlineValueCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "NewlineValueCheck.de.resx"))
                .WithChecks(checks => checks.Clear().Add(new NewlineValueCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_NewlineValueCheck_ReturnsValuesStartingOrEndingWithNewlines()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation values are invalid:");
            result.Report.Should().Contain("CultureInfo neutral, Key='NeutralStartsWithNewline'");
            result.Report.Should().Contain("CultureInfo neutral, Key='NeutralEndsWithNewline'");
            result.Report.Should().Contain("CultureInfo de-CH, Key='LocalizedStartsWithNewline'");
            result.Report.Should().Contain("CultureInfo de-CH, Key='LocalizedEndsWithNewline'");
        }
    }
}
