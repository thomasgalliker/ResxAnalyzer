namespace System.Resources.Tests.Checks
{
    public sealed class KeyMaxLengthCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public KeyMaxLengthCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Analyze_KeyMaxLengthCheck_ReturnsLongKeys()
        {
            // Act
            var result = TestDataPaths.AnalyzeCheck("KeyMaxLengthCheck", checks => checks.Add(new KeyMaxLengthCheck(10)));

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following resource keys exceed 10 characters (1):");
            result.Report.Should().Contain("> ThisKeyIsTooLong");
        }
    }
}
