namespace System.Resources.Tests.Checks
{
    public sealed class UnusedCultureFileCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public UnusedCultureFileCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "UnusedCultureFileCheck.resx"))
                .WithCultures([new CultureInfo("de-CH")])
                .WithChecks(checks => checks.Add(new UnusedCultureFileCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_UnusedCultureFileCheck_ReturnsDiscoveredCulturesOutsideConfiguredSet()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized resource files are not part of the configured culture set (1):");
            result.Report.Should().Contain("CultureInfo \"fr\"");
        }
    }
}
