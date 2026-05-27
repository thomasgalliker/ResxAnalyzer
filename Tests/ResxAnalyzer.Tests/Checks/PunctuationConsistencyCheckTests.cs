namespace System.Resources.Tests.Checks
{
    public sealed class PunctuationConsistencyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public PunctuationConsistencyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_PunctuationConsistencyCheck_ReturnsPunctuationMismatches()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("PunctuationConsistencyCheck", checks => checks.Add(new PunctuationConsistencyCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values have inconsistent terminal punctuation (1):");
            result.Report.Should().Contain("Key='QuestionText'");
        }
    }
}
