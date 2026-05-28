namespace System.Resources.Tests.Checks
{
    public sealed class SuspiciousSameAsNeutralCheckTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ResxAnalyzer resxAnalyzer;

        public SuspiciousSameAsNeutralCheckTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var checksDirectory = TestDataPaths.GetChecksDirectory();

            this.resxAnalyzer = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, "SuspiciousSameAsNeutralCheck.resx"))
                .WithLocalizedResource(new CultureInfo("de-CH"), Path.Combine(checksDirectory.FullName, "SuspiciousSameAsNeutralCheck.de.resx"))
                .WithChecks(checks => checks.Clear().Add(new SuspiciousSameAsNeutralCheck()))
                .Build();
        }

        [Fact]
        public void Analyze_SuspiciousSameAsNeutralCheck_ReturnsCopiedLocalizedValues()
        {
            // Act
            var result = this.resxAnalyzer.Analyze();

            // Assert
            this.testOutputHelper.WriteLine(result.Report);
            result.Succeeded.Should().BeFalse();
            result.Report.Should().Contain("Following localized values are identical to neutral values (1):");
            result.Report.Should().Contain("Key='CopiedValue'");
        }
    }
}
