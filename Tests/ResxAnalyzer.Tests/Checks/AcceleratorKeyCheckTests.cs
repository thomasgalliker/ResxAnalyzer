namespace Superdev.ResxAnalyzer.Tests.Checks
{
    public sealed class AcceleratorKeyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public AcceleratorKeyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_AcceleratorKeyCheck_ReturnsAcceleratorMismatches()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("AcceleratorKeyCheck", checks => checks.Add(new AcceleratorKeyCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent accelerator markers (1):");
            result.Report.Should().Contain("Key='SaveCommand'");
        }
    }
}
