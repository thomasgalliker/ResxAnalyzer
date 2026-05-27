namespace System.Resources.Tests.Checks
{
    public sealed class CompletenessCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public CompletenessCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "CompletenessCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "CompletenessCheck.de.resx"))
                .WithChecks(checks => checks.Add(new CompletenessCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_CompletenessCheck_ReturnsMissingNonInvariantKeys()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following translation keys are missing:");
            result.Report.Should().Contain("CultureInfo \"de-CH\" (1):");
            result.Report.Should().Contain("> MissingNonInvariant");
            result.Report.Should().NotContain("> InvariantOnly");
        }
    }
}
