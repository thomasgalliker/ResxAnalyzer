namespace Superdev.ResxAnalyzer.Tests.Checks
{
    public sealed class SuspiciousSameAsNeutralCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SuspiciousSameAsNeutralCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_SuspiciousSameAsNeutralCheck_ReturnsCopiedLocalizedValues()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("SuspiciousSameAsNeutralCheck", checks => checks.Add(new SuspiciousSameAsNeutralCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values are identical to neutral values (1):");
            result.Report.Should().Contain("Key='CopiedValue'");
        }
    }
}
