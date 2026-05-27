namespace System.Resources.Tests.Checks
{
    public sealed class PlaceholderConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public PlaceholderConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_PlaceholderConsistencyCheck_ReturnsNamedAndMalformedPlaceholderProblems()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("PlaceholderConsistencyCheck", checks => checks.Add(new PlaceholderConsistencyCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("NamedPlaceholder (neutral: {name}, localized: {firstName})");
            result.Report.Should().Contain("Following translation values contain malformed placeholders:");
            result.Report.Should().Contain("Key='MalformedPlaceholder'");
        }
    }
}
