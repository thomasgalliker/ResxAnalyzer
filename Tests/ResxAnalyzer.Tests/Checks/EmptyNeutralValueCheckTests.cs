namespace System.Resources.Tests.Checks
{
    public sealed class EmptyNeutralValueCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public EmptyNeutralValueCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_EmptyNeutralValueCheck_ReturnsEmptyNeutralValues()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("EmptyNeutralValueCheck", checks => checks.Add(new EmptyNeutralValueCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following neutral resource values are empty (1):");
            result.Report.Should().Contain("> EmptyNeutralValue");
        }
    }
}
