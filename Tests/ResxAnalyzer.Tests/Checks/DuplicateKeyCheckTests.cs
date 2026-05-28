namespace System.Resources.Tests.Checks
{
    public sealed class DuplicateKeyCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public DuplicateKeyCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "DuplicateKeyCheck.resx"))
                .WithChecks(checks => checks.Clear().Add(new DuplicateKeyCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_DuplicateKeyCheck_ReturnsDuplicateKeys()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following resource keys are duplicated (1):");
            result.Report.Should().Contain("Key='DuplicateKey', Count=2");
        }
    }
}
