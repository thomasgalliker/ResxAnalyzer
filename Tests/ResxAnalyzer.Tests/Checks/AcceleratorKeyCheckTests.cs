namespace System.Resources.Tests.Checks
{
    public sealed class AcceleratorKeyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public AcceleratorKeyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "AcceleratorKeyCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "AcceleratorKeyCheck.de.resx"))
                .WithChecks(checks => checks.Add(new AcceleratorKeyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_AcceleratorKeyCheck_ReturnsAcceleratorMismatches()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent accelerator markers (1):");
            result.Report.Should().Contain("Key='SaveCommand'");
        }
    }
}
