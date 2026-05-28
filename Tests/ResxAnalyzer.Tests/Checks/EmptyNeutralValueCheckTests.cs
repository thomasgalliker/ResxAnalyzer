namespace System.Resources.Tests.Checks
{
    public sealed class EmptyNeutralValueCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public EmptyNeutralValueCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "EmptyNeutralValueCheck.resx"))
                .WithChecks(checks => checks.Clear().Add(new EmptyNeutralValueCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_EmptyNeutralValueCheck_ReturnsEmptyNeutralValues()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following neutral resource values are empty (1):");
            result.Report.Should().Contain("> EmptyNeutralValue");
        }
    }
}
