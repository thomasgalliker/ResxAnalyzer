namespace System.Resources.Tests.Checks
{
    public sealed class CultureFileCoverageCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public CultureFileCoverageCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "CultureFileCoverageCheck.resx"))
                .WithCultures([new CultureInfo("de-CH"), new CultureInfo("fr-CH")])
                .WithChecks(checks => checks.Add(new CultureFileCoverageCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_CultureFileCoverageCheck_ReturnsMissingCultureFiles()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following configured culture resource files are missing (1):");
            result.Report.Should().Contain("CultureInfo \"fr-CH\"");
        }
    }
}
