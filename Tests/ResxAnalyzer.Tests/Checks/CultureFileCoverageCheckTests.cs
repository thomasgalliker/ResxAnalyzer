namespace System.Resources.Tests.Checks
{
    public sealed class CultureFileCoverageCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public CultureFileCoverageCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_CultureFileCoverageCheck_ReturnsMissingCultureFiles()
        {
            // Arrange
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            // Act
            var result = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "CultureFileCoverageCheck.resx"))
                .WithCultures([new CultureInfo("de-CH"), new CultureInfo("fr-CH")])
                .WithChecks(checks => checks.Add(new CultureFileCoverageCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following configured culture resource files are missing (1):");
            result.Report.Should().Contain("CultureInfo \"fr-CH\"");
        }
    }
}
