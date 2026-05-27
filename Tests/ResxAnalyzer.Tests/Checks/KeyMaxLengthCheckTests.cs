namespace System.Resources.Tests.Checks
{
    public sealed class KeyMaxLengthCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public KeyMaxLengthCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "KeyMaxLengthCheck.resx"))
                .WithChecks(checks => checks.Add(new KeyMaxLengthCheck(10)))
                .Build();
        }

        [Fact]
        public void Analyze_KeyMaxLengthCheck_ReturnsLongKeys()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following resource keys exceed 10 characters (1):");
            result.Report.Should().Contain("> ThisKeyIsTooLong");
        }
    }
}
