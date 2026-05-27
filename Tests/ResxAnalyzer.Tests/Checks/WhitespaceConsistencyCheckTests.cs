namespace Superdev.ResxAnalyzer.Tests.Checks
{
    public sealed class WhitespaceConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public WhitespaceConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_WhitespaceConsistencyCheck_ReturnsWhitespaceMismatches()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("WhitespaceConsistencyCheck", checks => checks.Add(new WhitespaceConsistencyCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent leading or trailing whitespace (1):");
            result.Report.Should().Contain("Key='WhitespaceValue'");
        }
    }
}
