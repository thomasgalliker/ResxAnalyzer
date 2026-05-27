namespace System.Resources.Tests.Checks
{
    public sealed class OrphanLocalizedKeyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public OrphanLocalizedKeyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "OrphanLocalizedKeyCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "OrphanLocalizedKeyCheck.de.resx"))
                .WithChecks(checks => checks.Add(new OrphanLocalizedKeyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_OrphanLocalizedKeyCheck_ReturnsLocalizedKeysMissingFromNeutral()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized keys do not exist in the neutral resource:");
            result.Report.Should().Contain("> OrphanLocalizedOnly");
        }
    }
}
