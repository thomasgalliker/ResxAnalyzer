namespace System.Resources.Tests.Checks
{
    public sealed class DuplicateKeyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DuplicateKeyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_DuplicateKeyCheck_ReturnsDuplicateKeys()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("DuplicateKeyCheck", checks => checks.Add(new DuplicateKeyCheck()));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following resource keys are duplicated (1):");
            result.Report.Should().Contain("Key='DuplicateKey', Count=2");
        }
    }
}
