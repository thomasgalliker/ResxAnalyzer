namespace Superdev.ResxAnalyzer.Tests.Checks
{
    public sealed class UnusedCultureFileCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public UnusedCultureFileCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_UnusedCultureFileCheck_ReturnsDiscoveredCulturesOutsideConfiguredSet()
        {
            // Arrange
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            // Act
            var result = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "UnusedCultureFileCheck.resx"))
                .WithCultures([new CultureInfo("de-CH")])
                .WithChecks(checks => checks.Add(new UnusedCultureFileCheck()))
                .Build()
                .Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized resource files are not part of the configured culture set (1):");
            result.Report.Should().Contain("CultureInfo \"fr\"");
        }
    }
}
