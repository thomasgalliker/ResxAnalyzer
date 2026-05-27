namespace System.Resources.Tests.Checks
{
    public sealed class PunctuationConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public PunctuationConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "PunctuationConsistencyCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "PunctuationConsistencyCheck.de.resx"))
                .WithChecks(checks => checks.Add(new PunctuationConsistencyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_PunctuationConsistencyCheck_ReturnsPunctuationMismatches()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent terminal punctuation (1):");
            result.Report.Should().Contain("Key='QuestionText'");
        }
    }
}
