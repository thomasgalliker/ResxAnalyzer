namespace System.Resources.Tests.Checks
{
    public sealed class OrphanLocalizedKeyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public OrphanLocalizedKeyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_OrphanLocalizedKeyCheck_ReturnsLocalizedKeysMissingFromNeutral()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("OrphanLocalizedKeyCheck", checks => checks.Add(new OrphanLocalizedKeyCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized keys do not exist in the neutral resource:");
            result.Report.Should().Contain("> OrphanLocalizedOnly");
        }
    }
}
